using Dapper;
using MobileWebApi.Data;
using MobileWebApi.Models;
using MobileWebApi.Constants;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace MobileWebApi.Repositories
{
    public class LocationRepository
    {
        private readonly DapperContext _context;
        private readonly ILogger<LocationRepository> _logger;

        public LocationRepository(DapperContext context, ILogger<LocationRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<LocationResponse>> GetLocationsAsync(int? userId, int? organizationId, int? branchId)
        {
            const string query = @"
                SELECT 
                    l.Id AS Id,
                    l.LocationName,
                    l.Latitude,
                    l.Longitude,
                    l.RadiusMeters,
                    b.BranchName,
                    o.Company_Name
                FROM Location l
                INNER JOIN Branch b ON l.BranchId = b.Id
                INNER JOIN Organization o ON l.OrganizationId = o.Id
                INNER JOIN Users u ON u.OrganizationId = o.Id AND u.BranchId = b.Id
                WHERE 
                    (@UserId IS NULL OR u.UserId = @UserId)
                    AND (@OrganizationId IS NULL OR o.Id = @OrganizationId)
                    AND (@BranchId IS NULL OR b.Id = @BranchId);";

            try
            {
                using var connection = _context.CreateConnection();
                _logger.LogInformation(LogMessages.Location.ExecutingGetLocationsAsync,
                    userId, organizationId, branchId);

                var locations = await connection.QueryAsync<LocationResponse>(query, new
                {
                    UserId = userId,
                    OrganizationId = organizationId,
                    BranchId = branchId
                });

                _logger.LogInformation(LogMessages.Location.FetchedLocationsCount, locations.AsList().Count);
                return locations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Location.ErrorFetchingLocationsForUserId, userId);
                throw;
            }
        }
    }
}

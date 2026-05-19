using Dapper;
using Microsoft.EntityFrameworkCore;
using MobileWebApi.Constants;
using MobileWebApi.Data;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Resources;
using System.Data;

namespace MobileWebApi.Repositories
{
	public class GeoTenantLocationRepository : IGeoTenantLocationRepository
	{
		private readonly DapperContext _context;
		private readonly ILogger<GeoTenantLocationRepository> _logger;
		private readonly QueryProvider _queryProvider; private readonly IDbConnection _connection;

		public GeoTenantLocationRepository(DapperContext context, ILogger<GeoTenantLocationRepository> logger, QueryProvider queryProvider)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_logger = (ILogger<GeoTenantLocationRepository>?)(logger ?? throw new ArgumentNullException(nameof(logger)));
			_queryProvider = queryProvider;
		}

		public async Task<GeoTenantLocationRow> GetActiveByTenantIdAsync(int organisationId)
		{
			try
			{
				string query = _queryProvider.Get("GetGeoTenantLocationByTenantId");

				using var connection = _context.CreateConnection();
				return await connection.QueryFirstOrDefaultAsync<GeoTenantLocationRow>(
					query,
					new { TenantId = organisationId, IsActive=true }
				);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, LogMessages.User.ErrorFetchingTenantConfigurationByOrganisationId);
				throw;
			}

			
		}
	}
}
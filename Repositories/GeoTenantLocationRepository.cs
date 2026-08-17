using Dapper;
using MobileWebApi.Constants;
using MobileWebApi.Data;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Resources;

namespace MobileWebApi.Repositories
{
	public class GeoTenantLocationRepository : IGeoTenantLocationRepository
	{
		private readonly DapperContext _context;
		private readonly ILogger<GeoTenantLocationRepository> _logger;
		private readonly QueryProvider _queryProvider;

		public GeoTenantLocationRepository(DapperContext context, ILogger<GeoTenantLocationRepository> logger, QueryProvider queryProvider)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_queryProvider = queryProvider ?? throw new ArgumentNullException(nameof(queryProvider));
		}

		public async Task<GeoTenantLocationRow?> GetActiveByTenantAndBranchAsync(int organisationId, int branchId)
		{
			try
			{
				string query = _queryProvider.Get("GetGeoTenantLocationByTenantId");

				using var connection = _context.CreateConnection();
				return await connection.QueryFirstOrDefaultAsync<GeoTenantLocationRow>(
					query,
					new { TenantId = organisationId, BranchId = branchId });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, LogMessages.User.ErrorFetchingTenantConfigurationByOrganisationId);
				throw;
			}
		}
	}
}

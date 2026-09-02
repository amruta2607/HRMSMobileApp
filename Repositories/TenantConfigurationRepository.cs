using Dapper;
using MobileWebApi.Constants;
using MobileWebApi.Data;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Resources;
using Serilog;
using System;
using System.Linq;

namespace MobileWebApi.Repositories
{
	public class TenantConfigurationRepository : ITenantConfigurationRepository
	{
		private readonly DapperContext _context;
		private readonly ILogger<TenantConfigurationRepository> _logger;
		private readonly QueryProvider _queryProvider;

		public TenantConfigurationRepository(DapperContext context, ILogger<TenantConfigurationRepository> logger, QueryProvider queryProvider)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_queryProvider = queryProvider;
		}

		public async Task<TenantConfiguration> GetByTenantIdAsync(
	int organisationId,
	int? branchId)
		{
			try
			{
				string query = _queryProvider.Get("GetByTenantId");

				using var connection = _context.CreateConnection();

				return await connection.QueryFirstOrDefaultAsync<TenantConfiguration>(
					query,
					new
					{
						TenantId = organisationId,
						BranchId = branchId
					});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex,
					LogMessages.User.ErrorFetchingTenantConfigurationByOrganisationId);

				throw;
			}
		}

		public async Task<TenantConfigurationRow?> GetTenantConfigurationRowByTenantIdAsync(int tenantId)
		{
			try
			{
				string query = _queryProvider.Get("GetTenantConfigurationRowByTenantId");

				using var connection = _context.CreateConnection();
				return await connection.QueryFirstOrDefaultAsync<TenantConfigurationRow>(
					query,
					new { TenantId = tenantId }
				);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, LogMessages.User.ErrorFetchingTenantConfigurationByOrganisationId);
				throw;
			}
		}

		public async Task<TenantConfigurationRow?> GetAttendanceTenantConfigurationByTenantIdAsync(int tenantId)
		{
			try
			{
				string query = _queryProvider.Get("GetAttendanceTenantConfigurationByTenantId");

				using var connection = _context.CreateConnection();
				return await connection.QueryFirstOrDefaultAsync<TenantConfigurationRow>(
					query,
					new { TenantId = tenantId }
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

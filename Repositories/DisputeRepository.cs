using Dapper;
using MobileWebApi.Data;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Resources;
using MobileWebApi.Constants;

namespace MobileWebApi.Repositories
{
    public class DisputeRepository : IDisputeRepository
    {
        private readonly DapperContext _context;
        private readonly ILogger<DisputeRepository> _logger;
        private readonly QueryProvider _queryProvider;

        public DisputeRepository(
            DapperContext context, 
            ILogger<DisputeRepository> logger, 
            QueryProvider queryProvider)
        {
            _context = context;
            _logger = logger;
            _queryProvider = queryProvider;
        }

        public async Task<IEnumerable<DisputeCategory>> GetDisputeCategoriesAsync()
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetDisputeCategories");

                return await conn.QueryAsync<DisputeCategory>(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetDisputeCategoriesAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.DisputeGetDisputeCategoriesDatabaseError}: Failed to fetch dispute categories",
                    ex);
            }
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int employeeId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetEmployeeById");

                return await conn.QueryFirstOrDefaultAsync<Employee>(query, new { Id = employeeId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetEmployeeByIdAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.DisputeGetEmployeeByIdDatabaseError}: Failed to fetch employee by id for dispute",
                    ex);
            }
        }

        public async Task<EmployeeDispute?> GetExistingDisputeAsync(int employeeId, DateTime disputeDate)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetExistingDispute");

                return await conn.QueryFirstOrDefaultAsync<EmployeeDispute>(query,
                    new
                    {
                        EmployeeId = employeeId,
                        DisputeDate = disputeDate.Date
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetExistingDisputeAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.DisputeGetExistingDisputeDatabaseError}: Failed to fetch existing dispute",
                    ex);
            }
        }

        public async Task<int> InsertDisputeAsync(EmployeeDispute dispute)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("InsertDispute");

                return await conn.ExecuteScalarAsync<int>(query,
                    new
                    {
                        EmployeeId = dispute.EmployeeId,
                        DisputeCategoryId = dispute.DisputeCategoryId,
                        DisputeDate = dispute.DisputeDate.Date,
                        Description = dispute.Description,
                        Status = dispute.Status,
                        CreatedOn = dispute.CreatedOn
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(InsertDisputeAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.DisputeInsertDisputeDatabaseError}: Failed to insert dispute",
                    ex);
            }
        }
    }
}


using Dapper;
using MobileWebApi.Data;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Resources;

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
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetDisputeCategories");
            
            return await conn.QueryAsync<DisputeCategory>(query);
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int employeeId)
        {
            using var conn = _context.CreateConnection();
            string query = _queryProvider.Get("GetEmployeeById");
            
            return await conn.QueryFirstOrDefaultAsync<Employee>(query, new { Id = employeeId });
        }

        public async Task<EmployeeDispute?> GetExistingDisputeAsync(int employeeId, DateTime disputeDate)
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

        public async Task<int> InsertDisputeAsync(EmployeeDispute dispute)
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
    }
}


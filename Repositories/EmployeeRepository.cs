using Dapper;
using MobileWebApi.Data;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Resources;

namespace MobileWebApi.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly DapperContext _context;
        private readonly QueryProvider _queries;

        public EmployeeRepository(DapperContext context, QueryProvider queries)
        {
            _context = context;
            _queries = queries;
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            string query = _queries.Get("GetEmployeeById");
            return await connection.QueryFirstOrDefaultAsync<Employee>(query, new { Id = id });
        }

        public async Task<EmployeePersonalDetailsQueryResult?> GetEmployeePersonalDetailsByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            string query = _queries.Get("GetEmployeePersonalDetailsById");
            return await connection.QueryFirstOrDefaultAsync<EmployeePersonalDetailsQueryResult>(query, new { Id = id });
        }

        public async Task<Employee?> GetEmployeebyUserIdAsync(int systemUserId)
        {
            using var connection = _context.CreateConnection();
            string query = _queries.Get("GetEmployeeByUserId");
            return await connection.QueryFirstOrDefaultAsync<Employee>(query, new { SystemUserId = systemUserId });
        }

        public async Task<Employee?> GetEmployeeByPhoneAsync(string phone)
        {
            using var connection = _context.CreateConnection();
            string query = _queries.Get("GetEmployeeByPhone");
            return await connection.QueryFirstOrDefaultAsync<Employee>(query, new { Phone = phone });
        }

        public async Task<Employee?> GetEmployeeByEmployeeNumberAsync(string employeeNumber)
        {
            using var connection = _context.CreateConnection();
            string query = _queries.Get("GetEmployeeByEmployeeNumber");
            return await connection.QueryFirstOrDefaultAsync<Employee>(query, new { EmployeeNumber = employeeNumber });
        }

        public async Task<Employee> AddEmployeeAsync(Employee employee)
        {
            string query = _queries.Get("AddEmployee");
            using var connection = _context.CreateConnection();
            employee.Id = await connection.ExecuteScalarAsync<int>(query, employee);
            return employee;
        }

        public async Task<bool> UpdateEmployeeAsync(Employee employee)
        {
            using var connection = _context.CreateConnection();
            string query = _queries.Get("UpdateEmployee");
            var rowsAffected = await connection.ExecuteAsync(query, employee);
            return rowsAffected > 0;
        }

        public async Task<bool> UpdateEmployeePhoneAndPictureAsync(int employeeId, string? phone, string? picture)
        {
            using var connection = _context.CreateConnection();
            string query = _queries.Get("UpdateEmployeePhoneAndPicture");
            var rowsAffected = await connection.ExecuteAsync(query, new { Id = employeeId, Phone = phone, Picture = picture });
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            using var connection = _context.CreateConnection();
            string query = _queries.Get("DeleteEmployee");
            var rowsAffected = await connection.ExecuteAsync(query, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<bool> DeactivateEmployeeAsync(int id)
        {
            using var connection = _context.CreateConnection();
            string query = _queries.Get("DeactivateEmployee");
            var rowsAffected = await connection.ExecuteAsync(query, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByBranchAsync(int branchId)
        {
            string query = _queries.Get("GetEmployeesByBranchId");
            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Employee>(query, new { BranchId = branchId });
        }

        public async Task<int?> GetOrganisationIdByNameAsync(string organisationName)
            => await GetIdByNameAsync("GetOrganisationIdByName", "TenantName", organisationName);

        public async Task<int?> GetDesignationIdByNameAsync(string jobTitleName)
            => await GetIdByNameAsync("GetDesignationIdByName", "Name", jobTitleName);

        public async Task<int?> GetBranchIdByNameAsync(string branchName)
            => await GetIdByNameAsync("GetBranchIdByName", "Name", branchName);

        public async Task<int?> GetBloodgroupIdByNameAsync(string bloodGroupName)
           => await GetIdByNameAsync("GetBloodgroupIdByName", "BloodgroupName", bloodGroupName);

        public async Task<int?> GetDepartmentIdByNameAsync(string departmentName)
            => await GetIdByNameAsync("GetDepartmentIdByName", "Name", departmentName);

        public async Task<int?> GetGenderIdByNameAsync(string genderName)
            => await GetIdByNameAsync("GetGenderIdByName", "Name", genderName);

        public async Task<int?> GetMaritalStatusIdByNameAsync(string maritalStatusName)
           => await GetIdByNameAsync("GetMaritalStatusIdByName", "MaritalStatusName", maritalStatusName);

        public async Task<int?> GetStateIdByNameAsync(string stateName)
          => await GetIdByNameAsync("GetStateIdByName", "Name", stateName);

        public async Task<int?> GetCountryIdByNameAsync(string countryName)
           => await GetIdByNameAsync("GetCountryIdByName", "Name", countryName);

        public async Task<IEnumerable<Employee>> GetEmployeesbyOrganisationIdAsync(int organisationId)
        {
            using var connection = _context.CreateConnection();
            string query = _queries.Get("GetEmployeesByOrganisationId");
            return await connection.QueryAsync<Employee>(query, new { OrganisationId = organisationId });
        }

        private async Task<int?> GetIdByNameAsync(string queryKey, string paramKey, string paramValue)
        {
            using var connection = _context.CreateConnection();
            string query = _queries.Get(queryKey);
            var parameters = new Dictionary<string, object> { { paramKey, paramValue } };
            return await connection.QueryFirstOrDefaultAsync<int?>(query, parameters);
        }

        public async Task<string> GenerateEmployeeNumberAsync(int organisationId)
        {
            using var connection = _context.CreateConnection();

            var tenantConfig = await connection.QueryFirstOrDefaultAsync<TenantConfiguration>(
                _queries.Get("GetTenantConfiguration"),
                new { OrganisationId = organisationId });

            if (tenantConfig != null &&
                !string.IsNullOrEmpty(tenantConfig.EmployeeNoPrefix) &&
                tenantConfig.EmployeeNoStartWith.HasValue)
            {
                string prefix = tenantConfig.EmployeeNoPrefix;
                int start = tenantConfig.EmployeeNoStartWith.Value;

                string empNo;
                bool exists;

                do
                {
                    empNo = $"{prefix}{start}";
                    exists = await connection.ExecuteScalarAsync<int>(
                        _queries.Get("CheckEmployeeNumberExists"),
                        new { EmpNo = empNo, OrganisationId = organisationId }) > 0;

                    if (exists) start++;
                }
                while (exists);

                return empNo;
            }

            var employeeNumbers = await connection.QueryAsync<string>(
                _queries.Get("GetAllEmployeeNumbersForTenant"),
                new { OrganisationId = organisationId });

            var numericList = employeeNumbers.Select(x => int.TryParse(x, out var n) ? n : 0).ToList();
            int next = numericList.Any() ? numericList.Max() + 1 : 1;

            return next.ToString();
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByBranchExceptUserAsync(int branchId, int userId)
        {
            using var connection = _context.CreateConnection();
            var sql = _queries.Get("GetEmployeesByBranchExceptUser");

            return await connection.QueryAsync<Employee>(
                sql,
                new { BranchId = branchId, UserId = userId }
            );
        }
    }
}

using Dapper;
using Microsoft.Extensions.Logging;
using MobileWebApi.Constants;
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
        private readonly ILogger<EmployeeRepository> _logger;

        public EmployeeRepository(DapperContext context, QueryProvider queries, ILogger<EmployeeRepository> logger)
        {
            _context = context;
            _queries = queries;
            _logger = logger;
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            try
            {
                using var connection = _context.CreateConnection();
                string query = _queries.Get("GetEmployeeById");
                return await connection.QueryFirstOrDefaultAsync<Employee>(query, new { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetEmployeeByIdAsync));
                throw new Exception($"{ExceptionCodes.Repository.DatabaseError}: Failed to fetch employee by id", ex);
            }
        }

        public async Task<EmployeePersonalDetailsQueryResult?> GetEmployeePersonalDetailsByIdAsync(int id)
        {
            try
            {
                using var connection = _context.CreateConnection();
                string query = _queries.Get("GetEmployeePersonalDetailsById");
                return await connection.QueryFirstOrDefaultAsync<EmployeePersonalDetailsQueryResult>(query, new { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetEmployeePersonalDetailsByIdAsync));
                throw new Exception($"{ExceptionCodes.Repository.DatabaseError}: Failed to fetch employee personal details by id", ex);
            }
        }

        public async Task<Employee?> GetEmployeebyUserIdAsync(int systemUserId)
        {
            try
            {
                using var connection = _context.CreateConnection();
                string query = _queries.Get("GetEmployeeByUserId");
                return await connection.QueryFirstOrDefaultAsync<Employee>(query, new { SystemUserId = systemUserId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetEmployeebyUserIdAsync));
                throw new Exception($"{ExceptionCodes.Repository.DatabaseError}: Failed to fetch employee by user id", ex);
            }
        }

        public async Task<Employee?> GetEmployeeByPhoneAsync(string phone)
        {
            try
            {
                using var connection = _context.CreateConnection();
                string query = _queries.Get("GetEmployeeByPhone");
                return await connection.QueryFirstOrDefaultAsync<Employee>(query, new { Phone = phone });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetEmployeeByPhoneAsync));
                throw new Exception($"{ExceptionCodes.Repository.DatabaseError}: Failed to fetch employee by phone", ex);
            }
        }

        public async Task<Employee?> GetEmployeeByEmployeeNumberAsync(string employeeNumber)
        {
            try
            {
                using var connection = _context.CreateConnection();
                string query = _queries.Get("GetEmployeeByEmployeeNumber");
                return await connection.QueryFirstOrDefaultAsync<Employee>(query, new { EmployeeNumber = employeeNumber });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetEmployeeByEmployeeNumberAsync));
                throw new Exception($"{ExceptionCodes.Repository.DatabaseError}: Failed to fetch employee by employee number", ex);
            }
        }

        public async Task<Employee> AddEmployeeAsync(Employee employee)
        {
            try
            {
                string query = _queries.Get("AddEmployee");
                using var connection = _context.CreateConnection();
                employee.Id = await connection.ExecuteScalarAsync<int>(query, employee);
                return employee;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(AddEmployeeAsync));
                throw new Exception($"{ExceptionCodes.Repository.DatabaseError}: Failed to add employee", ex);
            }
        }

        public async Task<bool> UpdateEmployeeAsync(Employee employee)
        {
            try
            {
                using var connection = _context.CreateConnection();
                string query = _queries.Get("UpdateEmployee");
                var rowsAffected = await connection.ExecuteAsync(query, employee);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(UpdateEmployeeAsync));
                throw new Exception($"{ExceptionCodes.Repository.DatabaseError}: Failed to update employee", ex);
            }
        }
		public async Task<bool> UpdateEmployeePhoneAndPictureAsync(
	int employeeId,
	string? phone,
	string? picture)
		{
			try
			{
				using var connection = _context.CreateConnection();

				var query = _queries.Get("UpdateEmployeePhoneAndPicture");
				if (string.IsNullOrWhiteSpace(query))
					throw new InvalidOperationException(
						"SQL query 'UpdateEmployeePhoneAndPicture' not found.");

				var rowsAffected = await connection.ExecuteAsync(
					query,
					new
					{
						Id = employeeId,
						Phone = phone,
						Picture = picture
					});

				return rowsAffected > 0;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Database error occurred in {Method}", nameof(UpdateEmployeePhoneAndPictureAsync));
				throw new Exception($"{ExceptionCodes.Repository.DatabaseError}: Failed to update employee phone and picture", ex);
			}
		}


		//public async Task<bool> UpdateEmployeePhoneAndPictureAsync(int employeeId, string? phone, string? picture)
  //      {
  //          using var connection = _context.CreateConnection();
  //          string query = _queries.Get("UpdateEmployeePhoneAndPicture");
  //          var rowsAffected = await connection.ExecuteAsync(query, new { Id = employeeId, Phone = phone, Picture = picture });
  //          return rowsAffected > 0;
  //      }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            try
            {
                using var connection = _context.CreateConnection();
                string query = _queries.Get("DeleteEmployee");
                var rowsAffected = await connection.ExecuteAsync(query, new { Id = id });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(DeleteEmployeeAsync));
                throw new Exception($"{ExceptionCodes.Repository.DatabaseError}: Failed to delete employee", ex);
            }
        }

        public async Task<bool> DeactivateEmployeeAsync(int id)
        {
            try
            {
                using var connection = _context.CreateConnection();
                string query = _queries.Get("DeactivateEmployee");
                var rowsAffected = await connection.ExecuteAsync(query, new { Id = id });
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(DeactivateEmployeeAsync));
                throw new Exception($"{ExceptionCodes.Repository.DatabaseError}: Failed to deactivate employee", ex);
            }
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByBranchAsync(int branchId)
        {
            try
            {
                string query = _queries.Get("GetEmployeesByBranchId");
                using var connection = _context.CreateConnection();
                return await connection.QueryAsync<Employee>(query, new { BranchId = branchId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetEmployeesByBranchAsync));
                throw new Exception($"{ExceptionCodes.Repository.DatabaseError}: Failed to fetch employees by branch", ex);
            }
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
            try
            {
                using var connection = _context.CreateConnection();
                string query = _queries.Get("GetEmployeesByOrganisationId");
                return await connection.QueryAsync<Employee>(query, new { OrganisationId = organisationId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetEmployeesbyOrganisationIdAsync));
                throw new Exception($"{ExceptionCodes.Repository.DatabaseError}: Failed to fetch employees by organisation id", ex);
            }
        }

        private async Task<int?> GetIdByNameAsync(string queryKey, string paramKey, string paramValue)
        {
            try
            {
                using var connection = _context.CreateConnection();
                string query = _queries.Get(queryKey);
                var parameters = new Dictionary<string, object> { { paramKey, paramValue } };
                return await connection.QueryFirstOrDefaultAsync<int?>(query, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetIdByNameAsync));
                throw new Exception($"{ExceptionCodes.Repository.DatabaseError}: Failed to resolve id by name", ex);
            }
        }

        public async Task<string> GenerateEmployeeNumberAsync(int organisationId)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GenerateEmployeeNumberAsync));
                throw new Exception($"{ExceptionCodes.Repository.DatabaseError}: Failed to generate employee number", ex);
            }
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByBranchExceptUserAsync(int branchId, int userId)
        {
            try
            {
                using var connection = _context.CreateConnection();
                var sql = _queries.Get("GetEmployeesByBranchExceptUser");

                return await connection.QueryAsync<Employee>(
                    sql,
                    new { BranchId = branchId, UserId = userId }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetEmployeesByBranchExceptUserAsync));
                throw new Exception($"{ExceptionCodes.Repository.DatabaseError}: Failed to fetch employees by branch except user", ex);
            }
        }
    }
}

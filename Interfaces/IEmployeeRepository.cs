using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<Employee?> GetEmployeeByIdAsync(int id);
        Task<EmployeePersonalDetailsQueryResult?> GetEmployeePersonalDetailsByIdAsync(int id);
        Task<Employee?> GetEmployeebyUserIdAsync(int systemUserId);
        Task<Employee?> GetEmployeeByPhoneAsync(string phone);
        Task<Employee?> GetEmployeeByEmployeeNumberAsync(string employeeNumber);
        Task<IEnumerable<Employee>> GetEmployeesByBranchAsync(int branchId);
        Task<IEnumerable<Employee>> GetEmployeesbyOrganisationIdAsync(int organisationId);
        Task<IEnumerable<Employee>> GetEmployeesByBranchExceptUserAsync(int branchId, int userId);
        Task<Employee> AddEmployeeAsync(Employee employee);
        Task<bool> UpdateEmployeeAsync(Employee employee);
        Task<bool> UpdateEmployeePhoneAndPictureAsync(int employeeId, string? phone, string? picture);
        Task<bool> DeleteEmployeeAsync(int id);
        Task<bool> DeactivateEmployeeAsync(int id);
        Task<int?> GetOrganisationIdByNameAsync(string organisationName);
        Task<int?> GetDesignationIdByNameAsync(string jobTitleName);
        Task<int?> GetBranchIdByNameAsync(string branchName);
        Task<int?> GetDepartmentIdByNameAsync(string departmentName);
        Task<int?> GetGenderIdByNameAsync(string genderName);
        Task<int?> GetBloodgroupIdByNameAsync(string bloodGroupName);
        Task<int?> GetMaritalStatusIdByNameAsync(string maritalStatus);
        Task<int?> GetStateIdByNameAsync(string stateName);
        Task<int?> GetCountryIdByNameAsync(string countryName);
        Task<string> GenerateEmployeeNumberAsync(int organisationId);
    }
}

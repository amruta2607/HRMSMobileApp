using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface IDisputeRepository
    {
        Task<IEnumerable<DisputeCategory>> GetDisputeCategoriesAsync();
        Task<Employee?> GetEmployeeByIdAsync(int employeeId);
        Task<EmployeeDispute?> GetExistingDisputeAsync(int employeeId, DateTime disputeDate);
        Task<int> InsertDisputeAsync(EmployeeDispute dispute);
    }
}


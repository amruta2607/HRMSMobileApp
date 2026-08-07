using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface IEmployeeService
    {
        Task<PersonalDetailServiceResponse> GetEmployeeByIdAsync(int id);
        Task<PersonalDetailServiceResponse> GetPersonalDetailsByUserIdAsync(int userId);
        Task<PersonalDetailServiceResponse> GetLoggedInEmployeeAsync(int userId);
        Task<PersonalDetailListResponse> GetEmployeesByBranchAsync(int branchId);
        Task<PersonalDetailListResponse> GetEmployeesByBranchExceptUserAsync(int branchId, int userId);
        Task<PersonalDetailServiceResponse> AddEmployeeAsync(PersonalDetailAddRequest request);
        Task<PersonalDetailServiceResponse> UpdateEmployeeAsync(PersonalDetailUpdateRequest request);
        Task<PersonalDetailServiceResponse> UpdateEmployeePhoneAndPictureAsync(PersonalDetailPhonePictureUpdateRequestInternal request);
        Task<PersonalDetailServiceResponse> DeleteEmployeeAsync(int userId);
        Task<PersonalDetailServiceResponse> DeactivateEmployeeAsync(int id);
    }
}

using System;

namespace MobileWebApi.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string EmployeeNumber { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Picture { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PlaceOfBirth { get; set; }
        public string BiometricNumber { get; set; }
        public string TaxNumber { get; set; }
        public string LatestEducationDegree { get; set; }
        public int GenderId { get; set; }
        public int GradeId { get; set; }
        public string ESINo { get; set; }
        public string PFNo { get; set; }
        public decimal SalarySlab { get; set; }
        public decimal BasicSalary { get; set; }
        public bool IsPerDayWagesEmployee { get; set; }
        public decimal? PerDayOverTimeWages { get; set; }
        public int DesignationId { get; set; }
        public int DepartmentId { get; set; }
        public int BranchId { get; set; }
        public int SupervisorId { get; set; }
        public int LeaveQuota { get; set; }
        public int LeaveTaken { get; set; }
        public DateTime DateOfJoining { get; set; }
        public string BankAccountForPayroll { get; set; }
        public string IFSCCode { get; set; }
        public string BankNameForPayroll { get; set; }
        public string BankBranchName { get; set; }
        public string SpouseName { get; set; }
        public DateTime? SpouseDateOfBirth { get; set; }
        public string SpouseProfession { get; set; }
        public string SpouseStreet { get; set; }
        public string SpouseCity { get; set; }
        public string SpouseState { get; set; }
        public string SpouseZipCode { get; set; }
        public string SpousePhone { get; set; }
        public string SpouseEmail { get; set; }
        public int SystemUserId { get; set; }
       
        /// <summary>
        /// OrganisationId - maps to TenantId column in database
        /// </summary>
        public int OrganisationId { get; set; }
        
        public decimal CostToCompany { get; set; }
        public string CtcFrequencyType { get; set; }
        public bool IsEmployeeActive { get; set; }
        public int CategoryId { get; set; }
        public int BloodGroup { get; set; }
        public int MaritalStatus { get; set; }
        public string PermanentStreet { get; set; }
        public string PermanentCity { get; set; }
        public string PermanentState { get; set; }
        public string PermanentZipCode { get; set; }
        public string PermanentPhone { get; set; }
        public string PermanentEmail { get; set; }
        public string PersonalEmail { get; set; }
        public int CountryId { get; set; }
        public int StateId { get; set; }
        public int PermanentCountryId { get; set; }
        public string GroupName { get; set; }
        public string NotificationGroupName { get; set; }
        public string UANNo { get; set; }
        public bool IsPayrollOnHold { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public int CompensationTemplateId { get; set; }
        public bool EnableLocationTracking { get; set; }
    }
}

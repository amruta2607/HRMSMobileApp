namespace MobileWebApi.Models
{
    public class PersonalDetailAddRequest
    {
        public string? name { get; set; }
       
        public string? email_id { get; set; }
        public string? mobile_number { get; set; }
        public string? personal_email_id { get; set; }
        public string? official_email_id { get; set; }
        public int? designationId { get; set; }
        public int? branchId { get; set; }
        public int? departmentId { get; set; }
        public DateTime date_of_joining { get; set; }
        public DateTime date_of_birth{ get; set; }
        public int? maritalStatusId { get; set; }
        public int? bloodGroupId { get; set; }
        public string? guardian_name { get; set; }
        public string? emergency_contact_name { get; set; }
        public string? emergency_contact_number { get; set; }
        public string? current_address { get; set; }
        public string? permanent_address { get; set; }
        public string? username { get; set; }
        public string? password { get; set; }
        public string? organisations { get; set; }
        public string? log_in_otp { get; set; }
        public int userId { get; set; }
        public int? genderId { get; set; }
        public int? stateId { get; set; }
        public int? countryId { get; set; }
        public int? permanentCountryId { get; set; }
    }
}

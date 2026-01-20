namespace MobileWebApi.Models
{
    public class PersonalDetailUpdateRequest
    {
        public int EmployeeId { get; set; }
        public string? name { get; set; }
        public string? email_id { get; set; }
        public string? mobile_number { get; set; }
        public string? personal_email_id { get; set; }
        public string? official_email_id { get; set; }
        public string? job_title { get; set; }
        public string? branch { get; set; }
        public string? department { get; set; }
        public DateTime? date_of_joining { get; set; }
        public DateTime? date_of_birth { get; set; }
        public string? marital_status { get; set; }
        public string? blood_group { get; set; }
        public string? guardian_name { get; set; }
        public string? emergency_contact_name { get; set; }
        public string? emergency_contact_number { get; set; }
        public string? current_address { get; set; }
        public string? permanent_address { get; set; }
        public string? gender { get; set; }
        public bool? is_active { get; set; }
    }
}


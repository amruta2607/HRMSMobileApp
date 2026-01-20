namespace MobileWebApi.Models
{
    public class Punch
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime PunchDate { get; set; }
        public DateTime? PunchIn { get; set; }
        public DateTime? PunchOut { get; set; }
        public double? Duration { get; set; }
        public DateTime InsertDate { get; set; }
        public int InsertUserId { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateUserId { get; set; }
        
        /// <summary>
        /// OrganisationId - maps to TenantId column in database
        /// </summary>
        public int OrganisationId { get; set; }
        
        //// Location data for punch-in
        //public double? PunchInLatitude { get; set; }
        //public double? PunchInLongitude { get; set; }
        
        //// Location data for punch-out
        //public double? PunchOutLatitude { get; set; }
        //public double? PunchOutLongitude { get; set; }
    }
}

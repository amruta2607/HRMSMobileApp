namespace MobileWebApi.Models
{
    /// <summary>
    /// Request model for getting list of pay slips
    /// </summary>
    public class PaySlipListRequest
    {
        /// <summary>
        /// User ID (System User ID)
        /// </summary>
        public int user { get; set; }
        
        /// <summary>
        /// Organization ID (TenantId - foreign key to Tenant table)
        /// </summary>
        public int? organization { get; set; }
        
        /// <summary>
        /// Year filter (optional)
        /// </summary>
        public int? year { get; set; }
        
        /// <summary>
        /// Month filter (optional, 1-12)
        /// </summary>
        public int? month { get; set; }
    }

    /// <summary>
    /// Request model for getting a specific pay slip
    /// </summary>
    public class PaySlipGetRequest
    {
        /// <summary>
        /// User ID (System User ID)
        /// </summary>
        public int user { get; set; }
        
        /// <summary>
        /// Organization ID (TenantId - foreign key to Tenant table)
        /// </summary>
        public int? organization { get; set; }
        
        /// <summary>
        /// Pay slip ID
        /// </summary>
        public int payslip_id { get; set; }
    }

    /// <summary>
    /// Request model for downloading pay slip
    /// </summary>
    public class PaySlipDownloadRequest
    {
        /// <summary>
        /// User ID (System User ID)
        /// </summary>
        public int user { get; set; }
        
        /// <summary>
        /// Organization ID (TenantId - foreign key to Tenant table)
        /// </summary>
        public int? organization { get; set; }
        
        /// <summary>
        /// Pay slip ID
        /// </summary>
        public int payslip_id { get; set; }
        
        /// <summary>
        /// Download format (pdf/excel)
        /// </summary>
        public string format { get; set; } = "pdf";
    }
}





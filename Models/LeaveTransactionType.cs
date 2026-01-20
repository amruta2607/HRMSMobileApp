namespace MobileWebApi.Models
{
    /// <summary>
    /// Enum for Leave Transaction Type
    /// Maps to LeaveTransactionType column in LeaveTransaction table
    /// </summary>
    public enum LeaveTransactionType
    {
        /// <summary>
        /// Add Leave (Value: 1)
        /// </summary>
        AddLeave = 1,

        /// <summary>
        /// Deduct Leave (Value: 2)
        /// </summary>
        DeductLeave = 2
    }
}


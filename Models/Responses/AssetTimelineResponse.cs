namespace MobileWebApi.Models.Responses
{
    /// <summary>
    /// A single AssetHistory row returned by the asset timeline API.
    /// Contains all columns from the AssetHistory table.
    /// </summary>
    public class AssetTimelineResponse
    {
        public int HistoryId { get; set; }
        public string? SourceTable { get; set; }
        public int? SourceRecordId { get; set; }
        public string? ActionType { get; set; }
        public DateTime? ActionDate { get; set; }
        public int? ActionUserId { get; set; }
        public int? TenantId { get; set; }

        public int? AssetId { get; set; }
        public string? Number { get; set; }
        public string? AssetName { get; set; }
        public string? Description { get; set; }

        public int? AssetStatusId { get; set; }
        public int? AssetCategoryId { get; set; }
        public int? DepartmentId { get; set; }
        public int? BranchId { get; set; }

        public DateTime? PurchaseDate { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? ActualValue { get; set; }

        public DateTime? WarrantyExpiryDate { get; set; }
        public DateTime? MaintenanceDueDate { get; set; }

        public string? PurchaseOrderNumber { get; set; }
        public string? PurchaseOrderBill { get; set; }

        public string? SupportCenter { get; set; }
        public string? Manufacturer { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public int? ProductionYear { get; set; }

        public string? AssetTagNumber { get; set; }
        public decimal? DepreciationPercentage { get; set; }

        public string? Images { get; set; }

        public string? AssetCode { get; set; }
        public string? QrCodePath { get; set; }
        public string? QrCodeText { get; set; }
        public bool? QrCodeGenerated { get; set; }
        public DateTime? QrCodeGeneratedDate { get; set; }

        public int? BusinessUnitId { get; set; }
        public int? AssetTypeId { get; set; }

        public string? Location { get; set; }
        public string? Owner { get; set; }

        public DateTime? HandOverDate { get; set; }
        public int? HandOverById { get; set; }
        public int? HandOverToId { get; set; }
        public string? HandOverByName { get; set; }
        public string? HandOverToName { get; set; }

        public DateTime? InsertDate { get; set; }
        public int? InsertUserId { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateUserId { get; set; }

        public decimal? MaintenanceCost { get; set; }
        public DateTime? MaintenanceWorkDate { get; set; }
        public int? MaintenanceResponsiblePersonId { get; set; }
        public string? MaintenanceResponsiblePersonName { get; set; }
        public string? MaintenanceAttachment { get; set; }
    }

    /// <summary>
    /// Response wrapper for asset timeline / history data.
    /// </summary>
    public class AssetTimelineListResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<AssetTimelineResponse> Data { get; set; } = new();
    }
}

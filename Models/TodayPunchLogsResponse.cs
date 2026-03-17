using System;
using System.Collections.Generic;

namespace MobileWebApi.Models
{
    public class TodayPunchLogItem
    {
        public string? Direction { get; set; }
        public DateTime LogDateTime { get; set; }
        public string? DeviceName { get; set; }
    }

    public class TodayPunchLogsResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<TodayPunchLogItem>? Data { get; set; }
    }
}


using MobileWebApi.Models;
using System.Text.Json;

namespace MobileWebApi.Helper
{
    public static class WeekOffHelper
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static int NormalizeDayOfWeek(int dayOffId)
        {
            if (dayOffId >= 1 && dayOffId <= 7)
                return dayOffId % 7;

            return dayOffId;
        }

        public static int GetWeekdayOccurrenceInMonth(DateTime date)
        {
            var occurrence = 0;
            for (var day = 1; day <= date.Day; day++)
            {
                if (new DateTime(date.Year, date.Month, day).DayOfWeek == date.DayOfWeek)
                    occurrence++;
            }

            return occurrence;
        }

        public static WeekOffConfiguration BuildConfiguration(
            IEnumerable<int> completeWeekOffDays,
            IEnumerable<PartialWeekOffDayItem>? partialWeekOffDays)
        {
            var config = new WeekOffConfiguration();

            if (completeWeekOffDays != null)
            {
                foreach (var day in completeWeekOffDays)
                    config.CompleteWeekOffDays.Add(NormalizeDayOfWeek(day));
            }

            if (partialWeekOffDays != null)
            {
                config.PartialWeekOffDays.AddRange(partialWeekOffDays);
            }

            return config;
        }

        public static bool IsWeeklyOff(DateTime date, WeekOffConfiguration config)
        {
            if (config == null)
                return false;

            var dayOfWeek = (int)date.DayOfWeek;
            if (config.CompleteWeekOffDays.Contains(dayOfWeek))
                return true;

            if (config.PartialWeekOffDays == null || config.PartialWeekOffDays.Count == 0)
                return false;

            var occurrence = GetWeekdayOccurrenceInMonth(date);
            return config.PartialWeekOffDays.Any(p =>
                NormalizeDayOfWeek(p.DayOffId) == dayOfWeek &&
                p.WeekOccurrence == occurrence);
        }

        public static List<int> ParseCompleteWeekOffs(string? weekOffList)
        {
            var result = new List<int>();
            if (string.IsNullOrWhiteSpace(weekOffList))
                return result;

            foreach (var part in weekOffList.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(part.Trim(), out var dayId))
                    result.Add(dayId);
            }

            return result.Distinct().ToList();
        }

        public static List<PartialWeekOffDayItem> ParsePartialWeekOffs(string? partialWeekOffJson)
        {
            if (string.IsNullOrWhiteSpace(partialWeekOffJson))
                return new List<PartialWeekOffDayItem>();

            try
            {
                var items = JsonSerializer.Deserialize<List<PartialWeekOffDayItem>>(partialWeekOffJson, JsonOptions);
                if (items == null)
                    return new List<PartialWeekOffDayItem>();

                return items
                    .Where(p => p != null && p.WeekOccurrence >= 1 && p.WeekOccurrence <= 5)
                    .Select(p => new PartialWeekOffDayItem
                    {
                        DayOffId = p.DayOffId,
                        WeekOccurrence = p.WeekOccurrence
                    })
                    .ToList();
            }
            catch
            {
                return new List<PartialWeekOffDayItem>();
            }
        }

        public static bool HasPartialWeekOffJson(string? partialWeekOffJson)
        {
            if (string.IsNullOrWhiteSpace(partialWeekOffJson))
                return false;

            if (string.Equals(partialWeekOffJson.Trim(), "[]", StringComparison.Ordinal))
                return false;

            return ParsePartialWeekOffs(partialWeekOffJson).Count > 0;
        }
    }
}

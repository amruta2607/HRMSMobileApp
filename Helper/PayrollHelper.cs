namespace MobileWebApi.Helper
{
	public static class PayrollHelper
	{
		public static short GetFinancialYearStart(short payrollMonth, short payrollYear)
			=> payrollMonth >= 4 ? payrollYear : (short)(payrollYear - 1);

		public static int NormalizeDayOfWeek(int dayOffId) => dayOffId % 7; // 7 -> 0 (Sunday)

		public static int GetTotalWeekOffDays(List<int> dayOffIds, int month, int year)
		{
			var weekOffSet = new HashSet<int>(dayOffIds);
			int totalDays = DateTime.DaysInMonth(year, month);
			int count = 0;

			for (int day = 1; day <= totalDays; day++)
			{
				int dayOfWeek = (int)new DateTime(year, month, day).DayOfWeek;
				if (weekOffSet.Contains(dayOfWeek))
					count++;
			}
			return count;
		}
	}
}

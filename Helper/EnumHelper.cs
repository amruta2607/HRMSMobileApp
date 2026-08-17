using System.ComponentModel;

namespace MobileWebApi.Helper
{
	public static class EnumHelper
	{
		public static string GetDescription<T>(T enumValue) where T : Enum
		{
			var fi = enumValue.GetType().GetField(enumValue.ToString());
			var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
			return attributes.Length > 0 ? attributes[0].Description : enumValue.ToString();
		}
	}

}

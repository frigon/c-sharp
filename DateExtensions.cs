using System;
using System.Collections.Generic;
using System.Text;

namespace Spring.Framework
{
	static class DateExtensions
	{
		public static DateTimeOffset ToMidnight(this DateTimeOffset baseDate)
		{
			var date = baseDate.Date;
			return new DateTimeOffset(date, baseDate.Offset);
		}

		public static DateTimeOffset ToNoon(this DateTimeOffset baseDate)
		{
			var date = baseDate.Date;
			date = date.AddHours(12);
			return new DateTimeOffset(date, baseDate.Offset);
		}

		public static DateTime GetMonth(this DateTime date)
		{
			return date.Date.AddDays(-date.Day + 1);
		}

		public static DateTime GetNextMonth(this DateTime date)
		{
			return date.GetMonth().AddMonths(1).Date;
		}

		public static DateTime GetPreviousMonth(this DateTime date)
		{
			return date.GetMonth().AddMonths(-1).Date;
		}

		public static DateTime GetWeek(this DateTime date)
		{
			return date.Date.AddDays(-(int)date.DayOfWeek);
		}

		public static DateTime GetNextWeek(this DateTime date)
		{
			return date.GetWeek().AddDays(7).Date;
		}
	}
}

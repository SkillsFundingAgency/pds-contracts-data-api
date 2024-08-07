using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Pds.Contracts.Data.Services.Extensions
{
    /// <summary>
    /// Date Time Extensions.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Extension method for displaying the datetime string.
        /// </summary>
        /// <param name="dateTime">DateTime.</param>
        /// <returns>DateTime string with custom display format.</returns>
        public static string DisplayFormat(this DateTime dateTime)
        {
            switch (dateTime.Kind)
            {
                case DateTimeKind.Local:
                    dateTime = dateTime.ToUniversalTime();
                    break;
                case DateTimeKind.Unspecified:
                    dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                    break;
                default:
                    break;
            }

            var dateString = TimeZoneInfo.ConvertTime(dateTime, TimeZoneInfo.Utc, TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"))
                      .ToString("d MMMM yyyy a\\t h:mmtt");

            return dateString.Replace(dateString[^2..], dateString[^2..].ToLower());
        }

        /// <summary>
        /// Extension method to convert a GMT datetime to a UTC datetime.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>Datetime.</returns>
        public static DateTime ToUtcTime(this DateTime dateTime)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(dateTime, timeZone);
        }
    }
}

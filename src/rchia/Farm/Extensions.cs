using System;
using System.Text;

namespace rchia.Farm
{

    internal static class Extensions
    {
        public static string FormatTimeSpan(this TimeSpan t)
        {
            var builder = new StringBuilder();
            if (t.Days > 0)
            {
                _ = builder.Append($"{t.Days} day{(t.Days > 1 ? "s" : "")} ");
            }

            if (t.Hours > 0)
            {
                _ = builder.Append($"{t.Hours} hour{(t.Hours > 1 ? "s" : "")} ");
            }

            if (t.Minutes > 0)
            {
                _ = builder.Append($"{t.Minutes} minute{(t.Minutes > 1 ? "s" : "")} ");
            }

            return builder.ToString();
        }
    }
}

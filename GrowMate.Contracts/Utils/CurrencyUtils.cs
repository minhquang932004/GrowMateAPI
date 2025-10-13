using System;
using System.Globalization;

namespace GrowMate.Contracts.Utils
{
    /// <summary>
    /// Utility class for handling currency-related operations
    /// </summary>
    public static class CurrencyUtils
    {
        /// <summary>
        /// The currency code for Vietnamese Dong
        /// </summary>
        public const string VND = "VND";
        
        /// <summary>
        /// Culture info for Vietnam
        /// </summary>
        private static readonly CultureInfo VietnameseCulture = new CultureInfo("vi-VN");
        
        /// <summary>
        /// Formats a decimal value as VND currency string
        /// </summary>
        /// <param name="amount">The amount to format</param>
        /// <returns>A formatted string representing the amount in VND</returns>
        public static string FormatVND(decimal amount)
        {
            // Format the amount using the Vietnamese culture
            return string.Format(VietnameseCulture, "{0:C0}", amount);
        }
        
        /// <summary>
        /// Rounds a decimal value to the nearest 1000 VND
        /// Vietnamese Dong is typically rounded to the nearest 1000 for practical purposes
        /// </summary>
        /// <param name="amount">The amount to round</param>
        /// <returns>The rounded amount</returns>
        public static decimal RoundToNearestThousand(decimal amount)
        {
            // Round to the nearest thousand
            return Math.Round(amount / 1000) * 1000;
        }
    }
}
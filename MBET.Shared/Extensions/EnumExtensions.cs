using Microsoft.Extensions.Localization;
using MBET.Shared.Resources;
using System;

namespace MBET.Shared.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Translates an Enum value using the IStringLocalizer.
        /// Expects resource keys in the format "EnumTypeName_ValueName".
        /// Example: OrderStatus.Pending -> looks for key "OrderStatus_Pending"
        /// </summary>
        public static string GetLocalizedName(this Enum enumValue, IStringLocalizer<L> localizer)
        {
            if (enumValue == null) return string.Empty;

            var typeName = enumValue.GetType().Name; // e.g., "OrderStatus"
            var valueName = enumValue.ToString();    // e.g., "Pending"
            var key = $"{typeName}_{valueName}";     // "OrderStatus_Pending"

            var localizedString = localizer[key];

            // If translation is missing, return the default enum text
            return localizedString.ResourceNotFound ? valueName : localizedString.Value;
        }
    }
}
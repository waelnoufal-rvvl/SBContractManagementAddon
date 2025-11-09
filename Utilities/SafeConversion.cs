using System;

namespace ContractManagementAddon.Utilities
{
    /// <summary>
    /// Provides safe type conversion methods that handle NULL and DBNull values
    /// without throwing exceptions. Used throughout repositories to prevent
    /// crashes when database fields contain NULL values.
    /// </summary>
    public static class SafeConversion
    {
        /// <summary>
        /// Safely converts an object to DateTime, returning a default value if null or DBNull
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="defaultValue">The default value to return if conversion fails</param>
        /// <returns>Converted DateTime or default value</returns>
        public static DateTime SafeToDateTime(object value, DateTime defaultValue)
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;

            try
            {
                return Convert.ToDateTime(value);
            }
            catch (Exception ex)
            {
                Logger.Warning($"Failed to convert value to DateTime: {value}. Using default. Error: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Safely converts an object to DateTime, returning DateTime.MinValue if null or DBNull
        /// </summary>
        public static DateTime SafeToDateTime(object value)
        {
            return SafeToDateTime(value, DateTime.MinValue);
        }

        /// <summary>
        /// Safely converts an object to nullable DateTime
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>Converted DateTime or null</returns>
        public static DateTime? SafeToNullableDateTime(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            try
            {
                return Convert.ToDateTime(value);
            }
            catch (Exception ex)
            {
                Logger.Warning($"Failed to convert value to DateTime: {value}. Error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Safely converts an object to int, returning a default value if null or DBNull
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="defaultValue">The default value to return if conversion fails</param>
        /// <returns>Converted int or default value</returns>
        public static int SafeToInt(object value, int defaultValue = 0)
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;

            try
            {
                return Convert.ToInt32(value);
            }
            catch (Exception ex)
            {
                Logger.Warning($"Failed to convert value to int: {value}. Using default. Error: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Safely converts an object to nullable int
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>Converted int or null</returns>
        public static int? SafeToNullableInt(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            try
            {
                return Convert.ToInt32(value);
            }
            catch (Exception ex)
            {
                Logger.Warning($"Failed to convert value to int: {value}. Error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Safely converts an object to double, returning a default value if null or DBNull
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="defaultValue">The default value to return if conversion fails</param>
        /// <returns>Converted double or default value</returns>
        public static double SafeToDouble(object value, double defaultValue = 0.0)
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;

            try
            {
                return Convert.ToDouble(value);
            }
            catch (Exception ex)
            {
                Logger.Warning($"Failed to convert value to double: {value}. Using default. Error: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Safely converts an object to nullable double
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>Converted double or null</returns>
        public static double? SafeToNullableDouble(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            try
            {
                return Convert.ToDouble(value);
            }
            catch (Exception ex)
            {
                Logger.Warning($"Failed to convert value to double: {value}. Error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Safely converts an object to decimal, returning a default value if null or DBNull
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="defaultValue">The default value to return if conversion fails</param>
        /// <returns>Converted decimal or default value</returns>
        public static decimal SafeToDecimal(object value, decimal defaultValue = 0.0m)
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;

            try
            {
                return Convert.ToDecimal(value);
            }
            catch (Exception ex)
            {
                Logger.Warning($"Failed to convert value to decimal: {value}. Using default. Error: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Safely converts an object to nullable decimal
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>Converted decimal or null</returns>
        public static decimal? SafeToNullableDecimal(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            try
            {
                return Convert.ToDecimal(value);
            }
            catch (Exception ex)
            {
                Logger.Warning($"Failed to convert value to decimal: {value}. Error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Safely converts an object to string, returning a default value if null or DBNull
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="defaultValue">The default value to return if null</param>
        /// <returns>Converted string or default value</returns>
        public static string SafeToString(object value, string defaultValue = "")
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;

            try
            {
                return value.ToString();
            }
            catch (Exception ex)
            {
                Logger.Warning($"Failed to convert value to string. Using default. Error: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Safely converts an object to bool, returning a default value if null or DBNull
        /// </summary>
        /// <param name="value">The value to convert (Y/N or true/false)</param>
        /// <param name="defaultValue">The default value to return if conversion fails</param>
        /// <returns>Converted bool or default value</returns>
        public static bool SafeToBool(object value, bool defaultValue = false)
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;

            try
            {
                string strValue = value.ToString().ToUpper();

                // Handle SAP B1 Y/N values
                if (strValue == "Y" || strValue == "YES" || strValue == "TRUE" || strValue == "1")
                    return true;

                if (strValue == "N" || strValue == "NO" || strValue == "FALSE" || strValue == "0")
                    return false;

                return Convert.ToBoolean(value);
            }
            catch (Exception ex)
            {
                Logger.Warning($"Failed to convert value to bool: {value}. Using default. Error: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Safely converts an object to nullable bool
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>Converted bool or null</returns>
        public static bool? SafeToNullableBool(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            try
            {
                string strValue = value.ToString().ToUpper();

                if (strValue == "Y" || strValue == "YES" || strValue == "TRUE" || strValue == "1")
                    return true;

                if (strValue == "N" || strValue == "NO" || strValue == "FALSE" || strValue == "0")
                    return false;

                return Convert.ToBoolean(value);
            }
            catch (Exception ex)
            {
                Logger.Warning($"Failed to convert value to bool: {value}. Error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Safely converts an object to Guid, returning a default value if null or DBNull
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="defaultValue">The default value to return if conversion fails</param>
        /// <returns>Converted Guid or default value</returns>
        public static Guid SafeToGuid(object value, Guid defaultValue)
        {
            if (value == null || value == DBNull.Value)
                return defaultValue;

            try
            {
                if (value is Guid)
                    return (Guid)value;

                return Guid.Parse(value.ToString());
            }
            catch (Exception ex)
            {
                Logger.Warning($"Failed to convert value to Guid: {value}. Using default. Error: {ex.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// Safely converts an object to Guid, returning Guid.Empty if null or DBNull
        /// </summary>
        public static Guid SafeToGuid(object value)
        {
            return SafeToGuid(value, Guid.Empty);
        }

        /// <summary>
        /// Safely converts an object to nullable Guid
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>Converted Guid or null</returns>
        public static Guid? SafeToNullableGuid(object value)
        {
            if (value == null || value == DBNull.Value)
                return null;

            try
            {
                if (value is Guid)
                    return (Guid)value;

                return Guid.Parse(value.ToString());
            }
            catch (Exception ex)
            {
                Logger.Warning($"Failed to convert value to Guid: {value}. Error: {ex.Message}");
                return null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SharedLibrary.Common
{
    /// <summary>
    /// Masks sensitive data in logs to protect PII and credentials.
    /// </summary>
    public static class SensitiveDataMasker
    {
        private const string MaskedValue = "***MASKED***";
        private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

        /// <summary>
        /// Masks sensitive fields in an object for logging.
        /// </summary>
        public static string MaskSensitiveData(object obj)
        {
            if (obj == null)
                return null;

            try
            {
                var json = JsonSerializer.Serialize(obj, JsonOptions);
                return MaskSensitiveFieldsInJson(json);
            }
            catch
            {
                return obj.ToString();
            }
        }

        /// <summary>
        /// Masks sensitive fields in a JSON string.
        /// </summary>
        public static string MaskSensitiveFieldsInJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return json;

            try
            {
                using var doc = JsonDocument.Parse(json);
                var maskedJson = MaskJsonElement(doc.RootElement).ToString();
                return maskedJson;
            }
            catch
            {
                return json;
            }
        }

        /// <summary>
        /// Masks sensitive fields in a dictionary.
        /// </summary>
        public static Dictionary<string, string> MaskHeaders(Dictionary<string, string> headers)
        {
            if (headers == null)
                return null;

            var masked = new Dictionary<string, string>();
            foreach (var kvp in headers)
            {
                masked[kvp.Key] = IsSensitiveField(kvp.Key) ? MaskedValue : kvp.Value;
            }
            return masked;
        }

        /// <summary>
        /// Masks sensitive fields in a string value.
        /// </summary>
        public static string MaskValue(string fieldName, string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return IsSensitiveField(fieldName) ? MaskedValue : value;
        }

        /// <summary>
        /// Checks if a field name is considered sensitive.
        /// </summary>
        public static bool IsSensitiveField(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
                return false;

            var lowerName = fieldName.ToLower();
            return LoggingConstants.SensitiveFields.Any(field => 
                lowerName.Contains(field) || 
                lowerName.Equals(field));
        }

        private static object MaskJsonElement(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Object => MaskJsonObject(element),
                JsonValueKind.Array => MaskJsonArray(element),
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.GetDecimal(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => null
            };
        }

        private static object MaskJsonObject(JsonElement element)
        {
            var result = new Dictionary<string, object>();

            foreach (var property in element.EnumerateObject())
            {
                var isSensitive = IsSensitiveField(property.Name);
                result[property.Name] = isSensitive 
                    ? MaskedValue 
                    : MaskJsonElement(property.Value);
            }

            return result;
        }

        private static object MaskJsonArray(JsonElement element)
        {
            var result = new List<object>();

            foreach (var item in element.EnumerateArray())
            {
                result.Add(MaskJsonElement(item));
            }

            return result;
        }
    }
}

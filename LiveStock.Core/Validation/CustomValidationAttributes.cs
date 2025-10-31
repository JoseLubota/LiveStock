using System.ComponentModel.DataAnnotations;

namespace LiveStock.Core.Validation
{
    /// <summary>
    /// Validates that a date is not in the future
    /// </summary>
    public class NotFutureDateAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null) return true; // Let [Required] handle null validation
            
            if (value is DateTime dateTime)
            {
                return dateTime.Date <= DateTime.Today;
            }
            
            if (value is DateOnly dateOnly)
            {
                return dateOnly <= DateOnly.FromDateTime(DateTime.Today);
            }
            
            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} cannot be in the future.";
        }
    }

    /// <summary>
    /// Validates that a date is within a reasonable range (not too far in past/future)
    /// </summary>
    public class ReasonableDateRangeAttribute : ValidationAttribute
    {
        private readonly int _yearsInPast;
        private readonly int _yearsInFuture;

        public ReasonableDateRangeAttribute(int yearsInPast = 50, int yearsInFuture = 5)
        {
            _yearsInPast = yearsInPast;
            _yearsInFuture = yearsInFuture;
        }

        public override bool IsValid(object? value)
        {
            if (value == null) return true;
            
            DateTime minDate = DateTime.Today.AddYears(-_yearsInPast);
            DateTime maxDate = DateTime.Today.AddYears(_yearsInFuture);
            
            if (value is DateTime dateTime)
            {
                return dateTime.Date >= minDate && dateTime.Date <= maxDate;
            }
            
            if (value is DateOnly dateOnly)
            {
                var dateOnlyMin = DateOnly.FromDateTime(minDate);
                var dateOnlyMax = DateOnly.FromDateTime(maxDate);
                return dateOnly >= dateOnlyMin && dateOnly <= dateOnlyMax;
            }
            
            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be within {_yearsInPast} years in the past and {_yearsInFuture} years in the future.";
        }
    }

    /// <summary>
    /// Validates that a string contains only safe characters (no HTML/script injection)
    /// </summary>
    public class SafeStringAttribute : ValidationAttribute
    {
        private static readonly string[] DangerousPatterns = {
            "<script", "</script>", "javascript:", "vbscript:", "onload=", "onerror=", 
            "onclick=", "onmouseover=", "onfocus=", "onblur=", "onchange=", "onsubmit="
        };

        public override bool IsValid(object? value)
        {
            if (value == null) return true;
            
            string stringValue = value.ToString()?.ToLowerInvariant() ?? string.Empty;
            
            return !DangerousPatterns.Any(pattern => stringValue.Contains(pattern));
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} contains potentially unsafe content.";
        }
    }

    /// <summary>
    /// Validates that a file extension is allowed
    /// </summary>
    public class AllowedFileExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _allowedExtensions;

        public AllowedFileExtensionsAttribute(params string[] allowedExtensions)
        {
            _allowedExtensions = allowedExtensions.Select(ext => ext.ToLowerInvariant()).ToArray();
        }

        public override bool IsValid(object? value)
        {
            if (value == null) return true;
            
            string url = value.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(url)) return true;
            
            try
            {
                var uri = new Uri(url);
                string extension = Path.GetExtension(uri.LocalPath).ToLowerInvariant();
                return _allowedExtensions.Contains(extension);
            }
            catch
            {
                return false; // Invalid URL format
            }
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} must have one of the following extensions: {string.Join(", ", _allowedExtensions)}.";
        }
    }
}
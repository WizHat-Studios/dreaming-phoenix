using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WizHat.DreamingPhoenix.ValidationRules
{
    public class ToStringEmptyValidationRule : ValidationRule
    {
        public bool CheckForNull { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value.ToString().Trim() == "")
                return new(false, "The value can't be empty");

            if (CheckForNull && value is null)
                return new(false, "The value can't be empty");

            return ValidationResult.ValidResult;
        }
    }
}

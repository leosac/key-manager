using Leosac.KeyManager.Library.Policy;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Leosac.KeyManager.Library.UI.Domain
{
    public class KeyPolicyValidationRule : ValidationRule
    {
        public IKeyPolicy? Policy { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                Policy?.Validate((value ?? "").ToString());
                return ValidationResult.ValidResult;
            }
            catch(KeyPolicyException ex)
            {
                return new ValidationResult(false, ex.Message);
            }
        }
    }
}

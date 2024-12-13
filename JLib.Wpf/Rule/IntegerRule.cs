using System.Globalization;
using System.Windows.Controls;

namespace JLib.Wpf.Rule
{
    public class IntegerRule : ValidationRule
    {
        #region Properties

        public int Min { get; set; }

        public int Max { get; set; }

        public bool CanEmpty { get; set; }

        #endregion


        #region Implement ValidationRule

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var s = (string)value;

            if (string.IsNullOrWhiteSpace(s))
            {
                if (CanEmpty)
                {
                    return ValidationResult.ValidResult;
                }

                return new ValidationResult(false, $"輸入須介於 {Min} 和 {Max} 之間");
            }

            if (!int.TryParse(s, out int n) || n < Min || n > Max)
            {
                return new ValidationResult(false, $"輸入須介於 {Min} 和 {Max} 之間");
            }

            return ValidationResult.ValidResult;
        }

        #endregion
    }
}

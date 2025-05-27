using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace DVLD.Classes
{
    public class clsValidatoin
    {

        public static bool ValidateEmail(string emailAddress)
        {
            var pattern = @"^[a-zA-Z0-9.!#$%&'*+-/=?^_`{|}~]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$";

            var regex = new Regex(pattern);

            return regex.IsMatch(emailAddress);
        }

        public static bool ValidateInteger(string Number)
        {
            var pattern = @"^[0-9]*$";

            var regex = new Regex(pattern);

            return regex.IsMatch(Number);
        }

        public static bool ValidateFloat(string Number)
        {
            var pattern = @"^[0-9]*(?:\.[0-9]*)?$";

            var regex = new Regex(pattern);

            return regex.IsMatch(Number);
        }

        public static bool IsNumber(string Number)
        {
            return (ValidateInteger(Number) || ValidateFloat(Number));
        }


        public static bool IsFirstLetterUpper(string Text)
        {
            // تحقق من أن النص يبدأ بحرف كبير (إنجليزي أو عربي)

            var pattern = @"^[A-Z\u0621-\u064A]";

            var regex = new Regex(pattern);

            return regex.IsMatch(Text);

        }
        public static bool AreRemainingLettersLowercase(string Text)
        {
            // تحقق من أن الحروف بعد الحرف الأول كلها صغيرة (إنجليزي أو عربي)
            var pattern = @"^[A-Zء-ي][a-zء-ي]*$";

            var regex = new Regex(pattern);

            return regex.IsMatch(Text);
        }

        public static bool IsTenDigitsOnly(string input)
        {
            // التحقق من أن النص يتكون من 10 أرقام فقط
            return input.Length == 10 && input.All(char.IsDigit);
        }
    }
}

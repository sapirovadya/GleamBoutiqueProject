using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Globalization;


namespace GleamBoutiqueProject.Models
{
    public class Payment
    {
        // The credit card number must be provided and should be a valid credit card number.
        [Required(ErrorMessage = "Credit card number is required.")]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Credit card number must be 16 digits.")]
        public string CreditCardNumber { get; set; }

        // The expiry date must be provided and should be in MM/YY format.
        [Required(ErrorMessage = "Expiry date is required.")]
        [RegularExpression(@"^(0[1-9]|1[012])\/\d{2}$", ErrorMessage = "Date must be in the format MM/YY")]
        public string ExpiryDate { get; set; }

        // The CVV must be provided and should be a 3 digit number.
        [Required(ErrorMessage = "CVV is required.")]
        [RegularExpression(@"^\d{3}$", ErrorMessage = "CVV must be a 3 digit number.")]
        public string CVV { get; set; }

        [Required(ErrorMessage = "ID is required.")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "ID must be an 8-digit number.")]
        public string ID { get; set; }

        // The full name must be provided.
        [Required(ErrorMessage = "Full name is required.")]
        public string FullName { get; set; }


        public bool IsExpiryDateValid()
        {
            DateTime expiryDate = ConvertToDateTime(ExpiryDate);
            DateTime currentDate = DateTime.Today;
            return expiryDate >= currentDate;
        }

        public DateTime ConvertToDateTime(string expiryDateMMYY)
        {
            if (DateTime.TryParseExact(expiryDateMMYY, "MM/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                return new DateTime(date.Year, date.Month, 1);
            }
            else
            {
                throw new ArgumentException("Invalid date format.", nameof(expiryDateMMYY));
            }
        }
    }
}





































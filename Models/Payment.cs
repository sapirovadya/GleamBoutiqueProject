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

        [Key]
        public int shipId { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "First name should only contain letters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Last name should only contain letters.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "City is required.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "City should only contain letters.")]
        public string City { get; set; }

        [Required(ErrorMessage = "Street is required.")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Street should only contain letters.")]
        public string Street { get; set; }

        [Required(ErrorMessage = "PostalCode is required.")]
        [RegularExpression(@"^\d{7}$", ErrorMessage = "Postal Code should exactly be 7 digits.")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Apartment is required.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-]+$", ErrorMessage = "Apartment should only contain letters, digits, spaces, and hyphens.")]
        public string Apartment { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^0\d{1,2}-\d{7}$", ErrorMessage = "Invaild Phone number")]
        public string Phone { get; set; }

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





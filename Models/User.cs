using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


using System.ComponentModel.DataAnnotations;

namespace GleamBoutiqueProject.Models
{

    public class User
    {
        public User(User user1)
        {
            this.FirstName = user1.FirstName;
            this.LastName = user1.LastName;
            this.Email = user1.Email;
            this.Password = user1.Password;

        }
        public User()
        {
        }



        [Required(ErrorMessage = "First name is required")]
        [StringLength(25, MinimumLength = 2, ErrorMessage = "First name must be between 2-25 letters")]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "First name should contain only letters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(25, MinimumLength = 2, ErrorMessage = "Last name must be between 2-25 letters")]
        [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "Last name should contain only letters")]
        public string LastName { get; set; }

        [Key]
        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email address")]
        public string Email { get; set; }


        [Required(ErrorMessage = "Password is required")]
        [StringLength(25, MinimumLength = 6, ErrorMessage = "Password must be between 6-25 letters")]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)[A-Za-z\\d]*$", ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, and one digit")]
        public string Password { get; set; }

    }
}

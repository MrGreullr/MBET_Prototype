using System.ComponentModel.DataAnnotations;

namespace MBET.web.ViewModels
{
    public class AdminCreateUserViewModel
    {
        [Required]
        public string FirstName { get; set; } = "";

        [Required]
        public string LastName { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Password { get; set; } = ""; // Admins usually set a temp password

        public string ConfirmPassword { get; set; } = ""; 

        [Required]
        public string Role { get; set; } = "Customer"; // Dropdown selection

        public bool IsActive { get; set; } = true;
    }
}
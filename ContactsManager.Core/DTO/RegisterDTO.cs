using ContactsManager.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace ContactsManager.Core.DTO
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Name cant be blank")]
        public string PersonName { get; set; }

        [Required(ErrorMessage = "Email cant be blank")]
        [EmailAddress(ErrorMessage = "Email should be in a proper email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone cant be blank")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Phone cant be blank")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Password cant be blank")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "ConfirmPassword cant be blank")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "password and Password confirm should match")]
        public string ConfirmPassword { get; set; }

        public UserTypeOptions UserType { get; set; } = UserTypeOptions.User;
    }
}

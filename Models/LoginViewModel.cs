using System.ComponentModel.DataAnnotations;

namespace IdentityProject.Models
{
    public class LoginViewModel
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(dataType: DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}

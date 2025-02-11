using System.ComponentModel.DataAnnotations;

namespace IdentityProject.Models
{
    public class EmailMFA
    {
        [Required]
        [Display(Name = "Security Code")]
        public string SecurityCode { get; set; }
        public bool RememberMe { get; set; }
    }
}

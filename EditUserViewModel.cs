using System.ComponentModel.DataAnnotations;

public class EditUserViewModel
{
    public string UserId { get; set; }

    [Required]
    [Display(Name = "Username")]
    public string Username { get; set; }

    [Required]
    [Display(Name = "Name")]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Required]
    [Display(Name = "Role")]
    public string Role { get; set; }
}

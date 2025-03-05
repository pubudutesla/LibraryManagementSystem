using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Application.DTOs
{
    public class MemberRegistrationDto
    {
        [Required]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string MembershipType { get; set; } = string.Empty;
    }

    public class MemberUpdateDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? Password { get; set; }

        public string? MembershipType { get; set; }
    }
}
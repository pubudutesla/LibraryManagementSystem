using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Domain.Entities
{
    [Index(nameof(Username), IsUnique = true)] 
    public class Member
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } 

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; } 

        [Required]
        public MembershipType MembershipType { get; set; }
    }

    public enum MembershipType
    {
        Admin,       // Full control over the system
        Librarian,   // Can manage books and loans
        Member       // Can borrow books
    }
}
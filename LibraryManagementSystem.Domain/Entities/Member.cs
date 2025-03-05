using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Domain.Entities
{
    [Index(nameof(Username), IsUnique = true)]
    public class Member
    {
        [Key]
        public int Id { get; private set; }

        [Required, StringLength(50)]
        public string Username { get; private set; }

        [Required]
        public string Name { get; private set; }

        [Required, EmailAddress]
        public string Email { get; private set; }

        [Required]
        public string PasswordHash { get; private set; }

        [Required]
        public MembershipType MembershipType { get; private set; }

        // EF private constructor
        private Member() { }

        // Domain constructor ensuring valid data
        public Member(string username, string name, string email, string passwordHash, MembershipType membershipType)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username is required.");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Member name is required.");

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.");

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("PasswordHash is required.");

            Username = username.ToLower().Trim(); // normalized
            Name = name.Trim();
            Email = email.Trim();
            PasswordHash = passwordHash;
            MembershipType = membershipType;
        }

        public void Update(string? newName, string? newEmail, string? newPasswordHash, MembershipType? newMembershipType)
        {
            if (!string.IsNullOrWhiteSpace(newName))
                Name = newName.Trim();

            if (!string.IsNullOrWhiteSpace(newEmail))
                Email = newEmail.Trim();

            if (!string.IsNullOrWhiteSpace(newPasswordHash))
                PasswordHash = newPasswordHash;

            if (newMembershipType.HasValue)
                MembershipType = newMembershipType.Value;
        }
    }

    public enum MembershipType
    {
        Admin,       // Full control over the system
        Librarian,   // Can manage books and loans
        Member       // Can borrow books
    }
}
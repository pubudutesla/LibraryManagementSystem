using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Infrastructure.Persistence;
using LibraryManagementSystem.Infrastructure.Repositories;

namespace LibraryManagementSystem.Tests.Repositories
{
    public class LoanRepositoryTests : IDisposable
    {
        private readonly LibraryDbContext _dbContext;
        private readonly LoanRepository _loanRepository;

        public LoanRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<LibraryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new LibraryDbContext(options);
            _loanRepository = new LoanRepository(_dbContext);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            // Ensure clean DB before seeding
            _dbContext.Loans.RemoveRange(_dbContext.Loans);
            _dbContext.Books.RemoveRange(_dbContext.Books);
            _dbContext.Members.RemoveRange(_dbContext.Members);
            _dbContext.SaveChanges();

            // 📚 Add Books
            var books = new List<Book>
            {
                new Book { Id = 1, Title = "C# Basics", Author = "John Doe", Genre = "Programming", ISBN = "1234567890", AvailableCopies = 5, PublicationYear = 2022 },
                new Book { Id = 2, Title = "Advanced C#", Author = "Jane Doe", Genre = "Technology", ISBN = "1234567888", AvailableCopies = 13, PublicationYear = 2021 }
            };

            _dbContext.Books.AddRange(books);
            _dbContext.SaveChanges();

            // 🧑 Add Members with Correct Hashing
            var member = new Member
            {
                Id = 1,
                Username = "testmember", 
                Name = "Test Member",
                Email = "testmember@example.com",
                PasswordHash = HashPassword("Test@123"), 
                MembershipType = MembershipType.Member
            };

            var librarian = new Member
            {
                Id = 2,
                Username = "librarianuser",
                Name = "Librarian User",
                Email = "librarian@example.com",
                PasswordHash = HashPassword("Librarian@123"),
                MembershipType = MembershipType.Librarian
            };

            var admin = new Member
            {
                Id = 3,
                Username = "adminuser",
                Name = "Admin User",
                Email = "admin@example.com",
                PasswordHash = HashPassword("Admin@123"),
                MembershipType = MembershipType.Admin
            };

            _dbContext.Members.AddRange(member, librarian, admin);
            _dbContext.SaveChanges();
        }

        /// <summary>
        /// Uses your existing password hashing method (Modify if needed)
        /// </summary>
        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        [Fact]
        public async Task AddAsync_ShouldAddLoanSuccessfully()
        {
            // Arrange
            var loan = new Loan
            {
                BookId = 1, // Ensure BookId exists
                MemberId = 1,
                LoanDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(14),
                ReturnDate = null
            };

            // Act
            await _loanRepository.AddAsync(loan);
            await _dbContext.SaveChangesAsync();  // **Ensure SaveChangesAsync is called**
            var retrievedLoan = await _loanRepository.GetByIdAsync(loan.Id);

            // Assert
            Assert.NotNull(retrievedLoan);
            Assert.Equal(loan.BookId, retrievedLoan.BookId);
            Assert.Equal(loan.MemberId, retrievedLoan.MemberId);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnLoan_WhenLoanExists()
        {
            // Arrange
            var loan = new Loan
            {
                BookId = 1,
                MemberId = 1,
                LoanDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(14)
            };

            await _loanRepository.AddAsync(loan);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _loanRepository.GetByIdAsync(loan.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(loan.Id, result.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenLoanDoesNotExist()
        {
            // Act
            var result = await _loanRepository.GetByIdAsync(999); // Non-existent Loan ID

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetLoansByMemberIdAsync_ShouldReturnLoans_ForGivenMember()
        {
            // Arrange
            var memberId = 1;
            var existingLoans = await _loanRepository.GetLoansByMemberIdAsync(memberId);
            var initialCount = existingLoans.Count(); // Get initial count to avoid duplicate counting

            var loan1 = new Loan { BookId = 1, MemberId = memberId, LoanDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(14) };
            var loan2 = new Loan { BookId = 2, MemberId = memberId, LoanDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(14) };

            await _loanRepository.AddAsync(loan1);
            await _loanRepository.AddAsync(loan2);
            await _dbContext.SaveChangesAsync();

            // Act
            var loans = await _loanRepository.GetLoansByMemberIdAsync(memberId);

            // Assert
            Assert.NotNull(loans);
            Assert.Equal(initialCount + 2, loans.Count()); // Ensure the new loans are counted correctly
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateLoanSuccessfully()
        {
            // Arrange
            var loan = new Loan
            {
                BookId = 1,
                MemberId = 1,
                LoanDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(14)
            };

            await _loanRepository.AddAsync(loan);
            await _dbContext.SaveChangesAsync();

            var existingLoan = await _loanRepository.GetByIdAsync(loan.Id);
            Assert.NotNull(existingLoan); // Ensure Loan Exists Before Update

            existingLoan.ReturnDate = DateTime.UtcNow;

            // Act
            await _loanRepository.UpdateAsync(existingLoan);
            await _dbContext.SaveChangesAsync();

            var updatedLoan = await _loanRepository.GetByIdAsync(existingLoan.Id);

            // Assert
            Assert.NotNull(updatedLoan);
            Assert.NotNull(updatedLoan.ReturnDate); // Ensure return date is updated
        }

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}
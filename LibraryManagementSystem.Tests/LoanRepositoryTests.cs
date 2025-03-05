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
                .UseInMemoryDatabase(databaseName: "LibraryTestDb_Loans")
                .Options;

            _dbContext = new LibraryDbContext(options);
            _loanRepository = new LoanRepository(_dbContext);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            _dbContext.Loans.RemoveRange(_dbContext.Loans);
            _dbContext.Books.RemoveRange(_dbContext.Books);
            _dbContext.Members.RemoveRange(_dbContext.Members);
            _dbContext.SaveChanges();

            // Add Books using your Book constructor
            var book1 = new Book("C# Basics", "John Doe", "1234567890", "Programming", 2022, 5);
            var book2 = new Book("Advanced C#", "Jane Doe", "1234567888", "Technology", 2021, 13);

            _dbContext.Books.AddRange(book1, book2);
            _dbContext.SaveChanges();

            // Add Members
            var member = new Member("testmember", "Test Member", "testmember@example.com", HashPassword("Test@123"), MembershipType.Member);
            var librarian = new Member("librarianuser", "Librarian User", "librarian@example.com", HashPassword("Librarian@123"), MembershipType.Librarian);
            var admin = new Member("adminuser", "Admin User", "admin@example.com", HashPassword("Admin@123"), MembershipType.Admin);

            _dbContext.Members.AddRange(member, librarian, admin);
            _dbContext.SaveChanges();
        }

        private string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        [Fact]
        public async Task AddAsync_ShouldAddLoanSuccessfully()
        {
            // Arrange
            var loan = new Loan(
                bookId: 1,
                memberId: 1,
                loanDate: DateTime.UtcNow,
                dueDate: DateTime.UtcNow.AddDays(14)
            );

            // Act
            await _loanRepository.AddAsync(loan);
            await _dbContext.SaveChangesAsync();

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
            var loan = new Loan(1, 1, DateTime.UtcNow, DateTime.UtcNow.AddDays(14));
            await _loanRepository.AddAsync(loan);
            await _dbContext.SaveChangesAsync();

            var loanId = loan.Id;

            // Act
            var result = await _loanRepository.GetByIdAsync(loanId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(loanId, result.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenLoanDoesNotExist()
        {
            // Act
            var result = await _loanRepository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetLoansByMemberIdAsync_ShouldReturnLoans_ForGivenMember()
        {
            // Arrange
            var memberId = 1;
            var existingLoans = await _loanRepository.GetLoansByMemberIdAsync(memberId);
            var initialCount = existingLoans.Count();

            var loan1 = new Loan(1, memberId, DateTime.UtcNow, DateTime.UtcNow.AddDays(14));
            var loan2 = new Loan(2, memberId, DateTime.UtcNow, DateTime.UtcNow.AddDays(14));

            await _loanRepository.AddAsync(loan1);
            await _loanRepository.AddAsync(loan2);
            await _dbContext.SaveChangesAsync();

            // Act
            var loans = await _loanRepository.GetLoansByMemberIdAsync(memberId);

            // Assert
            Assert.NotNull(loans);
            Assert.Equal(initialCount + 2, loans.Count());
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateLoanSuccessfully()
        {
            // Arrange
            var loan = new Loan(1, 1, DateTime.UtcNow, DateTime.UtcNow.AddDays(14));
            await _loanRepository.AddAsync(loan);
            await _dbContext.SaveChangesAsync();

            // Mark as returned
            loan.MarkAsReturned();

            // Act
            await _loanRepository.UpdateAsync(loan);
            await _dbContext.SaveChangesAsync();

            var updatedLoan = await _loanRepository.GetByIdAsync(loan.Id);

            // Assert
            Assert.NotNull(updatedLoan);
            Assert.NotNull(updatedLoan.ReturnDate);
        }

        // If you had special checks for "No available copies," that might be tested in the Service layer, 
        // because domain logic is typically enforced before repository. 
        // So we omit that test here, or adapt if needed.

        public void Dispose()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}
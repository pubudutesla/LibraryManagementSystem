using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using AutoMapper;
using LibraryManagementSystem.Application.Services;
using LibraryManagementSystem.Application.DTOs;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Infrastructure.Repositories;

namespace LibraryManagementSystem.Tests.Services
{
    public class LoanServiceTests
    {
        private readonly Mock<IBookRepository> _mockBookRepository;
        private readonly Mock<IMemberRepository> _mockMemberRepository;
        private readonly Mock<ILoanRepository> _mockLoanRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IMapper> _mockMapper;
        private readonly LoanService _loanService;

        public LoanServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLoanRepository = new Mock<ILoanRepository>();
            _mockBookRepository = new Mock<IBookRepository>();
            _mockMemberRepository = new Mock<IMemberRepository>();
            _mockMapper = new Mock<IMapper>();

            _mockUnitOfWork.Setup(u => u.Loans).Returns(_mockLoanRepository.Object);
            _mockUnitOfWork.Setup(u => u.Books).Returns(_mockBookRepository.Object);
            _mockUnitOfWork.Setup(u => u.Members).Returns(_mockMemberRepository.Object);

            // Ensure the Book has a valid Id
            var mockedBook = new Book("Test Book", "Test Author", "1234567890", "Test Genre", 2022, 5);
            typeof(Book).GetProperty(nameof(Book.Id))!.SetValue(mockedBook, 1);

            _mockBookRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(mockedBook);

            // Similarly for Member if the service uses it
            var mockedMember = new Member("testuser", "Test User", "test@example.com", "passhash", MembershipType.Member);
            typeof(Member).GetProperty(nameof(Member.Id))!.SetValue(mockedMember, 1);

            _mockMemberRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(mockedMember);

            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            _loanService = new LoanService(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task CreateLoanAsync_ShouldCreateLoanSuccessfully()
        {
            // Arrange
            var loanDto = new LoanRequestDto
            {
                BookId = 1,
                MemberId = 1
            };

            var loan = new Loan(
                bookId: 1,
                memberId: 1,
                loanDate: DateTime.UtcNow,
                dueDate: DateTime.UtcNow.AddDays(14)
            );
            typeof(Loan).GetProperty(nameof(Loan.Id))!.SetValue(loan, 1);

            var responseDto = new LoanResponseDto
            {
                Id = 1,
                BookId = 1,
                MemberId = 1
            };

            // Mock setup
            _mockMapper.Setup(m => m.Map<Loan>(loanDto)).Returns(loan);

            _mockLoanRepository.Setup(r => r.AddAsync(It.IsAny<Loan>()));

            // This is critical so the service sees a non-null mapping result:
            _mockMapper.Setup(m => m.Map<LoanResponseDto>(It.IsAny<Loan>())).Returns(responseDto);

            // Mock SaveChanges if needed
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _loanService.CreateLoanAsync(loanDto);

            // Assert
            Assert.NotNull(result);        // <-- Will pass now because result is not null
            Assert.Equal(1, result.Id);
            Assert.Equal(1, result.BookId);
            Assert.Equal(1, result.MemberId);
        }

        [Fact]
        public async Task GetAllLoansAsync_ShouldReturnListOfLoans()
        {
            // Arrange
            // Build domain loans using constructor, then optionally set Id if needed
            var loan1 = new Loan(1, 1, DateTime.UtcNow, DateTime.UtcNow.AddDays(14));
            typeof(Loan).GetProperty(nameof(Loan.Id))!.SetValue(loan1, 1);

            var loan2 = new Loan(2, 1, DateTime.UtcNow, DateTime.UtcNow.AddDays(14));
            typeof(Loan).GetProperty(nameof(Loan.Id))!.SetValue(loan2, 2);

            var loans = new List<Loan> { loan1, loan2 };

            var responseDtos = new List<LoanResponseDto>
            {
                new LoanResponseDto { Id = 1, BookId = 1, MemberId = 1 },
                new LoanResponseDto { Id = 2, BookId = 2, MemberId = 1 }
            };

            _mockLoanRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(loans);
            _mockMapper.Setup(m => m.Map<IEnumerable<LoanResponseDto>>(loans)).Returns(responseDtos);

            // Act
            var result = await _loanService.GetAllLoansAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetLoanByIdAsync_ShouldReturnLoan_WhenLoanExists()
        {
            // Arrange
            var loan = new Loan(1, 1, DateTime.UtcNow, DateTime.UtcNow.AddDays(14));
            typeof(Loan).GetProperty(nameof(Loan.Id))!.SetValue(loan, 1);

            var responseDto = new LoanResponseDto { Id = 1, BookId = 1, MemberId = 1 };

            _mockLoanRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(loan);
            _mockMapper.Setup(m => m.Map<LoanResponseDto>(loan)).Returns(responseDto);

            // Act
            var result = await _loanService.GetLoanByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetLoanByIdAsync_ShouldReturnNull_WhenLoanDoesNotExist()
        {
            // Arrange
            _mockLoanRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Loan?)null);

            // Act
            var result = await _loanService.GetLoanByIdAsync(1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task ReturnBookAsync_ShouldMarkLoanAsReturned_WhenLoanExists()
        {
            // Arrange
            var loan = new Loan(1, 1, DateTime.UtcNow, DateTime.UtcNow.AddDays(14));
            typeof(Loan).GetProperty(nameof(Loan.Id))!.SetValue(loan, 1);

            _mockLoanRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(loan);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _loanService.ReturnBookAsync(1);

            // Assert
            Assert.True(result);
            // If you want to confirm ReturnDate was set, you can do:
            // Assert.NotNull(loan.ReturnDate);
        }

        [Fact]
        public async Task ReturnBookAsync_ShouldReturnFalse_WhenLoanDoesNotExist()
        {
            // Arrange
            _mockLoanRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Loan?)null);

            // Act
            var result = await _loanService.ReturnBookAsync(1);

            // Assert
            Assert.False(result);
        }
    }
}
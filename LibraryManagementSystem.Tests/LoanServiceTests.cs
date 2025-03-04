using System.Collections.Generic;
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
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ILoanRepository> _mockLoanRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly LoanService _loanService;

        public LoanServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockLoanRepository = new Mock<ILoanRepository>();
            _mockMapper = new Mock<IMapper>();

            _mockUnitOfWork.Setup(u => u.Loans).Returns(_mockLoanRepository.Object);

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

            var loan = new Loan
            {
                Id = 1,
                BookId = 1,
                MemberId = 1
            };

            var responseDto = new LoanResponseDto
            {
                Id = 1,
                BookId = 1,
                MemberId = 1
            };

            _mockMapper.Setup(m => m.Map<Loan>(loanDto)).Returns(loan);
            _mockLoanRepository.Setup(r => r.AddAsync(It.IsAny<Loan>()));
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            _mockMapper.Setup(m => m.Map<LoanResponseDto>(loan)).Returns(responseDto);

            // Act
            var result = await _loanService.CreateLoanAsync(loanDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal(1, result.BookId);
            Assert.Equal(1, result.MemberId);
        }

        [Fact]
        public async Task GetAllLoansAsync_ShouldReturnListOfLoans()
        {
            // Arrange
            var loans = new List<Loan>
            {
                new Loan { Id = 1, BookId = 1, MemberId = 1 },
                new Loan { Id = 2, BookId = 2, MemberId = 1 }
            };

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
            var loan = new Loan { Id = 1, BookId = 1, MemberId = 1 };
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
            var loan = new Loan { Id = 1, BookId = 1, MemberId = 1, ReturnDate = null };

            _mockLoanRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(loan);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _loanService.ReturnBookAsync(1);

            // Assert
            Assert.True(result);
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
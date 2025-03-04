using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Api.Controllers;
using LibraryManagementSystem.Application.Services;
using LibraryManagementSystem.Application.DTOs;

namespace LibraryManagementSystem.Tests.Controllers
{
    public class LoanControllerTests
    {
        private readonly Mock<ILoanService> _mockLoanService;
        private readonly LoanController _controller;

        public LoanControllerTests()
        {
            _mockLoanService = new Mock<ILoanService>();
            _controller = new LoanController(_mockLoanService.Object);
        }

        [Fact]
        public async Task GetLoans_ShouldReturnOk_WithLoans()
        {
            // Arrange
            var loans = new List<LoanResponseDto>
            {
                new LoanResponseDto { Id = 1, BookId = 1, MemberId = 1 },
                new LoanResponseDto { Id = 2, BookId = 2, MemberId = 1 }
            };

            _mockLoanService.Setup(s => s.GetAllLoansAsync()).ReturnsAsync(loans);

            // Act
            var result = await _controller.GetLoans();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnLoans = Assert.IsAssignableFrom<IEnumerable<LoanResponseDto>>(okResult.Value);
            Assert.Equal(2, returnLoans.Count());
        }

        [Fact]
        public async Task GetLoanById_ShouldReturnNotFound_WhenLoanDoesNotExist()
        {
            // Arrange
            _mockLoanService.Setup(s => s.GetLoanByIdAsync(It.IsAny<int>())).ReturnsAsync((LoanResponseDto?)null);

            // Act
            var result = await _controller.GetLoanById(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateLoan_ShouldReturnCreatedAtAction_WhenSuccessful()
        {
            // Arrange
            var loanRequestDto = new LoanRequestDto { BookId = 1, MemberId = 1 };
            var loanResponseDto = new LoanResponseDto { Id = 1, BookId = 1, MemberId = 1 };

            _mockLoanService.Setup(s => s.CreateLoanAsync(It.IsAny<LoanRequestDto>())).ReturnsAsync(loanResponseDto);

            // Act
            var result = await _controller.CreateLoan(loanRequestDto);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal("GetLoanById", actionResult.ActionName);
        }

        [Fact]
        public async Task ReturnBook_ShouldReturnNoContent_WhenSuccessful()
        {
            // Arrange
            _mockLoanService.Setup(s => s.ReturnBookAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _controller.ReturnBook(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task ReturnBook_ShouldReturnNotFound_WhenLoanDoesNotExist()
        {
            // Arrange
            _mockLoanService.Setup(s => s.ReturnBookAsync(It.IsAny<int>())).ReturnsAsync(false);

            // Act
            var result = await _controller.ReturnBook(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
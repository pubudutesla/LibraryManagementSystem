using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Api.Controllers;
using LibraryManagementSystem.Application.Services;
using LibraryManagementSystem.Application.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using LibraryManagementSystem.Application.Common;

namespace LibraryManagementSystem.Tests.Controllers
{
    public class BookControllerTests
    {
        private readonly Mock<IBookService> _mockBookService;
        private readonly BookController _controller;
        private readonly Mock<ILogger<BookController>> _mockLogger;
        private readonly Mock<IAuthorizationService> _mockAuthorizationService;

        public BookControllerTests()
        {
            _mockBookService = new Mock<IBookService>();
            _mockLogger = new Mock<ILogger<BookController>>();
            _mockAuthorizationService = new Mock<IAuthorizationService>();

            // Ensure that every authorization check in the controller always 'succeeds' for these tests.
            _mockAuthorizationService
                .Setup(x => x.AuthorizeAsync(
                    It.IsAny<ClaimsPrincipal>(),
                    It.IsAny<object>(),
                    It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
                .ReturnsAsync(AuthorizationResult.Success());

            _controller = new BookController(
                _mockBookService.Object,
                _mockLogger.Object,
                _mockAuthorizationService.Object
            );
        }

        [Fact]
        public async Task GetBooks_ShouldReturnOk_WithBooks()
        {
            // Arrange
            var books = new List<BookDto>
            {
                new BookDto { Id = 1, Title = "Book A", Author = "Author A" },
                new BookDto { Id = 2, Title = "Book B", Author = "Author B" }
            };

            _mockBookService.Setup(s => s.GetAllBooksAsync()).ReturnsAsync(books);

            // Act
            var result = await _controller.GetBooks();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            // The controller returns ApiResponse<IEnumerable<BookDto>>
            var apiResponse = Assert.IsAssignableFrom<ApiResponse<IEnumerable<BookDto>>>(okResult.Value);
            Assert.NotNull(apiResponse.Data);
            Assert.Equal(2, apiResponse.Data.Count());
        }

        [Fact]
        public async Task GetBookById_ShouldReturnNotFound_WhenBookDoesNotExist()
        {
            // Arrange
            _mockBookService.Setup(s => s.GetBookByIdAsync(It.IsAny<int>()))
                            .ReturnsAsync((BookDto)null);

            // Act
            var result = await _controller.GetBookById(999);

            // Assert
            // Controller calls: NotFound(ApiResponse<BookDto>.ErrorResponse(...))
            // which is a NotFoundObjectResult
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var apiResponse = Assert.IsAssignableFrom<ApiResponse<BookDto>>(notFoundResult.Value);
            Assert.False(apiResponse.Success);
        }

        [Fact]
        public async Task AddBook_ShouldReturnCreatedAtAction_WhenSuccessful()
        {
            // Arrange
            var bookDto = new BookDto
            {
                Id = 1,
                Title = "New Book",
                Author = "Author",
                ISBN = "9876543210",
                Genre = "Non-Fiction"
            };

            _mockBookService.Setup(s => s.AddBookAsync(It.IsAny<BookDto>()))
                            .ReturnsAsync(bookDto);

            // Act
            var result = await _controller.AddBook(bookDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal("GetBookById", createdResult.ActionName);

            var apiResponse = Assert.IsAssignableFrom<ApiResponse<BookDto>>(createdResult.Value);
            Assert.True(apiResponse.Success);
            Assert.Equal(bookDto.Id, apiResponse.Data.Id);
        }

        [Fact]
        public async Task UpdateBook_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            var bookDto = new BookDto { Id = 1, Title = "Updated Book" };
            _mockBookService.Setup(s => s.UpdateBookAsync(1, bookDto))
                            .ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateBook(1, bookDto);

            // Controller returns Ok(ApiResponse<object>) on success
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsAssignableFrom<ApiResponse<object>>(okResult.Value);
            Assert.True(apiResponse.Success);
        }

        [Fact]
        public async Task DeleteBook_ShouldReturnOk_WhenSuccessful()
        {
            // Arrange
            _mockBookService.Setup(s => s.DeleteBookAsync(1))
                            .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteBook(1);

            // Controller returns Ok(ApiResponse<object>) on success
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var apiResponse = Assert.IsAssignableFrom<ApiResponse<object>>(okResult.Value);
            Assert.True(apiResponse.Success);
        }
    }
}
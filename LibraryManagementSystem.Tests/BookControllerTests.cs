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
    public class BookControllerTests
    {
        private readonly Mock<IBookService> _mockBookService;
        private readonly BookController _controller;

        public BookControllerTests()
        {
            _mockBookService = new Mock<IBookService>();
            _controller = new BookController(_mockBookService.Object);
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
            var returnBooks = Assert.IsAssignableFrom<IEnumerable<BookDto>>(okResult.Value);
            Assert.Equal(2, returnBooks.Count());
        }

        [Fact]
        public async Task GetBookById_ShouldReturnNotFound_WhenBookDoesNotExist()
        {
            // Arrange
            _mockBookService.Setup(s => s.GetBookByIdAsync(It.IsAny<int>())).ReturnsAsync((BookDto)null);

            // Act
            var result = await _controller.GetBookById(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
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

            _mockBookService.Setup(s => s.AddBookAsync(It.IsAny<BookDto>())).ReturnsAsync(bookDto);

            // Act
            var result = await _controller.AddBook(bookDto);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal("GetBookById", actionResult.ActionName);
        }

        [Fact]
        public async Task UpdateBook_ShouldReturnNoContent_WhenSuccessful()
        {
            // Arrange
            var bookDto = new BookDto { Id = 1, Title = "Updated Book" };
            _mockBookService.Setup(s => s.UpdateBookAsync(1, bookDto)).ReturnsAsync(true);

            // Act
            var result = await _controller.UpdateBook(1, bookDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteBook_ShouldReturnNoContent_WhenSuccessful()
        {
            // Arrange
            _mockBookService.Setup(s => s.DeleteBookAsync(1)).ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteBook(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
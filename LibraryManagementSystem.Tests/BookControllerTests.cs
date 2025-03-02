using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Api.Controllers;
using LibraryManagementSystem.Application.Services;
using LibraryManagementSystem.Domain.Entities;

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
            var books = new List<Book>
            {
                new Book { Id = 1, Title = "Book A", Author = "Author A" },
                new Book { Id = 2, Title = "Book B", Author = "Author B" }
            };

            _mockBookService.Setup(s => s.GetAllBooksAsync()).ReturnsAsync(books);

            // Act
            var result = await _controller.GetBooks();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnBooks = Assert.IsAssignableFrom<IEnumerable<Book>>(okResult.Value);
            Assert.Equal(2, returnBooks.Count());
        }

        [Fact]
        public async Task GetBookById_ShouldReturnNotFound_WhenBookDoesNotExist()
        {
            // Arrange
            _mockBookService.Setup(s => s.GetBookByIdAsync(It.IsAny<int>())).ReturnsAsync((Book)null);

            // Act
            var result = await _controller.GetBookById(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task AddBook_ShouldReturnCreatedAtAction_WhenSuccessful()
        {
            // Arrange
            var newBook = new Book { Id = 3, Title = "New Book", Author = "Author C" };
            _mockBookService.Setup(s => s.AddBookAsync(It.IsAny<Book>())).ReturnsAsync(newBook);

            // Act
            var result = await _controller.AddBook(newBook);

            // Assert
            var createdAtAction = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnBook = Assert.IsType<Book>(createdAtAction.Value);
            Assert.Equal(3, returnBook.Id);
        }

        [Fact]
        public async Task UpdateBook_ShouldReturnNoContent_WhenSuccessful()
        {
            // Arrange
            var existingBook = new Book { Id = 1, Title = "Test Book" };
            _mockBookService.Setup(s => s.GetBookByIdAsync(1)).ReturnsAsync(existingBook);
            _mockBookService.Setup(s => s.UpdateBookAsync(existingBook)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateBook(1, existingBook);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteBook_ShouldReturnNoContent_WhenSuccessful()
        {
            // Arrange
            var book = new Book { Id = 1, Title = "Test Book" };
            _mockBookService.Setup(s => s.GetBookByIdAsync(1)).ReturnsAsync(book);
            _mockBookService.Setup(s => s.DeleteBookAsync(1)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteBook(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
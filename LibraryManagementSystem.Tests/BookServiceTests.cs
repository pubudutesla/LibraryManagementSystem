using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using LibraryManagementSystem.Application.Services;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Infrastructure.Repositories;

namespace LibraryManagementSystem.Tests.Services
{
    public class BookServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IBookRepository> _mockBookRepository;
        private readonly BookService _bookService;

        public BookServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockBookRepository = new Mock<IBookRepository>();

            // Ensure UoW returns the mocked repository
            _mockUnitOfWork.Setup(u => u.Books).Returns(_mockBookRepository.Object);

            // ✅ Pass both repository and UoW to match the constructor
            _bookService = new BookService(_mockBookRepository.Object, _mockUnitOfWork.Object);
        }

        [Fact]
        public async Task GetAllBooksAsync_ShouldReturnAllBooks()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { Id = 1, Title = "Book One", Author = "Author A" },
                new Book { Id = 2, Title = "Book Two", Author = "Author B" }
            };
            _mockBookRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(books);

            // Act
            var result = await _bookService.GetAllBooksAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetBookByIdAsync_ShouldReturnBook_WhenBookExists()
        {
            // Arrange
            var book = new Book { Id = 1, Title = "Test Book", Author = "Author A" };
            _mockBookRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);

            // Act
            var result = await _bookService.GetBookByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Book", result.Title);
        }

        [Fact]
        public async Task GetBookByIdAsync_ShouldReturnNull_WhenBookDoesNotExist()
        {
            // Arrange
            _mockBookRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Book)null);

            // Act
            var result = await _bookService.GetBookByIdAsync(99);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddBookAsync_ShouldAddBookSuccessfully()
        {
            // Arrange
            var newBook = new Book { Id = 3, Title = "New Book", Author = "Author C" };
            _mockBookRepository.Setup(r => r.AddAsync(newBook)).ReturnsAsync(newBook);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _bookService.AddBookAsync(newBook);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Book", result.Title);
        }

        [Fact]
        public async Task UpdateBookAsync_ShouldUpdateExistingBook()
        {
            // Arrange
            var book = new Book { Id = 1, Title = "Updated Book", Author = "Author X" };
            _mockBookRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await _bookService.UpdateBookAsync(book);

            // Assert (No exception means test passed)
            _mockBookRepository.Verify(r => r.UpdateAsync(book), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateBookAsync_ShouldThrowException_WhenBookDoesNotExist()
        {
            // Arrange: Ensure the repository returns `null`
            var bookToUpdate = new Book { Id = 1, Title = "New Title", Author = "New Author" };

            _mockBookRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Book)null);

            // Act & Assert: Expect KeyNotFoundException when calling UpdateBookAsync
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _bookService.UpdateBookAsync(bookToUpdate));
        }

        [Fact]
        public async Task DeleteBookAsync_ShouldDeleteBook_WhenBookExists()
        {
            // Arrange
            var bookId = 1;
            _mockBookRepository.Setup(r => r.DeleteAsync(bookId)).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            await _bookService.DeleteBookAsync(bookId);

            // Assert
            _mockBookRepository.Verify(r => r.DeleteAsync(bookId), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}
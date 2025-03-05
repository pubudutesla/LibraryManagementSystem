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
    public class BookServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IBookRepository> _mockBookRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly BookService _bookService;

        public BookServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockBookRepository = new Mock<IBookRepository>();
            _mockMapper = new Mock<IMapper>();

            _mockUnitOfWork.Setup(u => u.Books).Returns(_mockBookRepository.Object);

            _bookService = new BookService(_mockUnitOfWork.Object, _mockMapper.Object);
        }

        [Fact]
        public async Task AddBookAsync_ShouldAddBookSuccessfully()
        {
            // Arrange: Ensure Title, Author, and ISBN are not empty
            var bookDto = new BookDto
            {
                Id = 3,
                Title = "New Book",
                Author = "Author C",
                ISBN = "1234567890",
                Genre = "Fiction",
                PublicationYear = 2022,
                AvailableCopies = 5
            };

            // We create the domain Book matching the above fields:
            var book = new Book("New Book", "Author C", "1234567890", "Fiction", 2022, 5);

            _mockMapper.Setup(m => m.Map<Book>(bookDto)).Returns(book);
            _mockBookRepository.Setup(r => r.AddAsync(It.IsAny<Book>())).ReturnsAsync(book);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            _mockMapper.Setup(m => m.Map<BookDto>(book)).Returns(bookDto);

            // Act
            var result = await _bookService.AddBookAsync(bookDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Book", result.Title);
            Assert.Equal("Author C", result.Author);
            Assert.Equal("1234567890", result.ISBN);
        }

        [Fact]
        public async Task UpdateBookAsync_ShouldUpdateExistingBook()
        {
            // Arrange: Ensure Title, Author, and ISBN are not empty
            var bookDto = new BookDto
            {
                Id = 3,
                Title = "New Book",
                Author = "Author C",
                ISBN = "1234567890",
                Genre = "Fiction",
                PublicationYear = 2022,
                AvailableCopies = 5
            };

            // We create the domain Book matching the above fields:
            var book = new Book("New Book", "Author C", "1234567890", "Fiction", 2022, 5);

            _mockBookRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);
            _mockMapper.Setup(m => m.Map(bookDto, book));
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _bookService.UpdateBookAsync(1, bookDto);

            // Assert
            Assert.True(result);
            _mockBookRepository.Verify(r => r.UpdateAsync(book), Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Infrastructure.Persistence;
using LibraryManagementSystem.Infrastructure.Repositories;
using LibraryManagementSystem.Domain.Entities;

namespace LibraryManagementSystem.Tests.Repositories
{
    public class BookRepositoryTests
    {
        private readonly LibraryDbContext _context;
        private readonly BookRepository _bookRepository;

        public BookRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<LibraryDbContext>()
                .UseInMemoryDatabase(databaseName: "LibraryTestDb")
                .Options;

            _context = new LibraryDbContext(options);
            _bookRepository = new BookRepository(_context);
        }

        [Fact]
        public async Task AddAsync_ShouldAddBook()
        {
            // Arrange
            var book = new Book
            {
                Id = 1,
                Title = "Book One",
                Author = "Author A",
                ISBN = "1234567890",
                Genre = "Fiction"
            };

            // Act
            await _bookRepository.AddAsync(book);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _bookRepository.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal("Book One", result.Title);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnBook_WhenExists()
        {
            var book = new Book
            {
                Title = "Test Book",
                Author = "John Doe",
                ISBN = "987-6543210987",
                Genre = "Science",
                PublicationYear = 2021,
                AvailableCopies = 10
            };
            var addedBook = await _bookRepository.AddAsync(book);
            await _context.SaveChangesAsync();

            // Use the actual generated Id
            var result = await _bookRepository.GetByIdAsync(addedBook.Id);
            Assert.NotNull(result);
            Assert.Equal(addedBook.Id, result.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenBookDoesNotExist()
        {
            // Act
            var result = await _bookRepository.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveBook()
        {
            // Arrange
            var book = new Book
            {
                Id = 1,
                Title = "Test Book",
                Author = "John Doe",
                ISBN = "123-4567890123",
                Genre = "Fiction", 
                PublicationYear = 2022,
                AvailableCopies = 5
            };
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            // Act
            await _bookRepository.DeleteAsync(3);
            await _context.SaveChangesAsync();

            // Assert
            var deletedBook = await _context.Books.FindAsync(3);
            Assert.Null(deletedBook);
        }
    }
}
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
                .UseInMemoryDatabase(databaseName: "LibraryTestDb_Books")
                .Options;

            _context = new LibraryDbContext(options);
            _bookRepository = new BookRepository(_context);
        }

        [Fact]
        public async Task AddAsync_ShouldAddBook()
        {
            // Arrange: Use the domain constructor
            var book = new Book(
                title: "Book One",
                author: "Author A",
                isbn: "1234567890",
                genre: "Fiction",
                publicationYear: 2022,
                availableCopies: 5
            );

            // Act
            await _bookRepository.AddAsync(book);
            await _context.SaveChangesAsync();

            // The ID is assigned by EF after save
            var result = await _bookRepository.GetByIdAsync(book.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Book One", result.Title);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnBook_WhenExists()
        {
            // Arrange: create & save a new book
            var book = new Book(
                title: "Test Book",
                author: "John Doe",
                isbn: "987-6543210987",
                genre: "Science",
                publicationYear: 2021,
                availableCopies: 10
            );
            await _bookRepository.AddAsync(book);
            await _context.SaveChangesAsync();

            // The ID is assigned now
            var bookId = book.Id;

            // Act
            var result = await _bookRepository.GetByIdAsync(bookId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(bookId, result.Id);
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
            var book = new Book(
                title: "Test Book",
                author: "John Doe",
                isbn: "123-4567890123",
                genre: "Fiction",
                publicationYear: 2022,
                availableCopies: 5
            );

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            // Now we have a valid ID
            var bookId = book.Id;

            // Act
            await _bookRepository.DeleteAsync(bookId);
            await _context.SaveChangesAsync();

            // Assert
            var deletedBook = await _context.Books.FindAsync(bookId);
            Assert.Null(deletedBook);
        }
    }
}
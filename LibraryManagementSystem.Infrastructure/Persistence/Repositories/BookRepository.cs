using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Infrastructure.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly LibraryDbContext _context;
        public BookRepository(LibraryDbContext context) => _context = context;

        public async Task<IEnumerable<Book>> GetAllAsync() => await _context.Books.ToListAsync();

        public async Task<Book?> GetByIdAsync(int id) => await _context.Books.FindAsync(id);

        public async Task<Book?> GetByISBNAsync(string isbn) =>
            await _context.Books.FirstOrDefaultAsync(b => b.ISBN == isbn);

        public async Task<Book> AddAsync(Book book)
        {
            await _context.Books.AddAsync(book);
            return book;
        }

        public async Task UpdateAsync(Book book)
        {
            _context.Books.Update(book);
        }

        public async Task DeleteAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
            }
        }
    }
}
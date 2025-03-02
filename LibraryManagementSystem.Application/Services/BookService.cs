using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Infrastructure.Repositories;

namespace LibraryManagementSystem.Application.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IUnitOfWork _unitOfWork;

        public BookService(IBookRepository bookRepository, IUnitOfWork unitOfWork)
        {
            _bookRepository = bookRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Book>> GetAllBooksAsync() => await _bookRepository.GetAllAsync();

        public async Task<Book> GetBookByIdAsync(int id) => await _bookRepository.GetByIdAsync(id);

        public async Task<Book> AddBookAsync(Book book)
        {
            var existingBook = await _bookRepository.GetByISBNAsync(book.ISBN);
            if (existingBook != null)
            {
                throw new InvalidOperationException($"A book with ISBN '{book.ISBN}' already exists.");
            }

            var newBook = await _bookRepository.AddAsync(book);
            await _unitOfWork.SaveChangesAsync();
            return newBook;
        }

        public async Task UpdateBookAsync(Book book)
        {
            var existingBook = await _bookRepository.GetByIdAsync(book.Id);
            if (existingBook == null) throw new KeyNotFoundException("Book not found");

            await _bookRepository.UpdateAsync(book);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteBookAsync(int id)
        {
            await _bookRepository.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
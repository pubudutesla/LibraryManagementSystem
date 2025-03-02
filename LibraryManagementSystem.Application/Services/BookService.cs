using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Infrastructure.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Application.Services
{
    public class BookService : IBookService
    {//sdsdd
        private readonly IUnitOfWork _unitOfWork;

        public BookService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<IEnumerable<Book>> GetAllBooksAsync() => await _unitOfWork.Books.GetAllAsync();

        public async Task<Book> GetBookByIdAsync(int id) => await _unitOfWork.Books.GetByIdAsync(id);

        public async Task<Book> AddBookAsync(Book book)
        {
            var newBook = await _unitOfWork.Books.AddAsync(book);
            await _unitOfWork.SaveChangesAsync();
            return newBook;
        }

        public async Task UpdateBookAsync(Book book)
        {
            await _unitOfWork.Books.UpdateAsync(book);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteBookAsync(int id)
        {
            await _unitOfWork.Books.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
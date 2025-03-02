using AutoMapper;
using LibraryManagementSystem.Application.DTOs;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Infrastructure.Repositories;

namespace LibraryManagementSystem.Application.Services
{
    public class BookService : IBookService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public BookService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BookDto>> GetAllBooksAsync()
        {
            var books = await _unitOfWork.Books.GetAllAsync();
            return _mapper.Map<IEnumerable<BookDto>>(books);
        }

        public async Task<BookDto?> GetBookByIdAsync(int id)
        {
            var book = await _unitOfWork.Books.GetByIdAsync(id);
            return book == null ? null : _mapper.Map<BookDto>(book);
        }

        public async Task<BookDto> AddBookAsync(BookDto bookDto)
        {
            if (string.IsNullOrEmpty(bookDto.Title) || string.IsNullOrEmpty(bookDto.Author) || string.IsNullOrEmpty(bookDto.ISBN))
                throw new ArgumentException("Title, Author, and ISBN are required fields.");

            var existingBook = await _unitOfWork.Books.GetByISBNAsync(bookDto.ISBN);
            if (existingBook != null)
                throw new InvalidOperationException("A book with this ISBN already exists.");

            bookDto.Id = null;

            var book = _mapper.Map<Book>(bookDto);
            var addedBook = await _unitOfWork.Books.AddAsync(book);
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<BookDto>(addedBook);
        }

        public async Task<bool> UpdateBookAsync(int id, BookDto bookDto)
        {
            var existingBook = await _unitOfWork.Books.GetByIdAsync(id);
            if (existingBook == null) return false;

            if (string.IsNullOrEmpty(bookDto.Title) || string.IsNullOrEmpty(bookDto.Author) || string.IsNullOrEmpty(bookDto.ISBN))
                throw new ArgumentException("Title, Author, and ISBN are required fields.");

            var bookWithSameISBN = await _unitOfWork.Books.GetByISBNAsync(bookDto.ISBN);
            if (bookWithSameISBN != null && bookWithSameISBN.Id != id)
                throw new InvalidOperationException("A book with this ISBN already exists.");

            _mapper.Map(bookDto, existingBook);
            await _unitOfWork.Books.UpdateAsync(existingBook);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            var existingBook = await _unitOfWork.Books.GetByIdAsync(id);
            if (existingBook == null) return false;

            await _unitOfWork.Books.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

    }
}
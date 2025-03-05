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
            ValidateBookDto(bookDto);

            // Check duplicate ISBN
            var existingBook = await _unitOfWork.Books.GetByISBNAsync(bookDto.ISBN);
            if (existingBook != null)
                throw new InvalidOperationException("A book with this ISBN already exists.");

            // In case the DTO has an ID set incorrectly
            bookDto.Id = null;

            var book = _mapper.Map<Book>(bookDto);

            var addedBook = await _unitOfWork.Books.AddAsync(book);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<BookDto>(addedBook);
        }

        public async Task<bool> UpdateBookAsync(int id, BookDto bookDto)
        {
            ValidateBookDto(bookDto);

            var existingBook = await _unitOfWork.Books.GetByIdAsync(id);
            if (existingBook == null)
            {
                return false; // The controller returns 404
            }

            // Check if new ISBN conflicts with a different book
            var bookWithSameIsbn = await _unitOfWork.Books.GetByISBNAsync(bookDto.ISBN);
            if (bookWithSameIsbn != null && bookWithSameIsbn.Id != id)
                throw new InvalidOperationException("A book with this ISBN already exists.");

            // Map new values onto the entity
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

        private void ValidateBookDto(BookDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Title is required.");

            if (string.IsNullOrWhiteSpace(dto.Author))
                throw new ArgumentException("Author is required.");

            if (string.IsNullOrWhiteSpace(dto.ISBN))
                throw new ArgumentException("ISBN is required.");
        }
    }
}
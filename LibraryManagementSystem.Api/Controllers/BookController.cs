using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Application.Services;
using LibraryManagementSystem.Application.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace LibraryManagementSystem.Api.Controllers
{
    [Authorize]
    [Route("api/books")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBookService _service;
        private readonly ILogger<BookController> _logger;

        public BookController(IBookService service, ILogger<BookController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks()
        {
            _logger.LogInformation("Fetching all books");
            var books = await _service.GetAllBooksAsync();
            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookDto>> GetBookById(int id)
        {
            _logger.LogInformation("Fetching book with ID {Id}", id);
            var book = await _service.GetBookByIdAsync(id);
            if (book == null)
            {
                _logger.LogWarning("Book with ID {Id} not found", id);
                return NotFound();
            }
            return Ok(book);
        }

        // Requires 'Librarian' or 'Admin' role
        [Authorize(Policy = "LibrarianOrAdmin")]
        [HttpPost]
        public async Task<ActionResult<BookDto>> AddBook(BookDto bookDto)
        {
            _logger.LogInformation("Adding new book: {Title}", bookDto.Title);
            var addedBook = await _service.AddBookAsync(bookDto);
            return CreatedAtAction(nameof(GetBookById), new { id = addedBook.Id }, addedBook);
        }

        // Requires 'Admin' role
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, BookDto bookDto)
        {
            _logger.LogInformation("Updating book with ID {Id}", id);
            var success = await _service.UpdateBookAsync(id, bookDto);
            if (!success)
            {
                _logger.LogError("Failed to update book with ID {Id}. Not found.", id);
                return NotFound();
            }
            return NoContent();
        }

        // Requires 'Admin' role
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            _logger.LogInformation("Deleting book with ID {Id}", id);
            var success = await _service.DeleteBookAsync(id);
            if (!success)
            {
                _logger.LogError("Failed to delete book with ID {Id}. Not found.", id);
                return NotFound();
            }
            return NoContent();
        }
    }
}
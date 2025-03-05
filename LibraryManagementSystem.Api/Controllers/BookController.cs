using LibraryManagementSystem.Application.DTOs;
using LibraryManagementSystem.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
        [EnableRateLimiting("GetBooksLimiter")]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks()
        {
            _logger.LogInformation("Fetching all books");
            try
            {
                var books = await _service.GetAllBooksAsync();
                return Ok(books);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching all books.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookDto>> GetBookById(int id)
        {
            _logger.LogInformation("Fetching book with ID {Id}", id);
            try
            {
                var book = await _service.GetBookByIdAsync(id);
                if (book == null)
                {
                    return NotFound();
                }
                return Ok(book);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching book ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        // Requires 'Librarian' or 'Admin' role
        [Authorize(Policy = "LibrarianOrAdmin")]
        [HttpPost]
        public async Task<ActionResult<BookDto>> AddBook([FromBody] BookDto bookDto)
        {
            _logger.LogInformation("Adding new book: {Title}", bookDto.Title);
            try
            {
                var addedBook = await _service.AddBookAsync(bookDto);
                return CreatedAtAction(nameof(GetBookById), new { id = addedBook.Id }, addedBook);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid input when adding book.");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operation error when adding book.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception when adding new book.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        // Requires 'Admin' role
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] BookDto bookDto)
        {
            _logger.LogInformation("Updating book with ID {Id}", id);
            try
            {
                var success = await _service.UpdateBookAsync(id, bookDto);
                if (!success)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid input when updating book ID {Id}.", id);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                // Duplicate ISBN or other domain conflict
                _logger.LogWarning(ex, "Conflict error when updating book ID {Id}.", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception when updating book ID {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        // Requires 'Admin' role
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            _logger.LogInformation("Deleting book with ID {Id}", id);
            try
            {
                var success = await _service.DeleteBookAsync(id);
                if (!success)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception when deleting book ID {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }
    }
}
using LibraryManagementSystem.Api.Auth;
using LibraryManagementSystem.Application.Common;
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
        private readonly IAuthorizationService _authorizationService;

        public BookController(IBookService service, ILogger<BookController> logger, IAuthorizationService authorizationService)
        {
            _service = service;
            _logger = logger;
            _authorizationService = authorizationService;
        }

        [AllowAnonymous]
        [HttpGet]
        [EnableRateLimiting("GetBooksLimiter")]
        public async Task<ActionResult<ApiResponse<IEnumerable<BookDto>>>> GetBooks()
        {
            _logger.LogInformation("Fetching all books");
            try
            {
                var books = await _service.GetAllBooksAsync();
                return Ok(ApiResponse<IEnumerable<BookDto>>.SuccessResponse(books, "Books retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching all books.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<IEnumerable<BookDto>>.ErrorResponse("An unexpected error occurred."));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<BookDto>>> GetBookById(int id)
        {
            _logger.LogInformation("Fetching book with ID {Id}", id);
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, null, new RolesRequirement("Librarian", "Admin"));
            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning("Unauthorized access attempt by user {User}", User.Identity?.Name);
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<BookDto>.ErrorResponse("Forbidden"));
            }

            try
            {
                var book = await _service.GetBookByIdAsync(id);
                if (book == null)
                {
                    return NotFound(ApiResponse<BookDto>.ErrorResponse($"Book with ID {id} not found."));
                }
                return Ok(ApiResponse<BookDto>.SuccessResponse(book, "Book retrieved successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching book ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<BookDto>.ErrorResponse("An unexpected error occurred."));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<BookDto>>> AddBook([FromBody] BookDto bookDto)
        {
            _logger.LogInformation("Adding new book: {Title}", bookDto.Title);
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, null, new RolesRequirement("Librarian", "Admin"));
            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning("Unauthorized access attempt by user {User}", User.Identity?.Name);
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<BookDto>.ErrorResponse("Forbidden"));
            }

            try
            {
                var addedBook = await _service.AddBookAsync(bookDto);
                return CreatedAtAction(nameof(GetBookById), new { id = addedBook.Id },
                    ApiResponse<BookDto>.SuccessResponse(addedBook, "Book added successfully."));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid input when adding book.");
                return BadRequest(ApiResponse<BookDto>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Operation error when adding book.");
                return BadRequest(ApiResponse<BookDto>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception when adding new book.");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<BookDto>.ErrorResponse("An unexpected error occurred."));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateBook(int id, [FromBody] BookDto bookDto)
        {
            _logger.LogInformation("Updating book with ID {Id}", id);
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, null, new RolesRequirement("Admin"));
            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning("Unauthorized access attempt by user {User}", User.Identity?.Name);
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<BookDto>.ErrorResponse("Forbidden"));
            }

            try
            {
                var success = await _service.UpdateBookAsync(id, bookDto);
                if (!success)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse($"Book with ID {id} not found."));
                }
                return Ok(ApiResponse<object>.SuccessResponse(null, "Book updated successfully."));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid input when updating book ID {Id}.", id);
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Conflict error when updating book ID {Id}.", id);
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception when updating book ID {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.ErrorResponse("An unexpected error occurred."));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteBook(int id)
        {
            _logger.LogInformation("Deleting book with ID {Id}", id);
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, null, new RolesRequirement("Admin"));
            if (!authorizationResult.Succeeded)
            {
                _logger.LogWarning("Unauthorized access attempt by user {User}", User.Identity?.Name);
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<BookDto>.ErrorResponse("Forbidden"));
            }

            try
            {
                var success = await _service.DeleteBookAsync(id);
                if (!success)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse($"Book with ID {id} not found."));
                }
                return Ok(ApiResponse<object>.SuccessResponse(null, "Book deleted successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception when deleting book ID {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.ErrorResponse("An unexpected error occurred."));
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Application.Services;
using LibraryManagementSystem.Domain.Entities;

namespace LibraryManagementSystem.Api.Controllers
{
    [Route("api/books")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBookService _service;

        public BookController(IBookService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks() => Ok(await _service.GetAllBooksAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBookById(int id)
        {
            var book = await _service.GetBookByIdAsync(id);
            if (book == null) return NotFound();
            return Ok(book);
        }

        [HttpPost]
        public async Task<ActionResult<Book>> AddBook(Book book) => Ok(await _service.AddBookAsync(book));

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, Book book)
        {
            if (id != book.Id) return BadRequest("ID mismatch");

            var existingBook = await _service.GetBookByIdAsync(id);
            if (existingBook == null) return NotFound();

            await _service.UpdateBookAsync(book);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var existingBook = await _service.GetBookByIdAsync(id);
            if (existingBook == null) return NotFound();

            await _service.DeleteBookAsync(id);
            return NoContent();
        }
    }
}
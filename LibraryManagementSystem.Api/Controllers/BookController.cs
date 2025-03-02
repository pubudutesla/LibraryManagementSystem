using LibraryManagementSystem.Application.Services;
using LibraryManagementSystem.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        [HttpPost]
        public async Task<ActionResult<Book>> AddBook(Book book) => Ok(await _service.AddBookAsync(book));
    }
}
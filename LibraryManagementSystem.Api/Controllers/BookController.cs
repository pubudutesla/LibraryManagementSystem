﻿using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Application.Services;
using LibraryManagementSystem.Application.DTOs;

namespace LibraryManagementSystem.Api.Controllers
{
    [Route("api/books")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBookService _service;

        public BookController(IBookService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks()
        {
            var books = await _service.GetAllBooksAsync();
            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookDto>> GetBookById(int id)
        {
            var book = await _service.GetBookByIdAsync(id);
            if (book == null) return NotFound();
            return Ok(book);
        }

        [HttpPost]
        public async Task<ActionResult<BookDto>> AddBook(BookDto bookDto)
        {
            var addedBook = await _service.AddBookAsync(bookDto);
            return CreatedAtAction(nameof(GetBookById), new { id = addedBook.Id }, addedBook);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, BookDto bookDto)
        {
            var success = await _service.UpdateBookAsync(id, bookDto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var success = await _service.DeleteBookAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
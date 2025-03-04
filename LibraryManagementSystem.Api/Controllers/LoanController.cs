using LibraryManagementSystem.Application.DTOs;
using LibraryManagementSystem.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Api.Controllers
{
    [Route("api/loans")]
    [ApiController]
    public class LoanController : ControllerBase
    {
        private readonly ILoanService _service;

        public LoanController(ILoanService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<ActionResult<IEnumerable<LoanResponseDto>>> GetLoans()
        {
            var loans = await _service.GetAllLoansAsync();
            return Ok(loans);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<LoanResponseDto>> GetLoanById(int id)
        {
            var loan = await _service.GetLoanByIdAsync(id);
            if (loan == null) return NotFound();
            return Ok(loan);
        }

        [HttpGet("member/{memberId}")]
        [Authorize(Roles = "Member, Librarian, Admin")]
        public async Task<ActionResult<IEnumerable<LoanResponseDto>>> GetLoansByMemberId(int memberId)
        {
            var loans = await _service.GetLoansByMemberIdAsync(memberId);

            if (!loans.Any())
                return NotFound($"No loans found for member ID {memberId}.");

            return Ok(loans);
        }

        [HttpPost]
        [Authorize(Roles = "Member")]
        public async Task<ActionResult<LoanResponseDto>> CreateLoan(LoanRequestDto loanDto)
        {
            var loan = await _service.CreateLoanAsync(loanDto);
            return CreatedAtAction(nameof(GetLoanById), new { id = loan.Id }, loan);
        }

        [HttpPut("{id}/return")]
        public async Task<IActionResult> ReturnBook(int id)
        {
            var success = await _service.ReturnBookAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
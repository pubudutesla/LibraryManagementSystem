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
        private readonly ILogger<LoanController> _logger;

        public LoanController(ILoanService service, ILogger<LoanController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Librarian")]
        public async Task<ActionResult<IEnumerable<LoanResponseDto>>> GetLoans()
        {
            _logger.LogInformation("Fetching all loans");
            try
            {
                var loans = await _service.GetAllLoansAsync();
                return Ok(loans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all loans");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<LoanResponseDto>> GetLoanById(int id)
        {
            _logger.LogInformation("Fetching loan with ID {Id}", id);
            try
            {
                var loan = await _service.GetLoanByIdAsync(id);
                if (loan == null)
                {
                    return NotFound();
                }
                return Ok(loan);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching loan ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [HttpGet("member/{memberId}")]
        [Authorize(Roles = "Member,Librarian,Admin")]
        public async Task<ActionResult<IEnumerable<LoanResponseDto>>> GetLoansByMemberId(int memberId)
        {
            _logger.LogInformation("Fetching loans for member ID {MemberId}", memberId);
            try
            {
                var loans = await _service.GetLoansByMemberIdAsync(memberId);
                if (!loans.Any())
                    return NotFound($"No loans found for member ID {memberId}.");

                return Ok(loans);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching loans for member ID {MemberId}", memberId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Member")]
        public async Task<ActionResult<LoanResponseDto>> CreateLoan(LoanRequestDto loanDto)
        {
            _logger.LogInformation("Creating a new loan for BookId={BookId}, MemberId={MemberId}",
                loanDto.BookId, loanDto.MemberId);

            try
            {
                var loan = await _service.CreateLoanAsync(loanDto);
                return CreatedAtAction(nameof(GetLoanById), new { id = loan.Id }, loan);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Create loan failed due to invalid operation.");
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Create loan failed due to argument error.");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception when creating a loan.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        [HttpPut("{id}/return")]
        [Authorize]
        //[Authorize(Policy = "CanManageLoans")]
        public async Task<IActionResult> ReturnBook(int id)
        {
            _logger.LogInformation("Returning book for loan ID {Id}", id);
            try
            {
                var success = await _service.ReturnBookAsync(id);
                if (!success)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Return book failed due to invalid operation for loan ID {Id}", id);
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Return book failed due to argument error for loan ID {Id}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception when returning book for loan ID {Id}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }
    }
}
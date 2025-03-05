using LibraryManagementSystem.Application.DTOs;
using LibraryManagementSystem.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Api.Controllers
{
    [Route("api/members")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly IMemberService _service;
        private readonly ILogger<MemberController> _logger;

        public MemberController(IMemberService service, ILogger<MemberController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Get all members (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<MemberResponseDto>>> GetMembers()
        {
            _logger.LogInformation("Fetching all members");
            try
            {
                var members = await _service.GetAllMembersAsync();
                return Ok(members);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all members");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Get a specific member by ID (Admin only)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<MemberResponseDto>> GetMemberById(int id)
        {
            _logger.LogInformation("Fetching member with ID {Id}", id);
            try
            {
                var member = await _service.GetMemberByIdAsync(id);
                if (member == null)
                    return NotFound();
                return Ok(member);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching member ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Create a new member (Librarian or Admin)
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<MemberResponseDto>> AddMember(MemberRegistrationDto createMemberDto)
        {
            _logger.LogInformation("Creating new member: {Username}", createMemberDto.Username);
            try
            {
                var addedMember = await _service.AddMemberAsync(createMemberDto);
                return CreatedAtAction(nameof(GetMemberById), new { id = addedMember.Id }, addedMember);
            }
            catch (ArgumentException ex)
            {
                // e.g. invalid membership type
                _logger.LogWarning(ex, "Failed to create member due to argument error");
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                // e.g. username is already taken
                _logger.LogWarning(ex, "Failed to create member due to invalid operation");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception creating new member");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Update an existing member (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateMember(int id, MemberUpdateDto updateMemberDto)
        {
            _logger.LogInformation("Updating member with ID {Id}", id);
            try
            {
                var success = await _service.UpdateMemberAsync(id, updateMemberDto);
                if (!success)
                    return NotFound();
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Update member failed for ID {Id} due to argument error", id);
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Update member failed for ID {Id} due to invalid operation", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception updating member ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        /// <summary>
        /// Delete a member (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMember(int id)
        {
            _logger.LogInformation("Deleting member with ID {Id}", id);
            try
            {
                var success = await _service.DeleteMemberAsync(id);
                if (!success)
                    return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception deleting member ID {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }
    }
}
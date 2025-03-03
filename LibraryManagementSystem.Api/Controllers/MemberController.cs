using LibraryManagementSystem.Application.DTOs;
using LibraryManagementSystem.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Api.Controllers
{
    [Route("api/members")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly IMemberService _service;

        public MemberController(IMemberService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get all members (Admin only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetMembers()
        {
            var members = await _service.GetAllMembersAsync();
            return Ok(members);
        }

        /// <summary>
        /// Get a specific member by ID (Admin only)
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<MemberDto>> GetMemberById(int id)
        {
            var member = await _service.GetMemberByIdAsync(id);
            if (member == null) return NotFound();
            return Ok(member);
        }

        /// <summary>
        /// Create a new member (Admin only)
        /// </summary>
        ///
        [HttpPost]
        [Authorize(Policy = "LibrarianOrAdmin")]
        public async Task<ActionResult<MemberDto>> AddMember(MemberRegistrationDto createMemberDto)
        {
            try
            {
                var addedMember = await _service.AddMemberAsync(createMemberDto);
                return CreatedAtAction(nameof(GetMemberById), new { id = addedMember.Id }, addedMember);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing member (Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateMember(int id, MemberUpdateDto updateMemberDto)
        {
            var success = await _service.UpdateMemberAsync(id, updateMemberDto);
            if (!success) return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Delete a member (Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMember(int id)
        {
            var success = await _service.DeleteMemberAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
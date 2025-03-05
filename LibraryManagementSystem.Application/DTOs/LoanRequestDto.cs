using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Application.DTOs
{
    public class LoanRequestDto
    {
        [Required(ErrorMessage = "BookId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "BookId must be a positive number.")]
        public int BookId { get; set; }

        [Required(ErrorMessage = "MemberId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "MemberId must be a positive number.")]
        public int MemberId { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Application.DTOs
{
    public class BookDto
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Author is required.")]
        [StringLength(200, ErrorMessage = "Author cannot exceed 200 characters.")]
        public string Author { get; set; }

        [Required(ErrorMessage = "ISBN is required.")]
        [StringLength(50, ErrorMessage = "ISBN cannot exceed 50 characters.")]
        // Potentially use a [RegularExpression(...)] if you want to validate ISBN patterns
        public string ISBN { get; set; }

        [StringLength(100, ErrorMessage = "Genre cannot exceed 100 characters.")]
        public string Genre { get; set; }

        [Range(1, 9999, ErrorMessage = "Publication year must be between 1 and 9999.")]
        public int PublicationYear { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Available copies cannot be negative.")]
        public int AvailableCopies { get; set; }
    }
}
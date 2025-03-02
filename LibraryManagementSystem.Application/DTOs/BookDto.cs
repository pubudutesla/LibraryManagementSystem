using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Application.DTOs
{
    public class BookDto
    {
        public int? Id { get; set; }

        [Required] public string Title { get; set; }
        [Required] public string Author { get; set; }
        [Required] public string ISBN { get; set; }
        public string Genre { get; set; }
        public int PublicationYear { get; set; }
        public int AvailableCopies { get; set; }
    }
}
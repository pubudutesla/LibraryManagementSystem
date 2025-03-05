using Microsoft.EntityFrameworkCore;
using System;

namespace LibraryManagementSystem.Domain.Entities
{
    [Index(nameof(ISBN), IsUnique = true)]
    public class Book
    {
        public int Id { get; private set; }
        public string Title { get; private set; }
        public string Author { get; private set; }
        public string ISBN { get; private set; }
        public string Genre { get; private set; }
        public int PublicationYear { get; private set; }
        public int AvailableCopies { get; set; }

        // EF requires a parameterless constructor for migrations/proxy creation
        private Book() { }

        // Domain-level constructor: enforce your business invariants
        public Book(string title, string author, string isbn, string genre, int publicationYear, int availableCopies)
        {
            // Example invariants -- tune these rules to your needs:
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Book title is required.");

            if (string.IsNullOrWhiteSpace(author))
                throw new ArgumentException("Book author is required.");

            if (string.IsNullOrWhiteSpace(isbn))
                throw new ArgumentException("ISBN is required.");

            if (availableCopies < 0)
                throw new ArgumentException("Available copies cannot be negative.");

            Title = title.Trim();
            Author = author.Trim();
            ISBN = isbn.Trim();
            Genre = genre?.Trim() ?? string.Empty;  // Genre can be optional
            PublicationYear = publicationYear;
            AvailableCopies = availableCopies;
        }

        // Example domain method for updating a Book’s details, also validated
        public void UpdateDetails(string newTitle, string newAuthor, string newIsbn, string newGenre, int newPubYear)
        {
            if (string.IsNullOrWhiteSpace(newTitle))
                throw new ArgumentException("Title is required.");

            if (string.IsNullOrWhiteSpace(newAuthor))
                throw new ArgumentException("Author is required.");

            if (string.IsNullOrWhiteSpace(newIsbn))
                throw new ArgumentException("ISBN is required.");

            Title = newTitle.Trim();
            Author = newAuthor.Trim();
            ISBN = newIsbn.Trim();
            Genre = newGenre?.Trim() ?? string.Empty;
            PublicationYear = newPubYear;
        }

        // Additional domain logic (e.g., DecrementCopies, etc.)
        public void DecrementCopies()
        {
            if (AvailableCopies <= 0)
                throw new InvalidOperationException("No copies available to decrement.");

            AvailableCopies--;
        }
    }
}
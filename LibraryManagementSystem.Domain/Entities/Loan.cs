using Microsoft.EntityFrameworkCore;
using System;

namespace LibraryManagementSystem.Domain.Entities
{
    [Index(nameof(BookId), nameof(MemberId), IsUnique = false)] // Example only
    public class Loan
    {
        public int Id { get; private set; }
        public int BookId { get; private set; }
        public Book Book { get; private set; } = null!;
        public int MemberId { get; private set; }
        public Member Member { get; private set; } = null!;
        public DateTime LoanDate { get; private set; }
        public DateTime DueDate { get; private set; }
        public DateTime? ReturnDate { get; private set; }

        private Loan() { }

        public Loan(int bookId, int memberId, DateTime loanDate, DateTime dueDate)
        {
            if (bookId <= 0)
                throw new ArgumentException("Invalid book ID for loan.");

            if (memberId <= 0)
                throw new ArgumentException("Invalid member ID for loan.");

            if (dueDate < loanDate)
                throw new ArgumentException("DueDate cannot be earlier than LoanDate.");

            BookId = bookId;
            MemberId = memberId;
            LoanDate = loanDate;
            DueDate = dueDate;
            ReturnDate = null;
        }

        public void MarkAsReturned()
        {
            if (ReturnDate != null)
                throw new InvalidOperationException("This loan has already been returned.");

            ReturnDate = DateTime.UtcNow;
        }
    }
}
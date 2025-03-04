using AutoMapper;
using LibraryManagementSystem.Application.DTOs;
using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Infrastructure.Repositories;

namespace LibraryManagementSystem.Application.Services
{
    public class LoanService : ILoanService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LoanService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<LoanResponseDto>> GetAllLoansAsync()
        {
            var loans = await _unitOfWork.Loans.GetAllAsync();
            return _mapper.Map<IEnumerable<LoanResponseDto>>(loans);
        }

        public async Task<LoanResponseDto?> GetLoanByIdAsync(int id)
        {
            var loan = await _unitOfWork.Loans.GetByIdAsync(id);
            return loan == null ? null : _mapper.Map<LoanResponseDto>(loan);
        }

        public async Task<IEnumerable<LoanResponseDto>> GetLoansByMemberIdAsync(int memberId)
        {
            var loans = await _unitOfWork.Loans.GetLoansByMemberIdAsync(memberId);
            return _mapper.Map<IEnumerable<LoanResponseDto>>(loans);
        }

        public async Task<LoanResponseDto> CreateLoanAsync(LoanRequestDto loanDto)
        {
            // Check if the book exists
            var book = await _unitOfWork.Books.GetByIdAsync(loanDto.BookId);
            if (book == null)
                throw new ArgumentException("The requested book does not exist.");

            // Check if the book has available copies
            if (book.AvailableCopies <= 0)
                throw new InvalidOperationException("No available copies of the book.");

            // Check if the member exists
            var member = await _unitOfWork.Members.GetByIdAsync(loanDto.MemberId);
            if (member == null)
                throw new ArgumentException("Invalid member ID.");

            // Check if the member already has an active loan for this book
            var existingLoan = await _unitOfWork.Loans
                .GetLoansByMemberIdAsync(loanDto.MemberId);

            bool alreadyBorrowed = existingLoan.Any(l => l.BookId == loanDto.BookId && l.ReturnDate == null);
            if (alreadyBorrowed)
                throw new InvalidOperationException("This book is already borrowed by the member and has not been returned.");

            var loan = new Loan
            {
                BookId = loanDto.BookId,
                MemberId = loanDto.MemberId,
                LoanDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(14), // Default loan period
                ReturnDate = null
            };

            await _unitOfWork.Loans.AddAsync(loan);

            // Reduce available copies
            book.AvailableCopies--;

            await _unitOfWork.SaveChangesAsync();

            // Return loan details
            return _mapper.Map<LoanResponseDto>(loan);
        }

        public async Task<bool> ReturnBookAsync(int loanId)
        {
            // Check if the loan exists
            var loan = await _unitOfWork.Loans.GetByIdAsync(loanId);
            if (loan == null) return false;

            // Check if the book exists
            var book = await _unitOfWork.Books.GetByIdAsync(loan.BookId);
            if (book == null)
                throw new ArgumentException("The book associated with this loan does not exist.");

            // Check if the loan has already been returned
            if (loan.ReturnDate != null)
                throw new InvalidOperationException("This loan has already been returned.");

            // Update return date
            loan.ReturnDate = DateTime.UtcNow;

            // Increase available copies of the book
            book.AvailableCopies++;

            // Save changes to the database
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
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
            // Basic checks
            var book = await _unitOfWork.Books.GetByIdAsync(loanDto.BookId)
                ?? throw new KeyNotFoundException("The requested book does not exist.");

            if (book.AvailableCopies <= 0)
                throw new InvalidOperationException("No available copies of this book.");

            var member = await _unitOfWork.Members.GetByIdAsync(loanDto.MemberId)
                ?? throw new KeyNotFoundException("Invalid member ID.");

            // Check if the member already has an active loan for this book
            var existingLoans = await _unitOfWork.Loans.GetLoansByMemberIdAsync(member.Id);
            bool alreadyBorrowed = existingLoans.Any(l => l.BookId == book.Id && l.ReturnDate == null);
            if (alreadyBorrowed)
                throw new InvalidOperationException("This book is already borrowed by the member.");

            // Create new domain Loan object
            // Domain constructor ensures we can’t create an invalid loan
            var loan = new Loan(
                bookId: book.Id,
                memberId: member.Id,
                loanDate: DateTime.UtcNow,
                dueDate: DateTime.UtcNow.AddDays(14)
            );

            // Add to UoW
            await _unitOfWork.Loans.AddAsync(loan);

            // Decrement the Book’s available copies
            book.AvailableCopies--;
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<LoanResponseDto>(loan);
        }

        public async Task<bool> ReturnBookAsync(int loanId)
        {
            var loan = await _unitOfWork.Loans.GetByIdAsync(loanId);
            if (loan == null) return false; // Let controller return 404

            var book = await _unitOfWork.Books.GetByIdAsync(loan.BookId)
                ?? throw new KeyNotFoundException("The book associated with this loan does not exist.");

            // Use domain method to mark as returned
            loan.MarkAsReturned();

            // Increase book copies
            book.AvailableCopies++;

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
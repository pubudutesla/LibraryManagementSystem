using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Infrastructure.Repositories
{
    public class LoanRepository : ILoanRepository
    {
        private readonly LibraryDbContext _context;

        public LoanRepository(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Loan>> GetAllAsync()
        {
            return await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .ToListAsync();
        }

        public async Task<Loan?> GetByIdAsync(int id)
        {
            return await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<Loan>> GetLoansByMemberIdAsync(int memberId)
        {
            return await _context.Loans
                .Include(l => l.Book)
                .Include(l => l.Member)
                .Where(l => l.MemberId == memberId)
                .ToListAsync();
        }

        public async Task AddAsync(Loan loan)
        {
            await _context.Loans.AddAsync(loan);
        }

        // If your domain method modifies the Loan (like MarkAsReturned),
        // you might just call SaveChanges in the UnitOfWork rather than here:
        public async Task UpdateAsync(Loan loan)
        {
            _context.Loans.Update(loan);
            await _context.SaveChangesAsync();
        }
    }
}
using LibraryManagementSystem.Domain.Entities;

namespace LibraryManagementSystem.Infrastructure.Repositories
{
    public interface ILoanRepository
    {
        Task<IEnumerable<Loan>> GetAllAsync();
        Task<Loan?> GetByIdAsync(int id);
        Task<IEnumerable<Loan>> GetLoansByMemberIdAsync(int memberId);
        Task AddAsync(Loan loan);
        Task UpdateAsync(Loan loan);
    }
}
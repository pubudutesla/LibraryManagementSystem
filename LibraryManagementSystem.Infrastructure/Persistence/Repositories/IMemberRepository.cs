using LibraryManagementSystem.Domain.Entities;

namespace LibraryManagementSystem.Infrastructure.Repositories
{
    public interface IMemberRepository
    {
        Task<IEnumerable<Member>> GetAllAsync();
        Task<Member?> GetByIdAsync(int id);
        Task<Member?> GetByUsernameAsync(string username);
        Task AddAsync(Member member);
        Task UpdateAsync(Member member);
        Task DeleteAsync(Member member);
    }
}
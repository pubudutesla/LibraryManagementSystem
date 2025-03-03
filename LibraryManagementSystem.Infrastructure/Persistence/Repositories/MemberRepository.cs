using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Infrastructure.Repositories
{
    public class MemberRepository : IMemberRepository
    {
        private readonly LibraryDbContext _context;

        public MemberRepository(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Member>> GetAllAsync()
        {
            return await _context.Members.ToListAsync();
        }

        public async Task<Member?> GetByIdAsync(int id)
        {
            return await _context.Members.FindAsync(id);
        }

        public async Task<Member?> GetByUsernameAsync(string username)
        {
            return await _context.Members.FirstOrDefaultAsync(m => m.Username.ToLower() == username.ToLower());
        }

        public async Task AddAsync(Member member)
        {
            await _context.Members.AddAsync(member);
        }

        public async Task UpdateAsync(Member member)
        {
            _context.Members.Update(member);
        }

        public async Task DeleteAsync(Member member)
        {
            _context.Members.Remove(member);
        }
    }
}
using LibraryManagementSystem.Infrastructure.Persistence;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LibraryDbContext _context;
        public IBookRepository Books { get; }

        public UnitOfWork(LibraryDbContext context, IBookRepository bookRepository)
        {
            _context = context;
            Books = bookRepository;
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}
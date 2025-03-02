using LibraryManagementSystem.Infrastructure.Persistence;

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

        public async Task BeginTransactionAsync() => await _context.Database.BeginTransactionAsync();

        public async Task CommitTransactionAsync() => await _context.Database.CommitTransactionAsync();

        public async Task RollbackTransactionAsync() => await _context.Database.RollbackTransactionAsync();

        public void Dispose() => _context.Dispose();
    }
}
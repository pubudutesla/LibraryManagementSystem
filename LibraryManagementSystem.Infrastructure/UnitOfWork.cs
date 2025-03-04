using LibraryManagementSystem.Domain.Entities;
using LibraryManagementSystem.Infrastructure.Persistence;

namespace LibraryManagementSystem.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LibraryDbContext _context;
        public IBookRepository Books { get; }
        public IMemberRepository Members { get; }
        public ILoanRepository Loans { get; }

        public UnitOfWork(LibraryDbContext context, IBookRepository bookRepository, IMemberRepository memberRepository)
        {
            _context = context;
            Books = bookRepository;
            Members = memberRepository;
            Loans = new LoanRepository(context);
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task BeginTransactionAsync() => await _context.Database.BeginTransactionAsync();

        public async Task CommitTransactionAsync() => await _context.Database.CommitTransactionAsync();

        public async Task RollbackTransactionAsync() => await _context.Database.RollbackTransactionAsync();

        public void Dispose() => _context.Dispose();
    }
}
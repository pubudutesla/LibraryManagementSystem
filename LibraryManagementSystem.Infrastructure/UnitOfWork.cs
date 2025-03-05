using LibraryManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace LibraryManagementSystem.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LibraryDbContext _context;
        private IDbContextTransaction? _currentTransaction;

        public IBookRepository Books { get; }
        public IMemberRepository Members { get; }
        public ILoanRepository Loans { get; }

        public UnitOfWork(
            LibraryDbContext context,
            IBookRepository bookRepository,
            IMemberRepository memberRepository,
            ILoanRepository loanRepository)
        {
            _context = context;
            Books = bookRepository;
            Members = memberRepository;
            Loans = loanRepository;
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction != null)
                return; // or throw if transaction is already in progress

            _currentTransaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_currentTransaction == null) return;

            await _currentTransaction.CommitAsync();
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }

        public async Task RollbackTransactionAsync()
        {
            if (_currentTransaction == null) return;

            await _currentTransaction.RollbackAsync();
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
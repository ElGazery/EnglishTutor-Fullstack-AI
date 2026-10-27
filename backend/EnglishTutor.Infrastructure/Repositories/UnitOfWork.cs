using EnglishTutor.Application.Interfaces.Repositories;
using EnglishTutor.Infrastructure.Persistence;

namespace EnglishTutor.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TutorDbContext _context;

        public UnitOfWork(TutorDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<T> Repository<T>() where T : class
            => new GenericRepository<T>(_context);

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);
    }
}

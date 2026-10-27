using EnglishTutor.Application.Interfaces.Repositories;
using EnglishTutor.Application.Specifications;
using EnglishTutor.Infrastructure.Persistence;
using EnglishTutor.Infrastructure.SpecificationEvaluator;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly TutorDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(TutorDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id)
            => await _dbSet.FindAsync(id);

        public async Task<List<T>> GetAllAsync()
            => await _dbSet.ToListAsync();

        public async Task AddAsync(T entity)
            => await _dbSet.AddAsync(entity);

        public Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        public async Task<int> CountWithSpecAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
            => await SpecificationEvaluator.SpecificationEvaluator
                .GetCountQuery(_context.Set<T>(), spec)
                .CountAsync(cancellationToken);

        public async Task<List<T>> GetAllWithSpecAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
            => await SpecificationEvaluator.SpecificationEvaluator
                .GetQuery(_context.Set<T>(), spec)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        public async Task<T?> GetFirstWithSpecAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
            => await SpecificationEvaluator.SpecificationEvaluator
                .GetQuery(_context.Set<T>(), spec)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);
    }
}

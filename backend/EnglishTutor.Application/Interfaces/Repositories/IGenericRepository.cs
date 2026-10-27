using EnglishTutor.Application.Specifications;

namespace EnglishTutor.Application.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<List<T>> GetAllAsync();
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);

        Task<int> CountWithSpecAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
        Task<List<T>> GetAllWithSpecAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
        Task<T?> GetFirstWithSpecAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
    }
}

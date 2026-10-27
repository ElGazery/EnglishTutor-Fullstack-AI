using EnglishTutor.Application.Specifications;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Infrastructure.SpecificationEvaluator
{
    public static class SpecificationEvaluator
    {
        public static IQueryable<T> GetQuery<T>(IQueryable<T> inputQuery, ISpecification<T> spec)
            where T : class
        {
            var query = inputQuery;

            if (spec.Criteria is not null)
                query = query.Where(spec.Criteria);

            query = spec.Includes.Aggregate(query,
                (current, include) => current.Include(include));

            if (spec.OrderBy is not null)
                query = query.OrderBy(spec.OrderBy);
            else if (spec.OrderByDescending is not null)
                query = query.OrderByDescending(spec.OrderByDescending);

            if (spec.IsPagingEnabled)
                query = query.Skip(spec.Skip!.Value).Take(spec.Take!.Value);

            return query;
        }

        public static IQueryable<T> GetCountQuery<T>(IQueryable<T> inputQuery, ISpecification<T> spec)
            where T : class
        {
            var query = inputQuery;

            if (spec.Criteria is not null)
                query = query.Where(spec.Criteria);

            return query;
        }
    }
}

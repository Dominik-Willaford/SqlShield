using System.Linq.Expressions;

namespace SqlShield.Interface
{
    public interface IQueryBuilder<T> where T : new()
    {
        // Chaining methods
        IQueryBuilder<T> Where(Expression<Func<T, bool>> predicate);
        IQueryBuilder<T> OrderBy<TKey>(Expression<Func<T, TKey>> keySelector);

        // Execution methods
        Task<IEnumerable<T>> QueryAsync();
        Task<T> QuerySingleAsync();
    }
}

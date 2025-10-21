using System.Linq.Expressions;

namespace ECommerce.Repositroy
{
    public class CategoryRepositroy
    {
        ApplicationDBContext context = new();

        public async Task<IEnumerable<Categroy>> GetAsync(
            Expression<Func<Categroy, bool>>? expression = null,
            Expression<Func<Categroy, object>>[]? includes = null,
            bool tracked = true,
            CancellationToken cancellationToken = default
            )
        {
            var categories = context.Categroys.AsQueryable();
            if (expression is not null)
                categories = context.Categroys.Where(expression);
            if (!tracked)
                categories = categories.AsNoTracking();
            if (includes is not null)
                foreach (var include in includes)
                    categories = categories.Include(include);
            return await categories.ToListAsync(cancellationToken);  
        }
        public async Task<Categroy?> GetOneAsync(
            Expression<Func<Categroy, bool>>? expression = null,
            Expression<Func<Categroy, object>>[]? includes = null,
            bool tracked = true,
            CancellationToken cancellationToken = default
            )
        {
            return (await GetAsync(expression , includes , tracked, cancellationToken)).FirstOrDefault();
        }
        public async Task AddAsync(Categroy categroy, CancellationToken cancellationToken)
        {
            await context.AddAsync(categroy, cancellationToken);
        }
        public void Update(Categroy categroy)
        {
            context.Update(categroy);
        }
        public void Delete(Categroy categroy)
        {
            context.Remove(categroy);
        }
        public async Task CommitAsync(CancellationToken cancellation = default)
        {
            await context.SaveChangesAsync(cancellation);
        }
    }
}

using System.Linq.Expressions;
using CallRecordIntelligence.EF.Repository.Extentions;
using CallRecordIntelligence.EF.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;

namespace CallRecordIntelligence.EF.Repository;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;
    private readonly ILogger _logger;
    private const int DefaultBatchSize = 100;

    public GenericRepository(ApplicationDbContext context, ILogger<GenericRepository<T>> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Get

    public virtual async Task<Result<IEnumerable<TResult>>> GetListAsync<TResult>(
        Expression<Func<T, bool>> filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        List<Func<IQueryable<T>, IIncludableQueryable<T, object>>> includes = null,
        Expression<Func<T, TResult>> selector = null,
        int? skip = null,
        int? take = null,
        bool asNoTracking = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<T> query = ApplyConditions(_dbSet, filter, orderBy, includes, skip, take, asNoTracking);

            if (selector != null)
            {
                return await query.Select(selector).ToListAsync(cancellationToken);
            }

            return await query.Cast<TResult>().ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetListAsync");
                
            return Error.Unexpected("An error occurred while retrieving the data."); 
        }
    }

    public virtual async Task<Result<TResult>> GetSingleAsync<TResult>(
        Expression<Func<T, bool>> filter = null,
        List<Func<IQueryable<T>, IIncludableQueryable<T, object>>> includes = null,
        Expression<Func<T, TResult>> selector = null,
        bool asNoTracking = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<T> query = ApplyConditions(_dbSet, filter, null, includes, null, null, asNoTracking);

            if (selector != null)
            {
                return await query.Select(selector).FirstOrDefaultAsync(cancellationToken);
            }

            return await query.Cast<TResult>().FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetSingleAsync");
                
            return Error.Unexpected("An error occurred while retrieving the data.");
        }
    }

    #endregion

    #region Add

    public virtual async Task<Result<T>> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await _dbSet.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AddAsync");
                
            return Error.Unexpected("An error occurred while adding the data.");
        }
    }

    public virtual async Task<Result<bool>> AddRangeAsync(IEnumerable<T> entities,
        int batchSize = DefaultBatchSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (entities == null || !entities.Any())
            {
                throw new ArgumentNullException(nameof(entities));
            }

            foreach (var batch in entities.Batch(batchSize))
            {
                await _dbSet.AddRangeAsync(batch, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AddRangeAsync");
                
            return Error.Unexpected("An error occurred while adding the data.");
        }
    }

    #endregion

    #region Update
        
    public virtual async Task<Result<T>> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync(cancellationToken);

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateAsync");
                
            return Error.Unexpected("An error occurred while updating the data.");
        }
    }
     
    public virtual async Task<Result<int>> UpdateRangeAsync(IEnumerable<T> entities,
        int batchSize = DefaultBatchSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<T> query = _dbSet;

            int totalUpdated = 0;

            foreach (var batch in entities.Batch(batchSize))
            {
                _context.UpdateRange(batch);
                totalUpdated += await _context.SaveChangesAsync(cancellationToken);
            }

            return totalUpdated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateRangeAsync");
                
            return Error.Unexpected("An error occurred while updating the data.");
        }
    }
    
    public virtual async Task<Result<int>> UpdateRangeAsync(Expression<Func<T, bool>> filter,
        Action<T> updateAction,
        int batchSize = DefaultBatchSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var entities = await query.ToListAsync(cancellationToken);

            int totalUpdated = 0;

            foreach (var batch in entities.Batch(batchSize))
            {
                foreach (var entity in batch)
                {
                    updateAction(entity);
                }
                    
                _context.UpdateRange(batch);
                totalUpdated += await _context.SaveChangesAsync(cancellationToken);
            }

            return totalUpdated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateRangeAsync");
                
            return Error.Unexpected("An error occurred while updating the data.");
        }
    }
        
    public virtual async Task<Result<int>> UpdateRangeAsync(Expression<Func<T, bool>> filter,
        Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> updateExpression,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbSet.Where(filter);
            var updatedCount = await query.ExecuteUpdateAsync(updateExpression, cancellationToken);

            return updatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateRangeAsync");
            return Error.Unexpected("An error occurred while updating the data.");
        }
    }
        
    #endregion

    #region Remove

    public virtual async Task<Result<T>> RemoveAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RemoveAsync");
                
            return Error.Unexpected("An error occurred while removing the data.");
        }
    }
        
    public virtual async Task<Result<bool>> RemoveRangeAsync(IEnumerable<T> entities,
        int batchSize = DefaultBatchSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (entities == null || !entities.Any())
            {
                throw new ArgumentNullException(nameof(entities));
            }

            foreach (var batch in entities.Batch(batchSize))
            {
                _dbSet.RemoveRange(batch);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RemoveRangeAsync");
                
            return Error.Unexpected("An error occurred while removing the data.");
        }
    }
        
    public virtual async Task<Result<int>> RemoveRangeAsync(Expression<Func<T, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbSet.Where(filter);
            var deletedCount = await query.ExecuteDeleteAsync(cancellationToken);

            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RemoveRangeAsync");
            return Error.Unexpected("An error occurred while removing the data.");
        }
    }

    #endregion

    #region Aggregations

    public virtual async Task<Result<bool>> ExistsAsync(Expression<Func<T, bool>> filter,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            return await _dbSet.AnyAsync(filter, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ExistsAsync");
                
            return Error.Unexpected("An error occurred while checking the existence of the data.");
        }
    }
        
    public virtual async Task<Result<int>> CountAsync(Expression<Func<T, bool>> filter = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.CountAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CountAsync");
                
            return Error.Unexpected("An error occurred while counting the data.");
        }
    }

    public virtual async Task<Result<decimal>> SumAsync(Expression<Func<T, bool>> filter,
        Expression<Func<T, decimal>> selector, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var sum = await query.SumAsync(selector, cancellationToken);

            return sum;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SumAsync");
                
            return Error.Unexpected("An error occurred while calculating the sum.");
        }
    }

    public virtual async Task<Result<decimal>> AverageAsync(Expression<Func<T, bool>> filter,
        Expression<Func<T, decimal>> selector, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var average = await query.AverageAsync(selector, cancellationToken);

            return average;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AverageAsync");
                
            return Error.Unexpected("An error occurred while calculating the average.");
        }
    }

    public virtual async Task<Result<decimal>> MaxAsync(Expression<Func<T, bool>> filter,
        Expression<Func<T, decimal>> selector, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var max = await query.MaxAsync(selector, cancellationToken);

            return max;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in MaxAsync");
                
            return Error.Unexpected("An error occurred while calculating the maximum.");
        }
    }

    public virtual async Task<Result<decimal>> MinAsync(Expression<Func<T, bool>> filter,
        Expression<Func<T, decimal>> selector, CancellationToken cancellationToken = default)
    {
        try
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var min = await query.MinAsync(selector, cancellationToken);

            return min;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in MinAsync");
                
            return Error.Unexpected("An error occurred while calculating the minimum.");
        }
    }

    #endregion

    #region Transactions

    public virtual async Task<Result<int>> ExecuteTransactionAsync(Func<Task> operation,
        CancellationToken cancellationToken = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await operation();
            await transaction.CommitAsync(cancellationToken);

            return 1;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error in ExecuteTransactionAsync");
                
            return Error.Unexpected("An error occurred while executing the transaction.");
        }
    }

    #endregion

    #region Helpers

    private IQueryable<T> ApplyConditions(IQueryable<T> query, Expression<Func<T, bool>> filter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
        List<Func<IQueryable<T>, IIncludableQueryable<T, object>>> includes = null, int? skip = null, int? take = null,
        bool asNoTracking = false)
    {
        if (filter != null)
        {
            query = query.Where(filter);
        }

        if (includes != null)
        {
            foreach (var include in includes)
            {
                query = include(query);
            }
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        if (skip.HasValue)
        {
            query = query.Skip(skip.Value);
        }

        if (take.HasValue)
        {
            query = query.Take(take.Value);
        }

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return query;
    }

    #endregion
}
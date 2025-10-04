using Microsoft.EntityFrameworkCore;
using Peel.Data.Context;
using Peel.Data.Entities;

namespace Peel.Data.Repositories;

public class SearchHistoryRepository(AppDbContext context)
    : GenericRepository<SearchHistory>(context)
{
    public async Task<IEnumerable<SearchHistory>> FindByFilterAsync(SearchHistory filter)
    {
        IQueryable<SearchHistory> query = _dbSet;

        query = query.Where(x =>
            (filter.OfferType == null && x.OfferType == null) || // force null
            (filter.OfferType != null && x.OfferType == filter.OfferType));

        query = query.Where(x =>
            (filter.AmountCurrency == null && x.AmountCurrency == null) || // force null
            (filter.AmountCurrency != null && x.AmountCurrency == filter.AmountCurrency));

        if (filter.AmountRange != null)
            query = query.Where(x => x.AmountRange != null && x.AmountRange.Value.Overlaps(filter.AmountRange.Value));
        else
            query = query.Where(x => x.AmountRange == null);

        if (filter.MaxSpread != null)
            query = query.Where(x => x.MaxSpread <= filter.MaxSpread);
        else
            query = query.Where(x => x.MaxSpread == null);

        if (filter.MinReputation != null)
            query = query.Where(x => x.MinReputation >= filter.MinReputation);
        else
            query = query.Where(x => x.MinReputation == null);

        return await query.ToListAsync();
    }

}

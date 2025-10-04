using Microsoft.EntityFrameworkCore;
using Peel.Models;

namespace Peel.Data.Context;

partial class AppDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum<CurrencyType>(schema: "public", name: "currency_type")
            .HasPostgresEnum<OfferTypeFilter>(schema: "public", name: "offer_type_filter");
    }
}

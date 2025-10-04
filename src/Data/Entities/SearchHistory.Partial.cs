using System;
using System.Collections.Generic;
using NpgsqlTypes;
using Peel.Models;

namespace Peel.Data.Entities;

partial class SearchHistory
{
    public OfferTypeFilter? OfferType { get; set; }
    
    public CurrencyType? AmountCurrency { get; set; }
}

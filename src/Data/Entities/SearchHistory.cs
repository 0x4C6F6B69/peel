using System;
using System.Collections.Generic;
using NpgsqlTypes;

namespace Peel.Data.Entities;

public partial class SearchHistory
{
    public int SearchId { get; set; }

    public NpgsqlRange<int>? AmountRange { get; set; }

    public float? MaxSpread { get; set; }

    public double? MinReputation { get; set; }

    public DateTime OccurredAt { get; set; }
}

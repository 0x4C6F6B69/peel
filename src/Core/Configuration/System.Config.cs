using System.ComponentModel.DataAnnotations;

namespace Peel.Configuration;

public sealed class SystemConfig
{
    public const string SectionName = "system";

    [Required(ErrorMessage = $"{nameof(DefaultFiat)} setting is required")]
    public required string DefaultFiat { get; init; }
}

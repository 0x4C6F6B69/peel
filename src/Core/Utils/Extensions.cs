using SharpX;

namespace Peel;

internal static class MaybeExtensions
{
    public static IEnumerable<T> Unwrap<T>(this Maybe<IEnumerable<T>> maybe) => maybe switch
    {
        { Tag: MaybeType.Just } => maybe.FromJust()!,
        _                       => []
    };
}

internal static class EnumExtensions
{
    public static string ToLower(this Enum value) => value.ToString().ToLowerInvariant();  
}

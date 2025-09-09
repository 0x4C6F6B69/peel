namespace Peel.Infrastructure;

public sealed class Result<T>
{
    public T? Value { get; }
    public List<string> Errors { get; }

    public bool IsSuccess => Value != null && Errors.Count == 0;
    public bool IsFailure => Value == null && Errors.Count > 0;
    public bool IsPartial => Value != null && Errors.Count > 0;

    public Result(T? value, List<string> errors)
    {
        Value = value;
        Errors = errors ?? new List<string>();
    }
}

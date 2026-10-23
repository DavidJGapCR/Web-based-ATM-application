namespace Atm.Domain.Common;

/// <summary>
/// Outcome of an operation that can fail for an expected business reason.
/// Expected failures travel as values; exceptions are reserved for the unexpected.
/// </summary>
public class Result
{
    private readonly Error? _error;

    protected Result(Error? error) => _error = error;

    public bool IsSuccess => _error is null;

    public bool IsFailure => !IsSuccess;

    public Error Error => _error ?? throw new InvalidOperationException("A successful result has no error.");

    public static Result Success() => new(null);

    public static Result Failure(Error error) => new(error);

    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>
/// Outcome of an operation that produces a value on success.
/// </summary>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value) : base(null) => _value = value;

    private Result(Error error) : base(error) => _value = default;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"A failed result has no value. Error: {Error.Code}");

    public static Result<T> Success(T value) => new(value);

    public new static Result<T> Failure(Error error) => new(error);

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(Error error) => Failure(error);
}

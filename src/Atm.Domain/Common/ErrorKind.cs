namespace Atm.Domain.Common;

/// <summary>
/// Category of an expected business failure. The API layer maps each kind to an HTTP status.
/// </summary>
public enum ErrorKind
{
    Validation,
    NotFound,
    InsufficientFunds,
    Conflict
}

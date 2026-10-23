namespace Atm.Domain.Common;

/// <summary>
/// An expected business failure: a stable machine-readable code, a human-readable message and its kind.
/// </summary>
public sealed record Error(string Code, string Message, ErrorKind Kind);

using Atm.Domain.Accounts;

namespace Atm.Domain.Transfers;

/// <summary>
/// The two history entries a transfer produces, linked by <see cref="TransferId"/>.
/// </summary>
public sealed record TransferLegs(Guid TransferId, AccountTransaction Debit, AccountTransaction Credit);

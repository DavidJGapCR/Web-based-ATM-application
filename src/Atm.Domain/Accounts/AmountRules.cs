using Atm.Domain.Common;

namespace Atm.Domain.Accounts;

/// <summary>
/// Rules every monetary amount must satisfy before it can move money.
/// </summary>
public static class AmountRules
{
    public const int MaxDecimalPlaces = 2;

    public static Result Validate(decimal amount)
    {
        if (amount <= 0)
        {
            return AccountErrors.InvalidAmount;
        }

        // Compare by value, not by scale: 10.100m is a valid 2-decimal amount.
        if (decimal.Round(amount, MaxDecimalPlaces) != amount)
        {
            return AccountErrors.TooManyDecimals;
        }

        return Result.Success();
    }
}

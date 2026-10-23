using Atm.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Atm.Infrastructure.Persistence.Configurations;

internal sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();
        builder.Property(a => a.Name).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Balance).IsRequired();

        // SQLite has no rowversion, so the domain increments Version on every change
        // and EF adds "WHERE Version = @original" to each UPDATE.
        builder.Property(a => a.Version).IsConcurrencyToken();

        builder.HasData(
            new { Id = 1, Name = "Checking", Balance = 0m, Version = 0 },
            new { Id = 2, Name = "Savings", Balance = 0m, Version = 0 });
    }
}

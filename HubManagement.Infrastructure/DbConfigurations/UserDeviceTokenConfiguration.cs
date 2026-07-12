using HubManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HubManagement.Infrastructure.DbConfigurations;

public class UserDeviceTokenConfiguration : IEntityTypeConfiguration<UserDeviceToken>
{
    public void Configure(EntityTypeBuilder<UserDeviceToken> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("UserDeviceTokens")
            .HasKey(x => x.Id);
    }
}
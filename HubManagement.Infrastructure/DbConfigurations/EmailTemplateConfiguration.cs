using HubManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HubManagement.Infrastructure.DbConfigurations;

public class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
{
    public void Configure(EntityTypeBuilder<EmailTemplate> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("EmailTemplates")
            .HasKey(x => x.Id);
        
        builder.Property(x => x.Title).HasMaxLength(250);
        
        builder.Property(x => x.Subject).HasMaxLength(500);

        builder.Property(x => x.Type)
            .HasConversion<string>();
        
        builder.Property(x => x.Body);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Outbox;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Id)
            .HasColumnType("uuid");

        builder.Property(message => message.EventId)
            .HasColumnType("uuid")
            .IsRequired();

        builder.HasIndex(message => message.EventId)
            .IsUnique();

        builder.Property(message => message.OccurredAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(message => message.Type)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(message => message.Payload)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(message => message.ProcessedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(message => message.NextAttemptAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(message => message.Error)
            .HasMaxLength(2000);
    }
}

using MercerAssistant.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MercerAssistant.Infrastructure.Data.Configurations;

public class AvailabilityWindowConfiguration : IEntityTypeConfiguration<AvailabilityWindow>
{
    public void Configure(EntityTypeBuilder<AvailabilityWindow> builder)
    {
        builder.HasKey(a => a.Id);

        builder.HasIndex(a => new { a.UserId, a.DayOfWeek });

        builder.HasOne(a => a.User)
            .WithMany(u => u.AvailabilityWindows)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

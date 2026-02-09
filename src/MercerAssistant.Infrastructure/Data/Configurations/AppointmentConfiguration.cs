using MercerAssistant.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MercerAssistant.Infrastructure.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.HasIndex(a => new { a.ProviderId, a.StartTimeUtc });
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.GoogleEventId);

        builder.HasOne(a => a.Provider)
            .WithMany(u => u.AppointmentsAsProvider)
            .HasForeignKey(a => a.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.BookingPage)
            .WithMany(bp => bp.Appointments)
            .HasForeignKey(a => a.BookingPageId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(20);
    }
}

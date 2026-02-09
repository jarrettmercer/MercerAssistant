using MercerAssistant.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MercerAssistant.Infrastructure.Data.Configurations;

public class BookingPageConfiguration : IEntityTypeConfiguration<BookingPage>
{
    public void Configure(EntityTypeBuilder<BookingPage> builder)
    {
        builder.HasKey(bp => bp.Id);
        builder.HasIndex(bp => bp.Slug).IsUnique();

        builder.HasOne(bp => bp.Owner)
            .WithMany(u => u.BookingPages)
            .HasForeignKey(bp => bp.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

using System.ComponentModel.DataAnnotations;

namespace MercerAssistant.Core.Entities;

public class BookingPage
{
    public Guid Id { get; set; }

    public string OwnerId { get; set; } = null!;
    public ApplicationUser Owner { get; set; } = null!;

    [Required, StringLength(100)]
    public string Title { get; set; } = "Book a Meeting";

    [StringLength(500)]
    public string? Description { get; set; }

    [Required, StringLength(50)]
    public string Slug { get; set; } = "";

    public int DefaultDurationMinutes { get; set; } = 30;
    public int MaxAdvanceDays { get; set; } = 60;
    public int MinNoticeHours { get; set; } = 2;
    public int BufferMinutes { get; set; } = 15;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Appointment> Appointments { get; set; } = [];
}

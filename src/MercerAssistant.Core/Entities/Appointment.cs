using System.ComponentModel.DataAnnotations;
using MercerAssistant.Core.Enums;

namespace MercerAssistant.Core.Entities;

public class Appointment
{
    public Guid Id { get; set; }

    public string ProviderId { get; set; } = null!;
    public ApplicationUser Provider { get; set; } = null!;

    [Required, StringLength(100)]
    public string ClientName { get; set; } = "";

    [Required, EmailAddress]
    public string ClientEmail { get; set; } = "";

    [Phone]
    public string? ClientPhone { get; set; }

    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }

    public int DurationMinutes { get; set; } = 30;

    [StringLength(500)]
    public string? Title { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

    public string? GoogleEventId { get; set; }

    public Guid? BookingPageId { get; set; }
    public BookingPage? BookingPage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

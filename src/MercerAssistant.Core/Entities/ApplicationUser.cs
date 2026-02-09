using System.ComponentModel.DataAnnotations;
using MercerAssistant.Core.Enums;
using Microsoft.AspNetCore.Identity;

namespace MercerAssistant.Core.Entities;

public class ApplicationUser : IdentityUser
{
    [StringLength(100)]
    public string DisplayName { get; set; } = "";

    public UserRole Role { get; set; } = UserRole.Client;

    [StringLength(50)]
    public string TimeZone { get; set; } = "America/New_York";

    public string? GoogleRefreshToken { get; set; }

    [StringLength(200)]
    public string? GoogleCalendarId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<AvailabilityWindow> AvailabilityWindows { get; set; } = [];
    public ICollection<Appointment> AppointmentsAsProvider { get; set; } = [];
    public ICollection<BookingPage> BookingPages { get; set; } = [];
    public ICollection<ChatConversation> ChatConversations { get; set; } = [];
}

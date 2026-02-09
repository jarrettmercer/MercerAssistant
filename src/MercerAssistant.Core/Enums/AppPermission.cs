namespace MercerAssistant.Core.Enums;

/// <summary>
/// Feature permissions that can be toggled per user.
/// Stored as Identity claims with claim type "Permission".
/// Admins always have full access regardless of these settings.
/// </summary>
public static class AppPermission
{
    public const string ClaimType = "Permission";

    public const string ViewDashboard = "ViewDashboard";
    public const string ViewChat = "ViewChat";
    public const string ViewAvailability = "ViewAvailability";
    public const string ViewAppointments = "ViewAppointments";
    public const string ViewSettings = "ViewSettings";

    /// <summary>
    /// Default permissions granted to new users.
    /// </summary>
    public static readonly string[] Defaults =
    [
        ViewDashboard,
        ViewChat,
        ViewAppointments
    ];

    /// <summary>
    /// All available permissions with display labels.
    /// </summary>
    public static readonly (string Value, string Label, string Description)[] All =
    [
        (ViewDashboard, "Dashboard", "View the dashboard with stats and upcoming appointments"),
        (ViewChat, "AI Chat", "Use the AI scheduling assistant chat"),
        (ViewAvailability, "Availability", "View and edit availability schedules"),
        (ViewAppointments, "Appointments", "View and manage appointments"),
        (ViewSettings, "Settings", "Access profile and integration settings"),
    ];
}

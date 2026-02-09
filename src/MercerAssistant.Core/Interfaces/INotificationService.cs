using MercerAssistant.Core.Entities;

namespace MercerAssistant.Core.Interfaces;

public interface INotificationService
{
    Task SendBookingConfirmationAsync(Appointment appointment);
    Task SendBookingCancellationAsync(Appointment appointment);
    Task SendBookingReminderAsync(Appointment appointment);
}

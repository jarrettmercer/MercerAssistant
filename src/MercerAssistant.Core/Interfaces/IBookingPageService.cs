using MercerAssistant.Core.Entities;

namespace MercerAssistant.Core.Interfaces;

public interface IBookingPageService
{
    Task<BookingPage?> GetBySlugAsync(string slug);
    Task<BookingPage> CreateAsync(BookingPage page);
    Task<BookingPage> UpdateAsync(BookingPage page);
    Task<List<BookingPage>> GetByOwnerAsync(string ownerId);
}

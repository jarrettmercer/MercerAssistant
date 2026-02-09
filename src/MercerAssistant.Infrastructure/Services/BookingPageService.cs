using MercerAssistant.Core.Entities;
using MercerAssistant.Core.Interfaces;
using MercerAssistant.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MercerAssistant.Infrastructure.Services;

public class BookingPageService : IBookingPageService
{
    private readonly AppDbContext _db;

    public BookingPageService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<BookingPage?> GetBySlugAsync(string slug)
    {
        return await _db.BookingPages
            .Include(bp => bp.Owner)
            .FirstOrDefaultAsync(bp => bp.Slug == slug && bp.IsActive);
    }

    public async Task<BookingPage> CreateAsync(BookingPage page)
    {
        _db.BookingPages.Add(page);
        await _db.SaveChangesAsync();
        return page;
    }

    public async Task<BookingPage> UpdateAsync(BookingPage page)
    {
        _db.BookingPages.Update(page);
        await _db.SaveChangesAsync();
        return page;
    }

    public async Task<List<BookingPage>> GetByOwnerAsync(string ownerId)
    {
        return await _db.BookingPages
            .Where(bp => bp.OwnerId == ownerId)
            .OrderByDescending(bp => bp.CreatedAt)
            .ToListAsync();
    }
}

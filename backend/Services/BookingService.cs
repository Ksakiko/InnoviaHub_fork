using backend.Models;
using backend.Models.Interfaces;
using static backend.Controllers.ChatController;

namespace backend.Services;

public class BookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IResourceRepository _resourceRepository;

    public BookingService(
        IBookingRepository bookingRepository,
        IResourceRepository resourceRepository
        )
    {
        _bookingRepository = bookingRepository;
        _resourceRepository = resourceRepository;
    }

    public async Task<string> HandleBooking(BookingInfoObj request)
    {
        var resource = await _resourceRepository.GetResourceByNameAsync(request.Resource);

        if (resource == null) return $"Resursen {request.Resource} hittades inde.";

        var overlap = await _bookingRepository.HasOverlap(resource.Id, request.StartTime, request.EndTime);

        if (overlap) return $"Tyvärr är resursen {request.Resource} nu upptagen från {request.StartTime} till {request.EndTime}";

        var booking = new Booking
        {
            UserId = request.UserId,
            UserName = request.UserName,
            ResourceId = resource.Id,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Status = request.Status
        };

        var confirmedBooking = await _bookingRepository.Add(booking);

        if (confirmedBooking != null) return "";

        return "Något gick fel. Vänligen försök igen.";
    }
    
}

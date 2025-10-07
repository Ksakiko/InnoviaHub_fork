using System;
using System.Security.Cryptography.X509Certificates;
using backend.Models.Entities;
using backend.Models.Interfaces;
using static backend.Services.ChatGptService;

namespace backend.Services;

public class BookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IResourceRepository _resourceRepository;
    private readonly IResourceTypeRepository _resourceTypeRepository;

    public BookingService(
        IBookingRepository bookingRepository,
        IResourceRepository resourceRepository,
        IResourceTypeRepository resourceTypeRepository)
    {
        _bookingRepository = bookingRepository;
        _resourceRepository = resourceRepository;
        _resourceTypeRepository = resourceTypeRepository;
    }

    public async Task<List<Resource>> GetAvailableResources(ExtractedBookingRequest bookingRequest)
    {
        // Check if the requested resource matches any resource name in Resources (not resourceType)
        var resources = await _resourceRepository.GetAll();
        var specificResource = resources.FirstOrDefault(x => x.Name == bookingRequest.Resource);

        // If user input matches one of the registered resource name, check availability
        if (specificResource != null)
        {
            var overlap = await _bookingRepository.HasOverlap(specificResource.Id, bookingRequest.StartTime, bookingRequest.EndTime);

            if (overlap) return [];
            return [specificResource];
        }

        // If user input did not specify any registered resource name,
        // use the input as resource type and get all available resources
        var resourceTypes = await _resourceTypeRepository.GetAllAsync();

        var requestedResourceType = resourceTypes.FirstOrDefault(x => x.Name == bookingRequest.Resource);

        if (requestedResourceType == null) return [];

        var allResources = await _resourceRepository.GetAllByResourceIdAsync(requestedResourceType.Id);

        var availableResources = new List<Resource>();

        foreach (var r in allResources)
        {
            var overlap = await _bookingRepository.HasOverlap(r.Id, bookingRequest.StartTime, bookingRequest.EndTime);
            if (!overlap) availableResources.Add(r);
        }
    
        return availableResources;
    }
}

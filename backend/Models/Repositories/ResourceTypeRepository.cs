using System;
using System.Security.AccessControl;
using backend.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace backend.Models.Repositories;

public class ResourceTypeRepository : IResourceTypeRepository
{
    private readonly AppDbContext _context;

    public ResourceTypeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Entities.ResourceType>> GetAllAsync()
    {
        return await _context.ResourceTypes.ToListAsync();
    }
}

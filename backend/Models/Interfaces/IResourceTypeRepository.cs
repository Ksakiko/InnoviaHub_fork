using System;
using System.Security.AccessControl;

namespace backend.Models.Interfaces;

public interface IResourceTypeRepository
{
    Task<IEnumerable<Entities.ResourceType>> GetAllAsync();
}

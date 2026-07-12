using HubManagement.BuildingBlock.Core.Abstractions;

namespace HubManagement.Application.Services;

public interface ICurrentUserService : ICurrentUser,ICurrentUserInitializer
{
    
}
using HubManagement.Application.Contracts;
using HubManagement.BuildingBlock.Infrastructure.FileStorage;
using Mediator;

namespace HubManagement.Application.Features.Files.UploadFiles;

public class UploadFilesHandler(IApplicationDbContext db, IStorageService storageService)
    : ICommandHandler<UploadFilesCommand>
{
    private readonly IApplicationDbContext _db = db;
    private readonly IStorageService _storageService = storageService;

    public ValueTask<Unit> Handle(UploadFilesCommand command, CancellationToken cancellationToken)
    {
        return default;
    }
}
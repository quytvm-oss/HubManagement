using HubManagement.BuildingBlock.Core.Domain;
using HubManagement.Domain.Enums;

namespace HubManagement.Domain.Entities;

public class EmailTemplate : AggregateRoot<Guid>, IAuditableEntity, ISoftDeletable
{
    public string? Title { get; set; }
    
    public EmailTemplateType Type { get; set; }

    public string? Subject { get; set; }

    public string? Body { get; set; }

    public DateTimeOffset CreatedOnUtc { get; private set; }
    
    public string? CreatedBy { get; private set; }
    
    public DateTimeOffset? LastModifiedOnUtc { get; private set; }
    
    public string? LastModifiedBy { get;  private set; }
    
    public bool IsDeleted { get; private set; }
    
    public DateTimeOffset? DeletedOnUtc { get;  private set; }
    
    public string? DeletedBy { get; private set; }
}
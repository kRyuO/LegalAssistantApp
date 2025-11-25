using LegalAssistantApp.Data;

namespace LegalAssistantApp.Services;

public class PermissionService
{
    private readonly AppDbContext _context;

    public PermissionService(AppDbContext context)
    {
        _context = context;
    }

    public bool CanViewDocuments(int userId)
    {
        var user = _context.Users.Find(userId);
        return user?.IsActive == true;
    }

    public bool CanEditDocument(int userId, int documentId)
    {
        var user = _context.Users.Find(userId);
        var document = _context.Documents.Find(documentId);

        return user?.IsActive == true &&
               document?.CreatedByUserId == userId;
    }

    public bool CanDeleteCounterparty(int userId)
    {
        var user = _context.Users.Find(userId);
        return user?.IsActive == true;
    }
}
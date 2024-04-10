using Microsoft.AspNetCore.Identity;

namespace ContactsManager.Core.Domain.IndentityEntites
{
    public class ApplicationRole : IdentityRole<Guid>
    {
        public string? PersonName { get; set; }
    }
}

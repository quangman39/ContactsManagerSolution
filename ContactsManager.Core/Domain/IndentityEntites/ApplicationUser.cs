using Microsoft.AspNetCore.Identity;

namespace ContactsManager.Core.Domain.IndentityEntites
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? PersonName { get; set; }
    }
}

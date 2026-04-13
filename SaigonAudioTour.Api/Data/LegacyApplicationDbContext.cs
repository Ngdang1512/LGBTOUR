using Microsoft.EntityFrameworkCore;

namespace LGBTOUR.Api.Data
{
    // Compatibility shim for existing EF Core migration metadata that still references LGBTOUR.Api.Data.ApplicationDbContext.
    public class ApplicationDbContext : SaigonAudioTour.Api.Data.ApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<SaigonAudioTour.Api.Data.ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}

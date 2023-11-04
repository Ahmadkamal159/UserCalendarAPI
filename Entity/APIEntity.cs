using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserCalendarAPI.Models;

namespace UserCalendarAPI.Entity
{
    public class APIEntity:IdentityDbContext<ApplicationUser>
    {
        public APIEntity()
        {
            
        }

        public APIEntity(DbContextOptions<APIEntity> options):base(options)
        {
            
        }


    }
}

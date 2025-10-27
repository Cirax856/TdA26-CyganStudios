using Microsoft.EntityFrameworkCore;
using System.Security.Principal;

namespace TdA26_CyganStudios;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }


}

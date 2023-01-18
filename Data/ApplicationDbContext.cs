using Microsoft.EntityFrameworkCore;
using Hangman.Models;

namespace Hangman.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {

        }
        public DbSet<User> Users { get; set; }
        public DbSet<Session> Sessions { get; set; }

        
    }
}

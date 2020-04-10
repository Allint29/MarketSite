using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using MyWebSite.Models;

namespace MyWebSite.Services
{
    public class ApplicationContext: DbContext
    {
        public DbSet<ImpersonalTrade> ImpersonalTrades { get; set; }

        public ApplicationContext()
        {
            //Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=helloappdb;Trusted_Connection=True;");
        }
    }
}

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Automacao.Core.Data
{
    internal class SincronizacaoDbContext : AutomacaoDbContext
    {
        public SincronizacaoDbContext(DbContextOptions<SincronizacaoDbContext> options)
        : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SincronizacaoDbContext).Assembly);
        }
    }
}

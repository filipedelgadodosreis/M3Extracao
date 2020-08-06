using EFCore.BulkExtensions;
using M3.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;

namespace M3Inventario.BackgroundTasks.Data
{
    public class M3InventarioContext : DbContext
    {
        public M3InventarioContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Inventario>(entity =>
            {
                entity.ToTable("CargaDadosAppInventario", "m3");
                entity.HasNoKey();
            });

            modelBuilder.Entity<Inventario>().Property(x => x.DATA).HasColumnName("DATA");
            modelBuilder.Entity<Inventario>().Property(x => x.DEVICE_NAME).HasColumnName("DEVICE_NAME");
            modelBuilder.Entity<Inventario>().Property(x => x.APP_NAME).HasColumnName("APP_NAME");
            modelBuilder.Entity<Inventario>().Property(x => x.VERSAO).HasColumnName("VERSAO");
            modelBuilder.Entity<Inventario>().Property(x => x.IND_SISTEMA).HasColumnName("IND_SISTEMA");
        }
    }

    public static class ModelBuilderExtensions
    {
        public static void RemovePluralizingTableNameConvention(this ModelBuilder modelBuilder)
        {
            foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
            {
                if (!entity.IsOwned() && entity.BaseType == null) // without this exclusion OwnedType would not be by default in Owner Table
                {
                    entity.SetTableName(entity.ClrType.Name);
                }
            }
        }
    }

    public static class ContextUtil
    {
        public static DbServer DbServer { get; set; }

        public static DbContextOptions GetOptions()
        {
            var optionsBuilder = new DbContextOptionsBuilder<M3InventarioContext>();

            if (DbServer == DbServer.SqlServer)
            {
                var connectionString = "Server=187.84.234.99;Database=dbMGIAdmin;User ID=userArena;Password=arena@2020;";
                optionsBuilder.UseSqlServer(connectionString);
            }
            else
            {
                throw new NotSupportedException($"Database {DbServer} is not supported. Only SQL Server and SQLite are Currently supported.");
            }

            return optionsBuilder.Options;
        }
    }
}

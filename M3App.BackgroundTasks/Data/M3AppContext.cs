using EFCore.BulkExtensions;
using M3.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;

namespace M3App.BackgroundTasks.Data
{
    public class M3AppContext : DbContext
    {
        public M3AppContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.RemovePluralizingTableNameConvention();

            modelBuilder.Entity<App>(entity =>
            {
                entity.ToTable("CargaDadosAppEquipamento", "m3");
                entity.HasNoKey();
            });

            modelBuilder.Entity<App>().Property(x => x.DtLeitura).HasColumnName("DtLeitura");
            modelBuilder.Entity<App>().Property(x => x.IdEmpresa).HasColumnName("IdEmpresa");
            modelBuilder.Entity<App>().Property(x => x.Data).HasColumnName("Data");
            modelBuilder.Entity<App>().Property(x => x.MDM_PROP_VALUE).HasColumnName("MDM_PROP_VALUE");
            modelBuilder.Entity<App>().Property(x => x.DEVICE_NAME).HasColumnName("DEVICE_NAME");
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
            var optionsBuilder = new DbContextOptionsBuilder<M3AppContext>();

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

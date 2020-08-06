using EFCore.BulkExtensions;
using M3.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;

namespace M3.BackgroundTasks.Data
{
    public class M3Context : DbContext
    {
        public M3Context(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Device> Devices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.RemovePluralizingTableNameConvention();

            modelBuilder.Entity<Device>(entity => {
                entity.ToTable("CargaDadosEquipamento","m3");
                entity.HasNoKey();
            });

            modelBuilder.Entity<Device>().Property(x => x.DtLeitura).HasColumnName("DtLeitura");

            modelBuilder.Entity<Device>().Property(x => x.IdEmpresa).HasColumnName("IdEmpresa");
            modelBuilder.Entity<Device>().Property(x => x.Device_ID).HasColumnName("Device_ID");
            modelBuilder.Entity<Device>().Property(x => x.FreeBatteryPercentage).HasColumnName("FreeBatteryPercentage");
            modelBuilder.Entity<Device>().Property(x => x.DEVICE_NAME).HasColumnName("DEVICE_NAME");
            modelBuilder.Entity<Device>().Property(x => x.LastConnection).HasColumnName("LastConnection");
            modelBuilder.Entity<Device>().Property(x => x.unit_group_name).HasColumnName("unit_group_name");
            modelBuilder.Entity<Device>().Property(x => x.FreeMemoryPercentage).HasColumnName("FreeMemoryPercentage");
            modelBuilder.Entity<Device>().Property(x => x.FreeStoragePercentage).HasColumnName("FreeStoragePercentage");

            modelBuilder.Entity<Device>().Property(x => x.DEVICE_EXT_UNIT).HasColumnName("DEVICE_EXT_UNIT");
            modelBuilder.Entity<Device>().Property(x => x.MDM_PROP_VALUE).HasColumnName("MDM_PROP_VALUE");
            modelBuilder.Entity<Device>().Property(x => x.ENABLED).HasColumnName("ENABLED");
            modelBuilder.Entity<Device>().Property(x => x.M3CLIENT_VERSION).HasColumnName("M3CLIENT_VERSION");
            modelBuilder.Entity<Device>().Property(x => x.IDH).HasColumnName("IDH");
            modelBuilder.Entity<Device>().Property(x => x.LastStorageCollectionDate).HasColumnName("LastStorageCollectionDate");
            modelBuilder.Entity<Device>().Property(x => x.UNIT_GROUP_ID).HasColumnName("UNIT_GROUP_ID");
            modelBuilder.Entity<Device>().Property(x => x.Operadora).HasColumnName("Operadora");

            modelBuilder.Entity<Device>().Property(x => x.Modelo_Equipamento).HasColumnName("Modelo_Equipamento");
            modelBuilder.Entity<Device>().Property(x => x.Sistema_Operacional).HasColumnName("Sistema_Operacional");
            modelBuilder.Entity<Device>().Property(x => x.InicioCiclo).HasColumnName("InicioCiclo");
            modelBuilder.Entity<Device>().Property(x => x.Ultima_Coleta).HasColumnName("Ultima_Coleta");
            modelBuilder.Entity<Device>().Property(x => x.Download_MB).HasColumnName("Download_MB");
            modelBuilder.Entity<Device>().Property(x => x.Upload_MB).HasColumnName("Upload_MB");
            modelBuilder.Entity<Device>().Property(x => x.TotalConsumo_MB).HasColumnName("TotalConsumo_MB");
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
            var optionsBuilder = new DbContextOptionsBuilder<M3Context>();

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

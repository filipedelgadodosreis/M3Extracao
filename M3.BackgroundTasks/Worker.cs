using Dapper;
using EFCore.BulkExtensions;
using M3.BackgroundTasks.Data;
using M3.Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace M3.BackgroundTasks
{
    public class Worker : BackgroundService
    {
        private readonly WorkerSettings _settings;

        private readonly ILogger<Worker> _logger;

        private IEnumerable<App> lstApps = null;
        private IEnumerable<Device> lstDevices = null;

        public Worker(ILogger<Worker> logger, WorkerSettings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug("Inica o processo de tratamento dos dados para o Sql-Server.");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(59), stoppingToken).ContinueWith((x) => ProcessWorker());
            }

            _logger.LogDebug("Fim do processo.");
        }

        /// <summary>
        /// Método responsável pelo processamento do Worker
        /// </summary>
        /// <returns></returns>
        private async Task ProcessWorker()
        {
            try
            {
                await ProcessaEquipamentos();
                await ProcesssaInventario();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao carregar dados M3: {ex.Message}");
            }


            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        }

        /// <summary>
        /// Método responsável pelo processamento da rotina de equipamentos
        /// </summary>
        public async Task ProcessaEquipamentos()
        {
            await ExtrairDadosEquipamentos();
            await SalvarDadosEquipamentos();
        }

        /// <summary>
        /// Método responsável pelo processamento da rotina de inventário
        /// </summary>
        public async Task ProcesssaInventario()
        {
            await ExtrairDadosInventario();
            await SalvarDadosInventario();
        }

        /// <summary>
        /// Método responsável por extrair as informações da fonte
        /// </summary>
        private async Task ExtrairDadosEquipamentos()
        {
            string sql = @"SELECT d.Device_ID,
                                   d.FreeBatteryPercentage,
                                   d.DEVICE_NAME,
                                   d.LastConnection,
                                   d.FreeMemoryPercentage,
                                   d.FreeStoragePercentage,
                                   d.DEVICE_EXT_UNIT,       
                                   NULL AS IdEmpresa,
                                   GETDATE() AS DtLeitura,
                                   d.ENABLED,
                                   d.M3CLIENT_VERSION,
                                   d.IDH,
                                   d.LastStorageCollectionDate,
                                   u.unit_group_name,
                                   u.UNIT_GROUP_ID,
                                   u.UNIT_GROUP_EXT_UNIT,
                            	   ISNULL(mp.MDM_PROP_VALUE, '') as Operadora,	   
                            	   ISNULL(mpModel.MDM_PROP_VALUE, '') as Modelo_Equipamento,
                            	   ISNULL(mpOS.MDM_PROP_VALUE, '') as Sistema_Operacional,
                            	   --MAX(CYCLES.DATA_LIMITE) as DataLimite,
                            	   MIN(CYCLES.MDM_PROP_VALUE) as InicioCiclo,
                            	   MAX(DATA_USAGES.CollectedTimeStamp) as Ultima_Coleta,
                            	   round(cast(SUM(DATA_USAGES.DownloadMobileDataUsage) as float) / cast(1024 as float) / cast(1024 as float), 2)  as Download_MB,
                            	   round(cast(SUM(DATA_USAGES.UploadMobileDataUsage) as float) / cast(1024 as float) / cast(1024 as float), 2)  as Upload_MB,
                            	   round(cast(SUM(DATA_USAGES.DownloadMobileDataUsage + DATA_USAGES.UploadMobileDataUsage) as float) / cast(1024 as float) / cast(1024 as float), 2)  as TotalConsumo_MB	   
                            FROM device d
                            INNER JOIN unit_group u ON u.UNIT_GROUP_ID = d.UNIT_GROUP_ID
                            LEFT JOIN DEVICE_MDM_PROPERTY mp ON d.DEVICE_ID = mp.DEVICE_ID
                            AND mp.MDM_PROP_KEY = 'TELEPHONY_OPERATOR'
                            LEFT JOIN DEVICE_MDM_PROPERTY mpOS ON d.DEVICE_ID = mpOS.DEVICE_ID 
                            AND mpOS.MDM_PROP_KEY = 'SYSTEM_OS'
                            LEFT JOIN DEVICE_MDM_PROPERTY mpModel ON d.DEVICE_ID = mpModel.DEVICE_ID 
                            AND mpModel.MDM_PROP_KEY = 'SYSTEM_MODEL'
                            LEFT JOIN
                            (SELECT 
                            M.DeviceId, 
                            M.CollectedTimeStamp,
                            M.DownloadMobileDataUsage,
                            M.UploadMobileDataUsage
                            FROM MobileDataUsages M 
                            Join Device D ON M.DeviceId = D.Device_Id
                            LEFT JOIN Device_Type T ON (D.Device_Type_ID = T.Device_Type_ID)
                            ) as DATA_USAGES
                            ON DATA_USAGES.DeviceId = d.DEVICE_ID
                            
                            LEFT JOIN 
                            (
                            SELECT 
                            DMP.Device_Id,
                            Device_Name,
                            MDM_Prop_Key,
                            MDM_Prop_Value,
                            CASE
                            	WHEN DATEPART(DAY, GETDATE()) >= MDM_Prop_Value THEN DATEADD(dd, MDM_Prop_Value -1, DATEADD(mm, DATEDIFF(mm,0, current_timestamp), 0))
                            	ELSE DATEADD(DD, MDM_Prop_Value -1, DATEADD(MM, -1, DATEADD(mm, DATEDIFF(mm,0, current_timestamp), 0)))
                            END AS DATA_LIMITE
                            FROM 
                            Device D 
                            JOIN Device_MDM_Property DMP ON DMP.Device_Id = D.Device_Id and MDM_Operation_Type = 24
                            LEFT JOIN UNIT_GROUP UG ON (UG.UNIT_GROUP_ID = D.UNIT_GROUP_ID)                    
                            LEFT JOIN Device_Type T ON (D.Device_Type_ID = T.Device_Type_ID)
                            WHERE DMP.MDM_PROP_KEY = 'DATA.USAGE.START.CYCLE'
                            ) as CYCLES ON CYCLES.DEVICE_ID = d.DEVICE_ID
                            
                            WHERE 
                            DATA_USAGES.CollectedTimeStamp > CYCLES.DATA_LIMITE --CRITÉRIO NECESSÁRIO AO CONSUMO DE DADOS
                            AND d.ENABLED = 1  
                            group by
                            d.Device_ID,
                                   d.FreeBatteryPercentage,
                                   d.DEVICE_NAME,
                                   d.LastConnection,
                                   d.FreeMemoryPercentage,
                                   d.FreeStoragePercentage,
                                   d.DEVICE_EXT_UNIT,
                                   mp.MDM_PROP_VALUE,              
                                   d.ENABLED,
                                   d.M3CLIENT_VERSION,
                                   d.IDH,
                                   d.LastStorageCollectionDate,
                                   u.unit_group_name,
                                   u.UNIT_GROUP_ID,
                                   u.UNIT_GROUP_EXT_UNIT,
                            	   mpOS.MDM_PROP_VALUE,
                            	   mpModel.MDM_PROP_VALUE
                            order by d.DEVICE_NAME ";

            using var conn = new SqlConnection(_settings.DefaultConnection);
            try
            {
                conn.Open();

                lstDevices = await conn.QueryAsync<Device>(sql);
            }
            catch (SqlException exception)
            {
                _logger.LogCritical(exception, "FATAL ERROR: Database connections could not be opened: {Message}", exception.Message);
            }
        }

        /// <summary>
        /// Método responsável por salvar as informações na base do cliente 
        /// para geração dos relátorios
        /// </summary>
        private async Task SalvarDadosEquipamentos()
        {
            using (var context = new M3Context(ContextUtil.GetOptions()))
            {
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    var list = lstDevices.ToList();

                    await context.BulkInsertAsync(list);
                    transaction.Commit();
                }
            }
        }
        /// <summary>
        /// Método responsável por recuperar as informações para dados do inventário de software
        /// </summary>
        private async Task ExtrairDadosInventario()
        {
            string sql = @"SELECT GETDATE() AS DtLeitura,
                                                       NULL AS IdEmpresa,
                                                       mdmp_OP_date.MDM_PROP_VALUE AS 'Data',
                                                       mdmp_OP.MDM_PROP_VALUE,
                                                       d.DEVICE_NAME
                                                FROM DEVICE d
                                                INNER JOIN DEVICE_MDM_PROPERTY mdmp_OP_date ON d.DEVICE_ID = mdmp_OP_date.DEVICE_ID
                                                AND mdmp_OP_date.MDM_OPERATION_TYPE = 18
                                                AND mdmp_OP_date.MDM_PROP_KEY = 'OPERATION_EXEC_DATE'
                                                INNER JOIN DEVICE_MDM_PROPERTY mdmp_OP ON d.DEVICE_ID = mdmp_OP.DEVICE_ID
                                                AND mdmp_OP.MDM_OPERATION_TYPE = 18
                                                AND mdmp_OP.MDM_PROP_KEY = 'SOFTWARE_LIST' ";

            using var conn = new SqlConnection(_settings.DefaultConnection);
            try
            {
                conn.Open();

                lstApps = await conn.QueryAsync<App>(sql);
            }
            catch (SqlException exception)
            {
                _logger.LogCritical(exception, "FATAL ERROR: Database connections could not be opened: {Message}", exception.Message);
            }

        }

        /// <summary>
        /// Método responsável por salvar as informações na base do cliente 
        /// para geração dos relátorios
        /// </summary>
        private async Task SalvarDadosInventario()
        {
            using (var context = new M3Context(ContextUtil.GetOptions()))
            {
                using (var transaction = await context.Database.BeginTransactionAsync())
                {
                    var list = lstApps.ToList();

                    await context.BulkInsertAsync(list);
                    transaction.Commit();
                }
            }
        }
    }
}

using Dapper;
using M3.Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Z.Dapper.Plus;

namespace M3.BackgroundTasks
{
    public class Worker : BackgroundService
    {
        private readonly WorkerSettings _settings;

        private readonly ILogger<Worker> _logger;


#pragma warning disable S125 // Sections of code should not be commented out
        //private IEnumerable<App> lstApps = null;

#pragma warning restore S125 // Sections of code should not be commented out

        private IEnumerable<Device> lstDevices = null;
        private bool flagProcess = false;

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
                await Task.Delay(ProcessTime(), stoppingToken).ContinueWith((x) => ProcessWorker());
            }

            _logger.LogDebug("Fim do processo.");
        }

        /// <summary>
        /// Método que calcula o horário de processamento diário
        /// </summary>
        /// <returns></returns>
        private TimeSpan ProcessTime()
        {
            var dateNow = DateTime.Now;

            var horaInicial = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 6, 0, 0);
            var horaFinal = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 21, 0, 0);

            TimeSpan ts;

            if (dateNow <= horaFinal && dateNow >= horaInicial)
            {
                ts = DateTime.Now.AddHours(1) - dateNow;
                flagProcess = false;
            }
            else if (flagProcess)
            {
                ts = DateTime.Now.AddHours(1) - dateNow;
            }
            else
            {
                flagProcess = true;
                ts = DateTime.Now - dateNow;
            }

            return ts;
        }

        private async Task ProcessWorker()
        {
            try
            {
                if (flagProcess)
                {

#pragma warning disable S125 // Sections of code should not be commented out
                    //var m3EmpresaCads = await BuscarEmpresa(46);
                    //await GetApp(lstDevices.Select(x => x.DEVICE_ID).ToList());
#pragma warning restore S125 // Sections of code should not be commented out

                    await ExtrairDados();
                    SalvarDadosEquipamentos();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao carregar dados M3: {ex.Message}");
            }


            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        }

        /// <summary>
        /// Método responsável por extrair as informações da fonte
        /// </summary>
        private async Task ExtrairDados()
        {
            string sql = @"SELECT d.Device_ID,
                                   d.FreeBatteryPercentage,
                                   d.DEVICE_NAME,
                                   d.LastConnection,
                                   d.FreeMemoryPercentage,
                                   d.FreeStoragePercentage,
                                   d.DEVICE_EXT_UNIT,       
                                   46 AS IdEmpresa,
                                   GETUTCDATE() AS DtLeitura,
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
        private void SalvarDadosEquipamentos()
        {
            DapperPlusManager.Entity<Device>().Table("[m3].[CargaDadosEquipamento]");

            using var conn = new SqlConnection(_settings.LocalConnection);
            try
            {
                conn.BulkInsert(lstDevices);
            }
            catch (SqlException exception)
            {
                _logger.LogCritical(exception, "FATAL ERROR: Database connections could not be opened: {Message}", exception.Message);
            }
        }

        //private async Task GetApp(IList<int> appIds)

#pragma warning disable S125 // Sections of code should not be commented out
                            //{
                            //    string sql = @"SELECT mdmp_OP_date.MDM_PROP_VALUE AS 'Data',
                            //                           mdmp_OP.MDM_PROP_VALUE,
                            //                           d.DEVICE_NAME
                            //                    FROM DEVICE d
                            //                    INNER JOIN DEVICE_MDM_PROPERTY mdmp_OP_date ON d.DEVICE_ID = mdmp_OP_date.DEVICE_ID
                            //                    AND mdmp_OP_date.MDM_OPERATION_TYPE = 18
                            //                    AND mdmp_OP_date.MDM_PROP_KEY = 'OPERATION_EXEC_DATE'
                            //                    INNER JOIN DEVICE_MDM_PROPERTY mdmp_OP ON d.DEVICE_ID = mdmp_OP.DEVICE_ID
                            //                    AND mdmp_OP.MDM_OPERATION_TYPE = 18
                            //                    AND mdmp_OP.MDM_PROP_KEY = 'SOFTWARE_LIST'
                            //                    WHERE d.DEVICE_ID IN @ids ";

        //    using var conn = new SqlConnection(_settings.DefaultConnection);
        //    try
        //    {
        //        conn.Open();

        //        //conn.Execute("CREATE TABLE #tempAnimalIds(animalId int not null primary key);");

        //        while (appIds.Any())
        //        {
        //            var ids2Insert = appIds.Take(1000);
        //            appIds = appIds.Skip(1000).ToList();

        //            lstApps = conn.Query<App>(sql, new { ids = ids2Insert }).Map(x => x.IdEmpresa = 46).Map(x => x.DtLeitura = DateTime.Now);

        //            SalvarDadosApp();

        //        }

        //        //return conn.Query<int>(@"SELECT animalID FROM #tempAnimalIds").ToList();
        //    }
        //    catch (SqlException exception)
        //    {
        //        _logger.LogCritical(exception, "FATAL ERROR: Database connections could not be opened: {Message}", exception.Message);
        //    }

        //}

        ///// <summary>
        ///// Método responsável por salvar as informações na base do cliente 
        ///// para geração dos relátorios
        ///// </summary>
        //private void SalvarDadosApp()
        //{
        //    DapperPlusManager.Entity<App>().Table("[m3].[CargaDadosAppEquipamento]");

        //    using (var conn = new SqlConnection(_settings.LocalConnection))
        //    {
        //        try
        //        {
        //            conn.BulkInsert(lstApps);
        //        }
        //        catch (SqlException exception)
        //        {
        //            _logger.LogCritical(exception, "FATAL ERROR: Database connections could not be opened: {Message}", exception.Message);
        //        }
        //    }
        //}

        ///// <summary> 
        ///// Método responsável por buscar as informações de configurações  
        ///// </summary>
        //private async Task<M3EmpresaCad> BuscarEmpresa(int configId)
        //{
        //    M3EmpresaCad m3EmpresaCad = null;

        //    string sql = $"Select IdEmpresa, NmRazaoSocial From [config].[CadEmpresas] where [IdEmpresa] = @ConfigId";

        //    using (var conn = new SqlConnection(_settings.LocalConnection))
        //    {
        //        try
        //        {
        //            conn.Open();

        //            m3EmpresaCad = await conn.QueryFirstOrDefaultAsync<M3EmpresaCad>(sql, new { ConfigId = configId });
        //        }
        //        catch (SqlException exception)
        //        {
        //            _logger.LogCritical(exception, "FATAL ERROR: Database connections could not be opened: {Message}", exception.Message);
        //        }

        //    }

        //    return m3EmpresaCad;
        //}
    }
#pragma warning restore S125 // Sections of code should not be commented out
}

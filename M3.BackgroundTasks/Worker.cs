using Dapper;
using M3.Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Z.Dapper.Plus;

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
            var DailyTime = _settings.DailyTime;
            var timeParts = DailyTime.Split(new char[1] { ':' });

            var dateNow = DateTime.Now;
            var date = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day,
                       int.Parse(timeParts[0]), int.Parse(timeParts[1]), int.Parse(timeParts[2]));

            TimeSpan ts;
            if (date > dateNow)
                ts = date - dateNow;
            else
            {
                date = date.AddDays(1);
                ts = date - dateNow;
            }

            return ts;
        }

        private async Task ProcessWorker()
        {
            try
            {
                // TODO:  Perguntar ao Cicero o pq desta tabela... esqueci
                var m3EmpresaCads = await BuscarEmpresa(46);

                await ExtrairDados();
                SalvarDadosEquipamentos();

                await GetApp(lstDevices.Select(x => x.DEVICE_ID).ToList());

            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao carregar dados M3: {ex.Message}");
            }


            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        }


        private async Task GetApp(IList<int> appIds)
        {
            string sql = @"SELECT mdmp_OP_date.MDM_PROP_VALUE AS 'Data',
                                   mdmp_OP.MDM_PROP_VALUE,
                                   d.DEVICE_NAME
                            FROM DEVICE d
                            INNER JOIN DEVICE_MDM_PROPERTY mdmp_OP_date ON d.DEVICE_ID = mdmp_OP_date.DEVICE_ID
                            AND mdmp_OP_date.MDM_OPERATION_TYPE = 18
                            AND mdmp_OP_date.MDM_PROP_KEY = 'OPERATION_EXEC_DATE'
                            INNER JOIN DEVICE_MDM_PROPERTY mdmp_OP ON d.DEVICE_ID = mdmp_OP.DEVICE_ID
                            AND mdmp_OP.MDM_OPERATION_TYPE = 18
                            AND mdmp_OP.MDM_PROP_KEY = 'SOFTWARE_LIST'
                            WHERE d.DEVICE_ID IN @ids ";

            using var conn = new SqlConnection(_settings.DefaultConnection);
            try
            {
                conn.Open();

                //conn.Execute("CREATE TABLE #tempAnimalIds(animalId int not null primary key);");

                while (appIds.Any())
                {
                    var ids2Insert = appIds.Take(1000);
                    appIds = appIds.Skip(1000).ToList();

                    lstApps = conn.Query<App>(sql, new { ids = ids2Insert }).Map(x => x.IdEmpresa = 46).Map(x => x.DtLeitura = DateTime.Now);

                    SalvarDadosApp();

                }

                //return conn.Query<int>(@"SELECT animalID FROM #tempAnimalIds").ToList();
            }
            catch (SqlException exception)
            {
                _logger.LogCritical(exception, "FATAL ERROR: Database connections could not be opened: {Message}", exception.Message);
            }

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
                                  mp.MDM_PROP_VALUE,
                                  46 AS IdEmpresa,
                                  GETUTCDATE() AS DtLeitura,
                                  d.ENABLED,
                                  d.M3CLIENT_VERSION,
                                  d.IDH,
                                  d.LastStorageCollectionDate,
                                  u.unit_group_name,
                                  u.UNIT_GROUP_ID,
                                  u.UNIT_GROUP_EXT_UNIT
                           FROM device d
                           INNER JOIN unit_group u ON u.UNIT_GROUP_ID = d.UNIT_GROUP_ID
                           LEFT JOIN DEVICE_MDM_PROPERTY mp ON d.DEVICE_ID = mp.DEVICE_ID
                           AND mp.MDM_PROP_KEY = 'TELEPHONY_OPERATOR'
                           WHERE d.DEVICE_EXT_UNIT IN ('rde', 'rd6','army')
                             AND d.ENABLED = 1; ";

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

            using (var conn = new SqlConnection(_settings.LocalConnection))
            {
                try
                {
                    conn.BulkInsert(lstDevices);
                }
                catch (SqlException exception)
                {
                    _logger.LogCritical(exception, "FATAL ERROR: Database connections could not be opened: {Message}", exception.Message);
                }
            }
        }

        /// <summary>
        /// Método responsável por salvar as informações na base do cliente 
        /// para geração dos relátorios
        /// </summary>
        private void SalvarDadosApp()
        {
            DapperPlusManager.Entity<App>().Table("[m3].[CargaDadosAppEquipamento]");

            using (var conn = new SqlConnection(_settings.LocalConnection))
            {
                try
                {
                    conn.BulkInsert(lstApps);
                }
                catch (SqlException exception)
                {
                    _logger.LogCritical(exception, "FATAL ERROR: Database connections could not be opened: {Message}", exception.Message);
                }
            }
        }

        /// <summary> 
        /// Método responsável por buscar as informações de configurações  
        /// </summary>
        private async Task<M3EmpresaCad> BuscarEmpresa(int configId)
        {
            M3EmpresaCad m3EmpresaCad = null;

            string sql = $"Select IdEmpresa, NmRazaoSocial From [config].[CadEmpresas] where [IdEmpresa] = @ConfigId";

            using (var conn = new SqlConnection(_settings.LocalConnection))
            {
                try
                {
                    conn.Open();

                    m3EmpresaCad = await conn.QueryFirstOrDefaultAsync<M3EmpresaCad>(sql, new { ConfigId = configId });
                }
                catch (SqlException exception)
                {
                    _logger.LogCritical(exception, "FATAL ERROR: Database connections could not be opened: {Message}", exception.Message);
                }

            }

            return m3EmpresaCad;
        }
    }
}

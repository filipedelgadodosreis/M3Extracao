using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using EFCore.BulkExtensions;
using M3.Domain;
using M3App.BackgroundTasks.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace M3App.BackgroundTasks
{
    public class Worker : BackgroundService
    {
        private readonly WorkerSettings _settings;

        private IEnumerable<App> lstApps = null;

        private readonly ILogger<Worker> _logger;

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
                await ProcesssaApp();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao carregar dados M3: {ex.Message}");
            }

            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        }

        /// <summary>
        /// Método responsável pelo processamento da rotina de app
        /// </summary>
        public async Task ProcesssaApp()
        {
            await ExtrairDadosApp();
            await SalvarDadosApp();
        }

        /// <summary>
        /// Método responsável por recuperar as informações para dados do inventário de software
        /// </summary>
        private async Task ExtrairDadosApp()
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
        private async Task SalvarDadosApp()
        {
            using var context = new M3AppContext(ContextUtil.GetOptions());
            using var transaction = await context.Database.BeginTransactionAsync();
            var list = lstApps.ToList();

            await context.BulkInsertAsync(list);
            transaction.Commit();
        }
    }
}

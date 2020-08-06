using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using EFCore.BulkExtensions;
using M3.Domain;
using M3Inventario.BackgroundTasks.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace M3Inventario.BackgroundTasks
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private readonly WorkerSettings _settings;
        private readonly List<Inventario> lstInventario = new List<Inventario>();

        private IEnumerable<App> lstApps = null;

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
                await ProcesssaInventario();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao carregar dados M3: {ex.Message}");
            }

            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
        }

        /// <summary>
        /// Método responsável pelo processamento da rotina de inventário
        /// </summary>
        public async Task ProcesssaInventario()
        {
            await ExtrairDadosApp();
            ExtrairDadosInventario();
            await SalvarDadosInventario();
        }

        /// <summary>
        /// Método para extrair da lista principal os dados de inventario
        /// </summary>
        private void ExtrairDadosInventario()
        {
            for (int i = 0; i < lstApps.Count() - 1; i++)
            {
                var item = lstApps.ElementAt(i).MDM_PROP_VALUE;

                if (!string.IsNullOrWhiteSpace(item))
                {
                    string[] arrayInventario = item.Split(new char[] { '/' });

                    for (int x = 0; x < arrayInventario.Length - 1; x++)
                    {
                        var arrayDados = arrayInventario[x].Split(new char[] { '|' });

                        TratarDadosInventario(arrayDados, i);
                    }
                }
            }
        }

        /// <summary>
        /// Método para tratar os dados e inserir na lista de inventario
        /// </summary>
        /// <param name="arrayDados"></param>
        /// <param name="index"></param>
        private void TratarDadosInventario(string[] arrayDados, int index)
        {
            var obj = lstApps.ElementAt(index);
            var inventario = new Inventario();

            inventario.DATA = obj.Data.Value;
            inventario.DEVICE_NAME = obj.DEVICE_NAME;

            if (arrayDados.Length == 1)
            {
                inventario.APP_NAME = arrayDados[0];
            }
            else if (arrayDados.Length == 2)
            {
                inventario.APP_NAME = arrayDados[0];
                inventario.VERSAO = arrayDados[1];
            }
            else
            {
                inventario.APP_NAME = arrayDados[0];
                inventario.VERSAO = arrayDados[1];
                inventario.IND_SISTEMA = arrayDados[2] == "1";
            }

            lstInventario.Add(inventario);
        }

        /// <summary>
        /// Método responsável por salvar as informações na base do cliente 
        /// para geração dos relátorios
        /// </summary>
        private async Task SalvarDadosInventario()
        {
            using var context = new M3InventarioContext(ContextUtil.GetOptions());
            using var transaction = await context.Database.BeginTransactionAsync();

            await context.BulkInsertAsync(lstInventario);
            transaction.Commit();

            lstInventario.Clear();
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
    }
}

using Dapper;
using M3.Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace M3.BackgroundTasks
{
    public class Worker : BackgroundService
    {
        private readonly WorkerSettings _settings;

        private readonly ILogger<Worker> _logger;
        private readonly List<Device> lstDevices = new List<Device>();

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
                try
                {
                    var m3EmpresaCads = await BuscarEmpresa(46);

                    ExtrairDados();

                }
                catch (Exception ex)
                {
                    _logger.LogError($"Erro ao carregar dados M3: {ex.Message}");
                }


                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }

            _logger.LogDebug("Fim do processo.");
        }

        /// <summary>
        /// Método responsável por extrair as informações da fonte
        /// </summary>
        private async void ExtrairDados()
        {
            //var lista = await _deviceRepository.Get();
        }

        /// <summary>
        /// Método responsável por salvar as informações na base do cliente 
        /// para geração dos relátorios
        /// </summary>
        private void SalvarDados()
        {

        }

        /// <summary> 
        /// Método responsável por buscar as informações de configurações  
        /// </summary>
        private async Task<M3EmpresaCad> BuscarEmpresa(int configId)
        {
            M3EmpresaCad m3EmpresaCad = null;

            string sql = $"Select IdEmpresa, NmRazaoSocial From [config].[CadEmpresas] where [IdEmpresa] = @ConfigId";

            using (var conn = new SqlConnection(_settings.DefaultConnection))
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

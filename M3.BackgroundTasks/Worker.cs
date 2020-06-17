using M3.Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Repository.Dapper.Contracts;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace M3.BackgroundTasks
{
    public class Worker : BackgroundService
    {
        private readonly IConfigRepository _repository;
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger, IConfigRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug("Inica o processo de tratamento dos dados para o Sql-Server.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var m3EmpresaCads = await _repository.GetById(46);
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
        private void ExtrairDados()
        {

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
        private void BuscarEmpresa()
        {

        }
    }
}

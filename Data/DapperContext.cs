
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MobileWebApi.Constants;
using System;
using System.Data;
using System.Data.SqlClient;

namespace MobileWebApi.Data
{
	public class DapperContext
	{
		private readonly IConfiguration _configuration;
		private readonly ILogger<DapperContext> _logger;
		private readonly string? _connectionString;

		public DapperContext(IConfiguration configuration, ILogger<DapperContext> logger)
		{
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			_connectionString = _configuration.GetConnectionString("ConnectionString");

			if (string.IsNullOrWhiteSpace(_connectionString))
			{
				_logger.LogError(LogMessages.Database.ConnectionStringMissing);
				throw new InvalidOperationException("Connection string 'ConnectionString' is not configured.");
			}

			_logger.LogInformation(LogMessages.Database.DapperContextInitialized);
		}

		public IDbConnection CreateConnection()
		{
			_logger.LogInformation(LogMessages.Database.CreatingNewSqlConnection);
			return new SqlConnection(_connectionString);
			

		}
	}
}

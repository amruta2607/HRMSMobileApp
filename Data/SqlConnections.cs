using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MobileWebApi.Interfaces;

namespace MobileWebApi.Data
{
    /// <summary>
    /// Simple implementation of ISqlConnections for database connection management
    /// </summary>
    public class DefaultSqlConnections : ISqlConnections
    {
        private readonly IConfiguration _configuration;

        public DefaultSqlConnections(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IDbConnection New(string connectionKey, string dialect = "Default")
        {
            var connectionString = _configuration.GetConnectionString(connectionKey);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = _configuration.GetConnectionString("ConnectionString");
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException($"Connection string '{connectionKey}' is not configured.");
            }

            return new SqlConnection(connectionString);
        }

        public IDbConnection NewByKey(string connectionKey)
        {
            return New(connectionKey);
        }
    }
}


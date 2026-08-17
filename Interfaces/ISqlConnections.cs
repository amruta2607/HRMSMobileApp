using System.Data;

namespace MobileWebApi.Interfaces
{
    /// <summary>
    /// Interface for SQL connection management (compatible with Serenity pattern)
    /// </summary>
    public interface ISqlConnections
    {
        /// <summary>
        /// Creates a new database connection
        /// </summary>
        /// <param name="connectionKey">Connection string key from configuration</param>
        /// <param name="dialect">Database dialect (optional)</param>
        /// <returns>Database connection</returns>
        IDbConnection New(string connectionKey, string dialect = "Default");

        /// <summary>
        /// Creates a new database connection by key
        /// </summary>
        /// <param name="connectionKey">Connection string key from configuration</param>
        /// <returns>Database connection</returns>
        IDbConnection NewByKey(string connectionKey);
    }
}


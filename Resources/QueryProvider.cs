using Microsoft.Extensions.Configuration;
namespace MobileWebApi.Resources
{
    

    public class QueryProvider
    {
        private readonly IConfiguration _config;

        public QueryProvider(IConfiguration config)
        {
            _config = config;
        }

        public string Get(string key)
        {
            var sql = _config[$"SQLQueries:{key}"];
            if (string.IsNullOrWhiteSpace(sql))
            {
                throw new InvalidOperationException(
                    $"SQL query '{key}' is missing or empty. Ensure Resources/queries.json defines SQLQueries:{key} and that the file is deployed with the application (same folder layout as development).");
            }

            return sql;
        }
    }

}

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
            return _config[$"SQLQueries:{key}"];
        }
    }

}

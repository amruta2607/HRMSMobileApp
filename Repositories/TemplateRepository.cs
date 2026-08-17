using Dapper;
using MobileWebApi.Constants;
using MobileWebApi.Data;
using MobileWebApi.Helper;
using MobileWebApi.Interfaces;
using MobileWebApi.Models.Responses;
using MobileWebApi.Resources;

namespace MobileWebApi.Repositories
{
    /// <summary>
    /// Provides tenant-scoped template data access using Dapper.
    /// </summary>
    public class TemplateRepository : ITemplateRepository
    {
        private readonly DapperContext _context;
        private readonly ITenantContext _tenantContext;
        private readonly QueryProvider _queries;
        private readonly ILogger<TemplateRepository> _logger;

        public TemplateRepository(
            DapperContext context,
            ITenantContext tenantContext,
            QueryProvider queries,
            ILogger<TemplateRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TemplateResponse>> GetTemplatesAsync()
        {
            try
            {
                var tenantId = _tenantContext.GetRequiredOrganisationId();
                var sql = _queries.Get("GetTemplates");

                using var connection = _context.CreateConnection();
                return await connection.QueryAsync<TemplateResponse>(sql, new { TenantId = tenantId });
            }
            catch (Exception ex)
            {
                _logger.LogException(
                    ExceptionCodes.Template.GetList,
                    nameof(GetTemplatesAsync),
                    ex,
                    _tenantContext.UserId);

                throw;
            }
        }
    }
}

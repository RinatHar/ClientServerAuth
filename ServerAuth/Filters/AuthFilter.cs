using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Relational;
using ServerAuth.Services;
using System.Configuration;
using System.Data;
using System.Text;

namespace ServerAuth.Filters
{
    public class AuthFilter : IAsyncAuthorizationFilter
    {
        private readonly ILogger<AuthFilter> _logger;
        private readonly AuthService _authService;

        public AuthFilter(ILogger<AuthFilter> logger, AuthService authService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authService = authService;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {

            string authHeader = context.HttpContext.Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(authHeader) || !TryParseAuthorizationHeader(authHeader, out string login, out string password))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            DataTable rights = await _authService.GetUserRights(login, password);

            if (rights.Rows.Count > 0)
            {
                SetUserRightsInContext(context, rights);
            }
            else
            {
                context.Result = new UnauthorizedResult();
            }
        }

        private bool TryParseAuthorizationHeader(string authHeader, out string login, out string password)
        {
            login = password = null;

            if (string.IsNullOrEmpty(authHeader))
                return false;

            string[] authHeaderParts = authHeader.Split(' ');
            if (authHeaderParts.Length != 2 || authHeaderParts[0].ToLower() != "basic")
                return false;

            try
            {
                string decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(authHeaderParts[1]));

                string[] credentials = decodedCredentials.Split(':');

                if (credentials.Length != 2)
                    return false;

                login = credentials[0];
                password = credentials[1];
                return true;
            }
            catch (FormatException)
            {
                // В случае ошибки декодирования строки Base64
                return false;
            }
        }


        private void SetUserRightsInContext(AuthorizationFilterContext context, DataTable rights)
        {
            List<string> rightsList = rights.AsEnumerable()
                                            .Select(row => row.Field<string>("right"))
                                            .ToList();

            string[] resultArray = [.. rightsList];
            context.HttpContext.Items["UserRights"] = new JsonResult(resultArray);
        }
    }
}
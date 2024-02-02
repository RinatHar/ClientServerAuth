using ServerAuth.Dto;
using ServerAuth.Models;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text;
using ServerAuth.Filters;
using ServerAuth.Services;

namespace Lab1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly DataService _dataService;
        private readonly AuthService _authService;

        public DataController(IConfiguration configuration, DataService dataService, AuthService authService)
        {
            _configuration = configuration;
            _dataService = dataService;
            _authService = authService;
        }

        [HttpGet("read")]
        [TypeFilter(typeof(AuthFilter))]
        [ProducesResponseType(typeof(Data), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Get()
        {
            try
            {
                string query = @"select id, value from data";

                Dictionary<string, object> parameters = [];
                DataTable table = await _dataService.GetDataAsync(query, parameters);

                return new JsonResult(table);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("write")]
        [TypeFilter(typeof(AuthFilter))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Post(DataDto data)
        {
            try
            {
                // Проверка прав доступа
                if (!_authService.UserHasWriteAccess(HttpContext))
                {
                    return new StatusCodeResult(StatusCodes.Status403Forbidden);
                }

                string query = @"insert into data(value) values (@value)";

                Dictionary<string, object> parameters = new()
                {
                    { "@value", data.value }
                };
                _ = await _dataService.ExecuteAsync(query, parameters);

                return new JsonResult("Added successfully!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("login")]
        [TypeFilter(typeof(AuthFilter))]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Login()
        {
            // Проверяем наличие прав в Items
            if (HttpContext.Items.TryGetValue("UserRights", out var userRights))
            {
                // Приводим объект к нужному типу (JsonResult)
                var userRightsJson = userRights as JsonResult;

                if (userRightsJson != null)
                {
                    return userRightsJson;
                }
            }

            // Если права не найдены, возвращаем Unauthorized
            return Unauthorized();
        }


        [HttpPost("registration")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Registration(UserDto user)
        {
            try
            {
                await _authService.RegisterUser(user);
                return new JsonResult("Registration successful!");
            }

            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

    }
}

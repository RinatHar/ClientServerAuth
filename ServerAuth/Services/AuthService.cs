using Microsoft.AspNetCore.Mvc;
using ServerAuth.Dto;
using System.Data;
using System.Text;
using System.Security.Cryptography;

namespace ServerAuth.Services
{
    public class AuthService
    {
        private readonly DataService _dataService;

        public AuthService(DataService dataService)
        {
            _dataService = dataService;
        }

        public async Task<DataTable> GetUserRights(string login, string password)
        {
            DataTable users = await FindUserByCredentials(login, password);

            if (users.Rows.Count > 0)
            {
                DataRow row = users.Rows[0];
                int id = Convert.ToInt32(row["id"]);
                Console.Out.WriteLine(id.ToString());

                DataTable rights = await GetUserRightsById(id);

                return rights;
            }
            else
            {
                return new DataTable();
            }
        }

        public async Task<DataTable> FindUserByCredentials(string login, string password)
        {
            string query = @"select id from users where login=@login and password=@password";
            Dictionary<string, object> parameters = new()
            {
                { "@login", login },
                { "@password", password }
            };
            return await _dataService.GetDataAsync(query, parameters);
        }

        public async Task<DataTable> GetUserRightsById(int id)
        {
            string query = @"select rights.`right` from users join users_rights on users_rights.users_id = users.id join rights on users_rights.rights_id = rights.id where users.id = @id";
            Dictionary<string, object> parameters = new()
            {
                    { "@id", id }
                };
            return await _dataService.GetDataAsync(query, parameters);
        }

        public bool UserHasWriteAccess(HttpContext httpContext)
        {
            // Проверяем наличие прав в Items
            if (httpContext.Items.TryGetValue("UserRights", out var userRights) && userRights is JsonResult userRightsJson)
            {
                if (userRightsJson.Value is string[] rightsArray)
                {
                    return rightsArray.Contains("WRITE");
                }
            }
            return false;
        }

        public async Task RegisterUser(UserDto user)
        {
            // Хеширование пароля по алгоритму SHA1
            string hashedPassword = GetSHA1Hash(user.password);

            // Запрос для вставки пользователя в таблицу users
            string insertUserQuery = @"insert into users(login, password) 
                                   values (@login, @password);
                                   select last_insert_id();";

            // Параметры для запроса
            Dictionary<string, object> userParameters = new Dictionary<string, object>
        {
            { "@login", user.login },
            { "@password", hashedPassword } // Используем хешированный пароль
        };

            // Выполняем запрос для вставки пользователя в таблицу users
            int userId = await _dataService.ExecuteScalarAsync<int>(insertUserQuery, userParameters);

            AddUserReadRight(userId);
        }

        public async void AddUserReadRight(int userId)
        {
            // Запрос для создания записи в таблице users_rights
            string insertRightsQuery = @"insert into users_rights(users_id, rights_id) values (@userId, 1)";

            // Параметры для запроса
            Dictionary<string, object> rightsParameters = new()
                {
                    { "@userId", userId}
                };

            // Выполняем запрос для создания записи в таблице users_rights
            _ = await _dataService.ExecuteAsync(insertRightsQuery, rightsParameters);
        }

        private static string GetSHA1Hash(string input)
        {
            byte[] hashBytes = SHA1.HashData(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }


    }
}

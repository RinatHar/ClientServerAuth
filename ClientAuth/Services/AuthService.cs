using ClientAuth.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Security.Cryptography;
using System.Windows;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace ClientAuth.Services
{
    internal class AuthService
    {
        private readonly HttpClient _client = new HttpClient();

        public async Task<List<Data>> GetDataFromServerAsync(string url)
        {
            try
            {
                var response = await _client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Data>>(data);
                }
            }
            catch { }

            return null;
        }

        public async Task<bool> AddDataToServerAsync(string url, string value)
        {
            var data = new DataDto { Value = value };
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch { }

            return false;
        }

        public async Task<List<string>> LoginUser(string login, string password)
        {
            // Создаем экземпляр HttpClient
            using HttpClient client = new();

            string hashedPass = GetSHA1Hash(password);

            // Кодируем логин и пароль в формате Base64
            string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{login}:{hashedPass}"));

            // Создаем объект HttpRequestMessage с заголовком Authorization
            var request = new HttpRequestMessage(HttpMethod.Get, "https://localhost:7218/api/data/login");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            // Отправляем запрос без тела
            var response = await client.SendAsync(request);

            // Проверяем успешность запроса
            if (response.IsSuccessStatusCode)
            {
                // Получаем ответ от сервера
                string responseBody = await response.Content.ReadAsStringAsync();

                // Десериализуем JSON в List<string>
                List<string> permissions = JsonConvert.DeserializeObject<List<string>>(responseBody);

                // Возвращаем массив строк Permissions
                return permissions;
            }
            else
            {
                throw new Exception("Ошибка при входе. Пожалуйста, попробуйте еще раз.");
            }
        }

        public async Task AddUser(string login, string password)
        {
            // Создаем экземпляр HttpClient
            using HttpClient client = new();

            string url = "https://localhost:7218/api/data/registration";
            var data = new Dictionary<string, string>
                    {
                        { "login", login },
                        { "password", password }
            };

            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            // Отправляем POST-запрос на сервер
            var response = await _client.PostAsync(url, content);

            // Проверяем успешность запроса
            if (response.IsSuccessStatusCode)
            {
                // Получаем ответ от сервера
                string responseBody = await response.Content.ReadAsStringAsync();

                MessageBox.Show("Регистрация прошла успешно.");
            }
            else
            {
                throw new Exception("Ошибка при входе. Пожалуйста, попробуйте еще раз.");
            }
        }

        public static string GetSHA1Hash(string input)
        {
            byte[] hashBytes = SHA1.HashData(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}

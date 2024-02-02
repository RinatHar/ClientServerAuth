using ClientAuth.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ClientAuth.Services
{
    internal class DataService
    {
        private readonly HttpClient _client = new HttpClient();

        public async Task<List<Data>> GetDataFromServerAsync(string url, string login, string password)
        {
            try
            {
                string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{login}:{password}"));

                // Создаем объект HttpRequestMessage с заголовком Authorization
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

                // Отправляем запрос без тела
                var response = await _client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<Data>>(data);
                }
            }
            catch { }

            return null;
        }

        public async Task<bool> AddDataToServerAsync(string url, string value, string login, string password)
        {
            var data = new DataDto { Value = value };
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                string credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{login}:{password}"));

                // Создаем объект HttpRequestMessage с заголовком Authorization
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                request.Content = content;

                // Отправляем запрос с телом
                var response = await _client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch { }

            return false;
        }
    }
}

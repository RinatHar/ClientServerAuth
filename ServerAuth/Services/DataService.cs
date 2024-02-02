using MySql.Data.MySqlClient;
using System.Data;

namespace ServerAuth.Services
{
    public class DataService(IConfiguration configuration)
    {
        private readonly IConfiguration _configuration = configuration;

        public async Task<DataTable> GetDataAsync(string query, Dictionary<string, object> parameters)
        {
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("DefaultConnection");
            using (MySqlConnection mycon = new(sqlDataSource))
            {
                await mycon.OpenAsync();
                using MySqlCommand myCommand = new(query, mycon);
                foreach (var param in parameters)
                {
                    myCommand.Parameters.AddWithValue(param.Key, param.Value);
                }

                using var myReader = await myCommand.ExecuteReaderAsync();
                table.Load(myReader);
            }

            return table;
        }

        public async Task<int> ExecuteAsync(string query, Dictionary<string, object> parameters)
        {
            string sqlDataSource = _configuration.GetConnectionString("DefaultConnection");
            using MySqlConnection mycon = new(sqlDataSource);
            await mycon.OpenAsync();
            using MySqlCommand myCommand = new(query, mycon);
            foreach (var param in parameters)
            {
                myCommand.Parameters.AddWithValue(param.Key, param.Value);
            }

            return await myCommand.ExecuteNonQueryAsync();
        }

        public async Task<T> ExecuteScalarAsync<T>(string query, Dictionary<string, object> parameters)
        {
            string sqlDataSource = _configuration.GetConnectionString("DefaultConnection");
            using MySqlConnection mycon = new(sqlDataSource);
            await mycon.OpenAsync();
            using MySqlCommand myCommand = new(query, mycon);
            foreach (var param in parameters)
            {
                myCommand.Parameters.AddWithValue(param.Key, param.Value);
            }

            var result = await myCommand.ExecuteScalarAsync();
            return (T)Convert.ChangeType(result, typeof(T));
        }
    }
}

using System;
using System.Text;
using System.Data;
using Npgsql;

namespace apply_neural_network.databases
{
    public class Postgres : IDisposable
    {
        private NpgsqlConnection _con;

        public Postgres()
        {
            _con = new NpgsqlConnection("Host=localhost;Username=postgres;Password=postgres;");
        }

        public NpgsqlDataReader selectQuery(string taskId)
        {
            try
            {
                _con.Open();
                var sql = "SELECT status, result FROM tasks WHERE task_id = @task_id";
                using (var cmd = new NpgsqlCommand(sql, _con))
                {
                    cmd.Parameters.AddWithValue("task_id", taskId);
                    cmd.Prepare();
                    return cmd.ExecuteReader();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
        public void Dispose()
        {
            _con.Close();
        }
    }
}

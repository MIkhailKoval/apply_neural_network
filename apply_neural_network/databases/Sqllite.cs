using System;
using System.Text;
using System.Data;
using System.Data.SQLite;

namespace apply_neural_network.databases
{
    public class Sqllite : IDisposable
    {
        private SQLiteConnection _sqlite;

        public Sqllite()
        {
              _sqlite = new SQLiteConnection("Data Source=statuses.db; Version=3;New=False;");
        }

        public SQLiteDataReader selectQuery(string query)
        {

              try
              {
                    SQLiteCommand cmd;
                    _sqlite.Open();
                    cmd = _sqlite.CreateCommand();
                    cmd.CommandText = query;  //set the passed query
                    return cmd.ExecuteReader();
              }
              catch(Exception ex)
              {
                  Console.WriteLine(ex);
                  return null;
              }
        }
        public void Dispose() {
            _sqlite.Close();
        }
    }
}

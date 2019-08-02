using System;
using System.Data.SqlClient;

namespace TestDb
{
    class Program
    {
        static string _cn = "Server=tcp:testdbal.database.windows.net,1433;Initial Catalog=testdbal;Persist Security Info=False;User ID=usertest;Password=Unmotdepasse59!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        static void Main(string[] args)
        {
            var conn = new SqlConnection(_cn);
            var cmd = new SqlCommand("SELECT * from TestTable");

            conn.Open();
            //cmd.ExecuteReader();
            conn.Close();
        }
    }
}

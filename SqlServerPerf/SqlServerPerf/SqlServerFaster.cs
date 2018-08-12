using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerPerf
{
    public class SqlServerFaster
    {
        private const string databaseName = "SampleDB";
        public void Faster()
        {
            var builder = new SqlConnectionStringBuilder();
            builder.DataSource = "DESKTOP-0NI9LQM";
            builder.InitialCatalog = "master";
            builder.UserID = "sa";
            builder.Password = "357592895";

            using (var connection = new SqlConnection(builder.ConnectionString))
            {
                connection.Open();

                var sql = $"DROP DATABASE IF EXISTS [{databaseName}]; CREATE DATABASE [{databaseName}]";

                using (var command = new SqlCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine($"{databaseName} created");
                }
                Console.Write($"Inserting 5 million rows into table 'Table_with_5M_rows'. This takes ~1 minute, please wait ... ");

                var sb = new StringBuilder();
                sb.Append($"USE {databaseName};");
                sb.Append("WITH a AS (SELECT * FROM (VALUES(1),(2),(3),(4),(5),(6),(7),(8),(9),(10)) AS a(a))");
                sb.Append("SELECT TOP(5000000)");
                sb.Append("ROW_NUMBER() OVER (ORDER BY a.a) AS OrderItemId ");
                sb.Append(",a.a + b.a + c.a + d.a + e.a + f.a + g.a + h.a AS OrderId ");
                sb.Append(",a.a * 10 AS Price ");
                sb.Append(",CONCAT(a.a, N' ', b.a, N' ', c.a, N' ', d.a, N' ', e.a, N' ', f.a, N' ', g.a, N' ', h.a) AS ProductName ");
                sb.Append("INTO Table_with_5M_rows ");
                sb.Append("FROM a, a AS b, a AS c, a AS d, a AS e, a AS f, a AS g, a AS h;");

                using (var command = new SqlCommand(sb.ToString(), connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Done.");
                }

                var elapsedTimeWithoutIndex = SumPrice(connection);

                Console.Write("Adding a columnstore to table 'Table_with_5M_rows'  ... ");
                sql = "CREATE CLUSTERED COLUMNSTORE INDEX columnstoreindex ON Table_with_5M_rows;";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Done.");
                }
                
                var elapsedTimeWithIndex = SumPrice(connection);

                Console.WriteLine("Query time WITH columnstore index: " + elapsedTimeWithIndex + "ms");
                Console.WriteLine("Performance improvement with columnstore index: "
                    + Math.Round(elapsedTimeWithoutIndex / elapsedTimeWithIndex) + "x!");
            }
        }

        private double SumPrice(SqlConnection connection)
        {
            var sql = "SELECT SUM(Price) FROM Table_with_5M_rows";
            var stackTicks = DateTime.Now.Ticks;
            using (var command = new SqlCommand(sql, connection))
            {
                try
                {
                    var sum = command.ExecuteScalar();
                    var elapsed = TimeSpan.FromTicks(DateTime.Now.Ticks) - TimeSpan.FromTicks(stackTicks);
                    return elapsed.TotalMilliseconds;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            return 0;
        }
    }
}

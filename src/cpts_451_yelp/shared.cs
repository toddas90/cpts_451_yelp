using System;
using Eto.Forms;
using Eto.Drawing;
using Npgsql;

namespace cpts_451_yelp
{
    public class Business
    {
        public string name { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string zip { get; set; }
        public string bid { get; set; }
    }

    public class TipInfo
    {
        public DateTime date { get; set; }
        public string name { get; set; }
        public int likes { get; set; }
        public string text { get; set; }
    }

    public class SharedInfo
    {
        private string connectionInfo()
        {
            return "Host=192.168.0.250; Username=postgres; Database=test_yelp; Password=mustafa; Timeout=5";
        }

        // Executes the queries, straight out of the video
        public void executeQuery(string sqlstr, Action<NpgsqlDataReader> myf)
        {
            using (var connection = new NpgsqlConnection(connectionInfo()))
            {
                try
                {
                    connection.Open();
                }
                catch (Npgsql.NpgsqlException ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                    MessageBox.Show(ex.Message.ToString());
                    System.Environment.Exit(1);
                }
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandText = sqlstr;
                    try
                    {
                        // Console.WriteLine("Executing Query: " + sqlstr); // For debugging
                        var reader = cmd.ExecuteReader();
                        while (reader.Read())
                            myf(reader);
                    }
                    catch (NpgsqlException ex)
                    {
                        Console.WriteLine(ex.Message.ToString());
                        MessageBox.Show("SQL Error - " + ex.Message.ToString());
                    }
                    catch (System.TimeoutException ex)
                    {
                        Console.WriteLine(ex.Message.ToString());
                        MessageBox.Show(ex.Message.ToString());
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }
    }
}
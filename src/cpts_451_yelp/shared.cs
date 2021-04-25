using System;
using System.Collections;
using System.Collections.Generic;
using Eto.Forms;
using Eto.Drawing;
using Npgsql;

namespace cpts_451_yelp
{
    public class Business // Business class for keeping data together
    {
        public string name { get; set; }
        public string state { get; set; }
        public string city { get; set; }
        public string zip { get; set; }
        public string bid { get; set; }
        public string addy { get; set; }
        public double dist { get; set; }
        public double stars { get; set; }
        public int tips { get; set; }
        public int checkins { get; set; }
    }

    public class TipInfo // Keeps the tip info together
    {
        public string uid { get; set; }
        public DateTime date { get; set; }
        public string name { get; set; }
        public int likes { get; set; }
        public string text { get; set; }
        public string businessname { get; set; }
        public string city { get; set; }

    }


    public class UserInfo // Keeps user info together
    {
        public string Username { get; set; }
        public string UserID { get; set; }
        public double UserLat { get; set; }
        public double UserLong { get; set; }
        public int likes { get; set; }
        public double avgStars { get; set; }
        public DateTime date { get; set; }
        public int tipCount { get; set; }
        public int fans { get; set; }
        public int funny { get; set; }
        public int cool { get; set; }
        public int useful { get; set; }

    }

    // public class AttInfo
    // {
    //     public string displayname { get; set; }
    //     public string attributename { get; set; }
    //     public string value { get; set; }
    // }

    public class SharedInfo // Connection info, and common methods
    {
        private string connectionInfo()
        {
            return "Host=localhost; Username=postgres; Database=test_yelp; Password=mustafa; Timeout=5";
        }


        // Executes the queries, straight out of the video, true for read, false for write
        public void executeQuery(string sqlstr, Action<NpgsqlDataReader> myf, bool io)
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
                        //Console.WriteLine("Executing Query: " + sqlstr); // For debugging
                        if (io == true)
                        {
                            var reader = cmd.ExecuteReader();
                            while (reader.Read())
                                myf(reader);
                        }
                        else if (io == false)
                        {
                            cmd.ExecuteNonQuery();
                        }
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
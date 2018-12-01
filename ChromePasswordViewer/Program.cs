using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ChromePasswordViewer
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                @"\Google\Chrome\User Data\Default\Login Data";
            var tempfile = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "Login Data.tmp";
            File.Copy(path, tempfile, true);
            var connectionString = new SQLiteConnectionStringBuilder { DataSource = tempfile };

            using (var connect = new SQLiteConnection(connectionString.ConnectionString))
            {
                var dataTable = new DataTable();
                var command = new SQLiteCommand("select * from logins", connect);
                var adapter = new SQLiteDataAdapter(command);

                adapter.Fill(dataTable);

                foreach (var row in dataTable.Rows.Cast<DataRow>())
                {
                    var startTime = new DateTime(1601, 01, 01);
                    var timeLong = (long)row["date_created"] / 1000000;
                    var timeString = $"* {startTime.AddSeconds(timeLong)} *";
                    var byteArray = (byte[])row["password_value"];
                    var userData = ProtectedData.Unprotect(byteArray, null, DataProtectionScope.CurrentUser);
                    var boundary = "".PadLeft(timeString.Length, '*');
                    var url = row["origin_url"];
                    var username = row["username_value"];
                    var password = Encoding.UTF8.GetString(userData);

                    Console.WriteLine(boundary);
                    Console.WriteLine(timeString);
                    Console.WriteLine(boundary);
                    Console.WriteLine("url:      {0}", url);
                    Console.WriteLine("username: {0}", username);
                    Console.WriteLine("password: {0}", password);
                    Console.WriteLine();
                }
            }

            Console.ReadLine();
        }
    }
}

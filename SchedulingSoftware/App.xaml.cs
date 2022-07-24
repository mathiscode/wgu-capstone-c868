using MySql.Data.MySqlClient;
using SchedulingSoftware.Classes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Extensions;

namespace SchedulingSoftware
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static MySqlConnection db;
        public static User me;

        public App()
        {
            // For development
            //CultureInfo startupCulture = new CultureInfo("de");
            CultureInfo startupCulture = Thread.CurrentThread.CurrentUICulture;

            Thread.CurrentThread.CurrentCulture = startupCulture;
            Thread.CurrentThread.CurrentUICulture = startupCulture;
            LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
            LocalizeDictionary.Instance.Culture = startupCulture;

            //string connectionString = "server=localhost;port=3306;user=sqlUser;password=Passw0rd!;database=client_schedule";
            string connectionString = "server=cloud155.hostgator.com;port=3306;user=x8yn3upy_wgu;password=mk[Z-(m&M,dK;database=x8yn3upy_wguscheduler";
            MySqlConnection mysqlConnection = new MySqlConnection(connectionString);

            try
            {
                mysqlConnection.Open();
                App.db = mysqlConnection;
                App.db.Close();
            }
            catch (Exception err)
            {
                MessageBox.Show(App.GetLocalizedValue<string>("DatabaseConnectionFailed") + ":\n" + err.Message, App.GetLocalizedValue<string>("Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                App.Current.Shutdown();
            }
        }

        public static T GetLocalizedValue<T>(string key)
        {
            return LocExtension.GetLocalizedValue<T>(Assembly.GetCallingAssembly().GetName().Name + ":Resources:" + key);
        }
    }
}

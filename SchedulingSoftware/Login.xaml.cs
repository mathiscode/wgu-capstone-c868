using MySql.Data.MySqlClient;
using SchedulingSoftware.Classes;
using System;
using System.Collections;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace SchedulingSoftware
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }


        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // The project's sample database uses plaintext passwords. I did not choose this. :|
            // In production, we would use hashed and salted passwords
            App.db.Open();
            string sql = $"SELECT * FROM user WHERE userName=@userName AND password=@password LIMIT 1";
            MySqlCommand command = new MySqlCommand(sql, App.db);
            command.Parameters.AddWithValue("@userName", Username.Text);
            command.Parameters.AddWithValue("@password", Password.Password);
            MySqlDataReader userReader = command.ExecuteReader();

            if (!userReader.HasRows)
            {
                string errorMessage = App.GetLocalizedValue<string>("LoginIncorrect");
                TextBlock errorLabel = (TextBlock)this.FindName("errorLabel");
                errorLabel.Text = errorMessage;
                App.db.Close();
                //File.AppendAllLines("logins.txt", new[] { $"FAILED Login : {Username.Text} at {DateTime.Now.ToUniversalTime()} UTC" });
                return;
            }

            userReader.Read();

            User user = new User(userReader);
            App.db.Close();
            App.me = user;

            //File.AppendAllLines("logins.txt", new[] { $"Login : {user.name} (ID: {user.id}) logged in successfully at {DateTime.Now.ToUniversalTime()} UTC" });

            MainWindow mainWindow = new MainWindow();
            App.Current.MainWindow = mainWindow;
            mainWindow.Show();
            this.Close();
        }
    }
}

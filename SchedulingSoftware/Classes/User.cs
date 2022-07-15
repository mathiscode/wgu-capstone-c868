using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace SchedulingSoftware.Classes
{
    public class User
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool active { get; set; }
        public DateTime createDate { get; set; }
        public DateTime lastUpdate { get; set; }
        public string createdBy { get; set; }
        public string lastUpdateBy { get; set; }

        public User(MySqlDataReader userReader)
        {
            this.id = (int)userReader[0];
            this.name = (string)userReader[1];
            this.active = Convert.ToBoolean(userReader[3]);
            this.createDate = ((DateTime)userReader[4]).ToLocalTime();
            this.createdBy = (string)userReader[5];
            this.lastUpdate = ((DateTime)userReader[6]).ToLocalTime();
            this.lastUpdateBy = (string)userReader[7];
        }

        public static List<User> all()
        {
            App.db.Open();

            string sql = $"SELECT * FROM user ORDER BY userName ASC";
            MySqlCommand command = new MySqlCommand(sql, App.db);
            MySqlDataReader userReader = command.ExecuteReader();

            List<User> users = new List<User>();

            while (userReader.Read())
            {
                users.Add(new User(userReader));
            }

            App.db.Close();

            return users;
        }
    }
}

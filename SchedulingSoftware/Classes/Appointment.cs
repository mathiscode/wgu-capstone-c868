using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace SchedulingSoftware.Classes
{
    public class Appointment : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propName));
            }
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public int id { get; set; }
        public int customerId { get; set; }
        public string customerName { get; set; }
        public int userId { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string location { get; set; }
        public string contact { get; set; }
        public string type { get; set; }
        public string url { get; set; }
        public string createdBy { get; set; }
        public string lastUpdateBy { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public DateTime createDate { get; set; }
        public DateTime lastUpdate { get; set; }

        public Appointment (MySqlDataReader appointmentReader)
        {
            this.id = (int)appointmentReader[0];
            this.customerId = (int)appointmentReader[1];
            this.userId = (int)appointmentReader[2];
            this.title = (string)appointmentReader[3];
            this.description = (string)appointmentReader[4];
            this.location = (string)appointmentReader[5];
            this.contact = (string)appointmentReader[6];
            this.type = (string)appointmentReader[7];
            this.url = (string)appointmentReader[8];
            this.start = ((DateTime)appointmentReader[9]).ToLocalTime();
            this.end = ((DateTime)appointmentReader[10]).ToLocalTime();
            this.createDate = ((DateTime)appointmentReader[11]).ToLocalTime();
            this.createdBy = (string)appointmentReader[12];
            this.lastUpdate = ((DateTime)appointmentReader[13]).ToLocalTime();
            this.lastUpdateBy = (string)appointmentReader[14];
            this.customerName = (string)appointmentReader[15];
        }

        public Appointment(Hashtable appointmentReader)
        {
            if (App.db is null) return;

            this.title = (string)appointmentReader["title"];
            this.customerId = (int)appointmentReader["customerId"];
            this.customerName = (string)appointmentReader["customerName"];
            this.userId = App.me.id;
            this.description = (string)appointmentReader["description"];
            this.location = (string)appointmentReader["location"];
            this.contact = (string)appointmentReader["contact"];
            this.type = (string)appointmentReader["type"];
            this.url = (string)appointmentReader["url"];
            this.start = (DateTime)appointmentReader["start"];
            this.end = (DateTime)appointmentReader["end"];
            this.createDate = DateTime.Now;
            this.createdBy = App.me.name;
            this.lastUpdateBy = App.me.name;

            App.db.Open();

            // It wasn't specified in the rubric, but I'm assuming that appointments can overlap between users, but not for an individual user
            string appointmentExistsSql = @"SELECT appointmentId FROM appointment WHERE userId=@userId AND (end >= @start AND start <= @end)";
            MySqlCommand appointmentExistsCommand = new MySqlCommand(appointmentExistsSql, App.db);
            appointmentExistsCommand.Parameters.AddWithValue("userId", App.me.id);
            appointmentExistsCommand.Parameters.AddWithValue("start", this.start.ToUniversalTime());
            appointmentExistsCommand.Parameters.AddWithValue("end", this.end.ToUniversalTime());
            MySqlDataReader appointmentExistsReader = appointmentExistsCommand.ExecuteReader();

            if (appointmentExistsReader.HasRows) throw new Exception(App.GetLocalizedValue<string>("OverlappingAppointment"));
            App.db.Close();

            App.db.Open();

            string newAppointmentSql = @"INSERT INTO appointment
                (customerId, userId, title, description, location, contact, type, url, start, end, createDate, createdBy, lastUpdateBy) 
                VALUES (@customerId, @userId, @title, @description, @location, @contact, @type, @url, @start, @end, @createDate, @username, @username)";
            MySqlCommand newAppointmentCommand = new MySqlCommand(newAppointmentSql, App.db);
            newAppointmentCommand.Parameters.AddWithValue("customerId", this.customerId);
            newAppointmentCommand.Parameters.AddWithValue("userId", this.userId);
            newAppointmentCommand.Parameters.AddWithValue("title", this.title);
            newAppointmentCommand.Parameters.AddWithValue("description", this.description);
            newAppointmentCommand.Parameters.AddWithValue("location", this.location);
            newAppointmentCommand.Parameters.AddWithValue("contact", this.contact);
            newAppointmentCommand.Parameters.AddWithValue("type", this.type);
            newAppointmentCommand.Parameters.AddWithValue("url", this.url);
            newAppointmentCommand.Parameters.AddWithValue("start", this.start.ToUniversalTime());
            newAppointmentCommand.Parameters.AddWithValue("end", this.end.ToUniversalTime());
            newAppointmentCommand.Parameters.AddWithValue("createDate", this.createDate.ToUniversalTime());
            newAppointmentCommand.Parameters.AddWithValue("username", App.me.name);
            newAppointmentCommand.ExecuteNonQuery();
            this.id = Convert.ToInt32(newAppointmentCommand.LastInsertedId);

            App.db.Close();
        }

        public void delete()
        {
            App.db.Open();

            string deleteAppointmentSql = @"DELETE FROM appointment WHERE appointmentId = @appointmentId";
            MySqlCommand deleteAppointmentCommand = new MySqlCommand(deleteAppointmentSql, App.db);
            deleteAppointmentCommand.Parameters.AddWithValue("appointmentId", this.id);
            deleteAppointmentCommand.ExecuteNonQuery();
            ((MainWindow)Application.Current.MainWindow).Appointments.Remove(this);

            App.db.Close();
        }

        public void update()
        {
            App.db.Open();

            string updateAppointmentSql = @"UPDATE appointment SET customerId=@customerId, userId=@userId, title=@title, description=@description, location=@location, contact=@contact, type=@type, url=@url, start=@start, end=@end, lastUpdateBy=@username WHERE appointmentId=@appointmentId";
            MySqlCommand updateAppointmentCommand = new MySqlCommand(updateAppointmentSql, App.db);
            updateAppointmentCommand.Parameters.AddWithValue("customerId", this.customerId);
            updateAppointmentCommand.Parameters.AddWithValue("userId", this.userId);
            updateAppointmentCommand.Parameters.AddWithValue("title", this.title);
            updateAppointmentCommand.Parameters.AddWithValue("description", this.description);
            updateAppointmentCommand.Parameters.AddWithValue("location", this.location);
            updateAppointmentCommand.Parameters.AddWithValue("contact", this.contact);
            updateAppointmentCommand.Parameters.AddWithValue("type", this.type);
            updateAppointmentCommand.Parameters.AddWithValue("url", this.url);
            updateAppointmentCommand.Parameters.AddWithValue("start", this.start.ToUniversalTime());
            updateAppointmentCommand.Parameters.AddWithValue("end", this.end.ToUniversalTime());
            updateAppointmentCommand.Parameters.AddWithValue("username", App.me.name);
            updateAppointmentCommand.Parameters.AddWithValue("appointmentId", this.id);
            updateAppointmentCommand.ExecuteNonQuery();

            App.db.Close();
        }
    }
}

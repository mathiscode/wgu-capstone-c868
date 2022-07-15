using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SchedulingSoftware.Classes
{
    public class Appointments : ObservableCollection<Appointment>
    {
        public Appointments()
        {
            if (App.db is null) return;
            App.db.Open();

            string sql = $"SELECT appointment.*, customer.customerName FROM appointment " +
                $"LEFT JOIN customer ON appointment.customerId=customer.customerId WHERE userId=@userId ORDER BY start ASC";
            MySqlCommand command = new MySqlCommand(sql, App.db);
            command.Parameters.AddWithValue("@userId", App.me.id);
            MySqlDataReader appointmentReader = command.ExecuteReader();

            this.Clear();

            while (appointmentReader.Read())
            {
                Appointment appt = new Appointment(appointmentReader);
                this.Add(appt);
            }

            App.db.Close();
        }

        public List<Appointment> findUpcoming(double minutes)
        {
            DateTime cutoff = DateTime.Now.AddMinutes(minutes);
            List<Appointment> customerList = this.ToList();
            List<Appointment> upcoming = customerList.Where(appt => appt.start > DateTime.Now && appt.start <= cutoff).ToList();
            return upcoming;
        }
    }
}

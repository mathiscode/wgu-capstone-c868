using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;

namespace SchedulingSoftware.Classes
{
    public class Customers : ObservableCollection<Customer>
    {
        public Customers()
        {
            if (App.db is null) return;
            App.db.Open();

            string sql = @"SELECT * FROM customer
                            LEFT JOIN address ON address.addressId = customer.addressId
                            INNER JOIN city ON address.cityId = city.cityId
                            INNER JOIN country ON city.countryId = country.countryId
                            ORDER BY customer.lastUpdate DESC";

            MySqlCommand command = new MySqlCommand(sql, App.db);
            MySqlDataReader customerReader = command.ExecuteReader();

            this.Clear();

            while (customerReader.Read())
            {
                Customer cust = new Customer(customerReader);
                this.Add(cust);
            }

            App.db.Close();
        }
    }
}

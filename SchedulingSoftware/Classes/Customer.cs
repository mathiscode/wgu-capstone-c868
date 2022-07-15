using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace SchedulingSoftware.Classes
{
    public class Customer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                MessageBox.Show(PropertyChanged.ToString());
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
        public string name { get; set; }
        public int addressId { get; set; }
        public string address { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string postalCode { get; set; }
        public string phone { get; set; }
        public string country { get; set; }
        public bool active { get; set; }
        public string createdBy { get; set; }
        public string lastUpdateBy { get; set; }
        public DateTime createDate { get; set; }
        public DateTime lastUpdate { get; set; }

        public Customer(MySqlDataReader customerReader)
        {
            this.id = (int)customerReader[0];
            this.name = (string)customerReader[1];
            this.addressId = (int)customerReader[2];
            this.active = (bool)customerReader[3];
            this.createDate = ((DateTime)customerReader[4]).ToLocalTime();
            this.createdBy = (string)customerReader[5];
            this.lastUpdate = ((DateTime)customerReader[6]).ToLocalTime();
            this.lastUpdateBy = (string)customerReader[7];
            this.address = (string)customerReader[9];
            this.address2 = (string)customerReader[10];
            this.postalCode = (string)customerReader[12];
            this.phone = (string)customerReader[13];
            this.city = (string)customerReader[19];
            this.country = (string)customerReader[26];
        }

        public Customer(Hashtable customerReader)
        {
            if (App.db is null) return;

            this.name = (string)customerReader["name"];
            this.address = (string)customerReader["address"];
            this.address2 = (string)customerReader["address2"];
            this.postalCode = (string)customerReader["postalCode"];
            this.phone = (string)customerReader["phone"];
            this.city = (string)customerReader["city"];
            this.country = (string)customerReader["country"];

            if (this.name == string.Empty) throw new Exception(App.GetLocalizedValue<string>("ErrorInvalidName"));
            if (this.address == string.Empty) throw new Exception(App.GetLocalizedValue<string>("ErrorInvalidAddress"));
            if (this.city == string.Empty) throw new Exception(App.GetLocalizedValue<string>("ErrorInvalidCity"));
            if (this.postalCode == string.Empty) throw new Exception(App.GetLocalizedValue<string>("ErrorInvalidPostalCode"));
            if (this.country == string.Empty) throw new Exception(App.GetLocalizedValue<string>("ErrorInvalidCountry"));
            if (this.phone == string.Empty) throw new Exception(App.GetLocalizedValue<string>("ErrorInvalidPhone"));

            this.addressId = addressHandler();

            // Insert new customer
            this.active = true;
            this.createDate = DateTime.Now;
            this.lastUpdate = DateTime.Now;

            App.db.Open();

            string newCustomerSql = @"INSERT INTO customer (customerName, addressId, active, createDate, createdBy, lastUpdateBy) VALUES (@customerName, @addressId, @active, @createDate, @username, @username)";
            MySqlCommand newCustomerCommand = new MySqlCommand(newCustomerSql, App.db);
            newCustomerCommand.Parameters.AddWithValue("customerName", this.name);
            newCustomerCommand.Parameters.AddWithValue("addressId", this.addressId);
            newCustomerCommand.Parameters.AddWithValue("active", this.active);
            newCustomerCommand.Parameters.AddWithValue("createDate", this.createDate.ToUniversalTime());
            newCustomerCommand.Parameters.AddWithValue("username", App.me.name);
            newCustomerCommand.ExecuteNonQuery();
            this.id = Convert.ToInt32(newCustomerCommand.LastInsertedId);

            App.db.Close();
        }

        private int addressHandler(bool update = false)
        {
            App.db.Open();
            int addressId = 0;

            string sql = @"SELECT * FROM address WHERE address = @address AND postalCode = @postalCode";

            MySqlCommand command = new MySqlCommand(sql, App.db);
            command.Parameters.AddWithValue("@address", this.address);
            command.Parameters.AddWithValue("@postalCode", this.postalCode);
            MySqlDataReader addressReader = command.ExecuteReader();

            if (!addressReader.HasRows) // Address does not exist, create it
            {
                addressReader.Close();

                // Process country
                string countrySql = @"SELECT countryId FROM country WHERE country = @country LIMIT 1";
                MySqlCommand countryCommand = new MySqlCommand(countrySql, App.db);
                countryCommand.Parameters.AddWithValue("country", this.country);
                MySqlDataReader countryReader = countryCommand.ExecuteReader();
                countryReader.Read();

                int countryId = 0;

                if (!countryReader.HasRows)
                {
                    // Country does not exist, create it
                    countryReader.Close();
                    string newCountrySql = @"INSERT INTO country (country, createDate, createdBy, lastUpdateBy) VALUES (@country, @createDate, @username, @username)";
                    MySqlCommand newCountryCommand = new MySqlCommand(newCountrySql, App.db);
                    newCountryCommand.Parameters.AddWithValue("country", this.country);
                    newCountryCommand.Parameters.AddWithValue("createDate", (DateTime.Now).ToUniversalTime());
                    newCountryCommand.Parameters.AddWithValue("username", App.me.name);
                    newCountryCommand.ExecuteNonQuery();
                    countryId = Convert.ToInt32(newCountryCommand.LastInsertedId);
                }
                else
                {
                    // Country exists
                    countryId = (int)countryReader[0];
                    countryReader.Close();
                }

                // Process city
                string citySql = @"SELECT cityId FROM city WHERE city = @city LIMIT 1";
                MySqlCommand cityCommand = new MySqlCommand(citySql, App.db);
                cityCommand.Parameters.AddWithValue("city", this.city);
                MySqlDataReader cityReader = cityCommand.ExecuteReader();
                cityReader.Read();

                int cityId = 0;

                if (!cityReader.HasRows)
                {
                    // City does not exist, create it
                    cityReader.Close();
                    string newCitySql = @"INSERT INTO city (city, countryId, createDate, createdBy, lastUpdateBy) VALUES (@city, @countryId, @createDate, @username, @username)";
                    MySqlCommand newCityCommand = new MySqlCommand(newCitySql, App.db);
                    newCityCommand.Parameters.AddWithValue("city", this.city);
                    newCityCommand.Parameters.AddWithValue("countryId", countryId);
                    newCityCommand.Parameters.AddWithValue("createDate", (DateTime.Now).ToUniversalTime());
                    newCityCommand.Parameters.AddWithValue("username", App.me.name);
                    newCityCommand.ExecuteNonQuery();
                    cityId = Convert.ToInt32(newCityCommand.LastInsertedId);
                }
                else
                {
                    // Country exists
                    cityId = (int)cityReader[0];
                    cityReader.Close();
                }

                // Insert address
                string newAddressSql = @"INSERT INTO address (address, address2, cityId, postalCode, phone, createDate, createdBy, lastUpdateBy) VALUES (@address, @address2, @cityId, @postalCode, @phone, @createDate, @username, @username)";
                MySqlCommand newAddressCommand = new MySqlCommand(newAddressSql, App.db);
                newAddressCommand.Parameters.AddWithValue("address", this.address);
                newAddressCommand.Parameters.AddWithValue("address2", this.address2);
                newAddressCommand.Parameters.AddWithValue("cityId", cityId);
                newAddressCommand.Parameters.AddWithValue("postalCode", this.postalCode);
                newAddressCommand.Parameters.AddWithValue("phone", this.phone);
                newAddressCommand.Parameters.AddWithValue("createDate", (DateTime.Now).ToUniversalTime());
                newAddressCommand.Parameters.AddWithValue("username", App.me.name);
                newAddressCommand.ExecuteNonQuery();
                addressId = Convert.ToInt32(newAddressCommand.LastInsertedId);
            }
            else // Address exists, use it
            {
                addressReader.Read();
                addressId = (int)addressReader[0];
            }

            addressReader.Close();

            if (update)
            {
                string updateAddressSql = @"UPDATE address SET phone=@phone WHERE addressId=@addressId";
                MySqlCommand updateAddressCommand = new MySqlCommand(updateAddressSql, App.db);
                updateAddressCommand.Parameters.AddWithValue("phone", this.phone);
                updateAddressCommand.Parameters.AddWithValue("addressId", addressId);
                updateAddressCommand.ExecuteNonQuery();
            }

            App.db.Close();
            return addressId;
        }

        public void delete()
        {
            App.db.Open();
            string deleteCustomerSql = @"DELETE FROM customer WHERE customerId = @customerId";
            MySqlCommand deleteCustomerCommand = new MySqlCommand(deleteCustomerSql, App.db);
            deleteCustomerCommand.Parameters.AddWithValue("customerId", this.id);
            deleteCustomerCommand.ExecuteNonQuery();
            ((MainWindow)Application.Current.MainWindow).Customers.Remove(this);
            App.db.Close();
        }

        public void update()
        {
            if (this.name == string.Empty) throw new Exception(App.GetLocalizedValue<string>("ErrorInvalidName"));
            if (this.address == string.Empty) throw new Exception(App.GetLocalizedValue<string>("ErrorInvalidAddress"));
            if (this.city == string.Empty) throw new Exception(App.GetLocalizedValue<string>("ErrorInvalidCity"));
            if (this.postalCode == string.Empty) throw new Exception(App.GetLocalizedValue<string>("ErrorInvalidPostalCode"));
            if (this.country == string.Empty) throw new Exception(App.GetLocalizedValue<string>("ErrorInvalidCountry"));
            if (this.phone == string.Empty) throw new Exception(App.GetLocalizedValue<string>("ErrorInvalidPhone"));

            this.addressId = addressHandler(true);

            App.db.Open();
            string updateCustomerSql = @"UPDATE customer SET customerName=@customerName, addressId=@addressId, active=@active, lastUpdateBy=@username WHERE customerId = @customerId";
            MySqlCommand updateCustomerCommand = new MySqlCommand(updateCustomerSql, App.db);
            updateCustomerCommand.Parameters.AddWithValue("customerId", this.id);
            updateCustomerCommand.Parameters.AddWithValue("customerName", this.name);
            updateCustomerCommand.Parameters.AddWithValue("addressId", this.addressId);
            updateCustomerCommand.Parameters.AddWithValue("active", this.active);
            updateCustomerCommand.Parameters.AddWithValue("username", App.me.name);
            updateCustomerCommand.ExecuteNonQuery();
            App.db.Close();
        }
    }
}

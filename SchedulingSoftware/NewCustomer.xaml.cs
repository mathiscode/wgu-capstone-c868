using SchedulingSoftware.Classes;
using System;
using System.Collections;
using System.Windows;

namespace SchedulingSoftware
{
    /// <summary>
    /// Interaction logic for NewCustomer.xaml
    /// </summary>
    public partial class NewCustomer : Window
    {
        public NewCustomer()
        {
            InitializeComponent();
        }

        private void NewCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            Hashtable details = new Hashtable();

            try
            {
                details["name"] = NewCustomerName.Text;
                details["address"] = NewCustomerAddressLine1.Text;
                details["address2"] = NewCustomerAddressLine2.Text;
                details["city"] = NewCustomerCity.Text;
                details["postalCode"] = NewCustomerPostalCode.Text;
                details["country"] = NewCustomerCountry.Text;
                details["phone"] = NewCustomerPhone.Text;

                Customer newCustomer = new Customer(details);
                ((MainWindow)Application.Current.MainWindow).Customers.Add(newCustomer);
                this.Close();
            }
            catch (Exception err)
            {
                App.db.Close();
                MessageBox.Show(err.Message, App.GetLocalizedValue<string>("Error"));
            }
        }
    }
}

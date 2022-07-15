using SchedulingSoftware.Classes;
using System;
using System.Windows;

namespace SchedulingSoftware
{
    /// <summary>
    /// Interaction logic for EditCustomer.xaml
    /// </summary>
    public partial class EditCustomer : Window
    {
        public EditCustomer(Customer selectedCustomer)
        {
            this.DataContext = selectedCustomer;
            InitializeComponent();
        }

        private void EditCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ((Customer)this.DataContext).update();
                ((MainWindow)App.Current.MainWindow).CustomersView.Refresh();
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

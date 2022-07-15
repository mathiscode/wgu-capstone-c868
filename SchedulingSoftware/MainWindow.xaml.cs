using SchedulingSoftware.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace SchedulingSoftware
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Appointments Appointments { get; set; }
        public Customers Customers { get; set; }
        private Customer SelectedCustomer { get; set; }
        private Appointment SelectedAppointment { get; set; }
        public ICollectionView AppointmentsView { get; set; }
        public ICollectionView CustomersView { get; set; }
        public string CustomerSearchQuery { get; set; }

        public MainWindow()
        {
            Appointments = new Appointments();
            Customers = new Customers();

            foreach (Appointment appt in Appointments)
            {
                List<Customer> matchedCustomer = new List<Customer>(Customers);
                IEnumerable<Customer> match = matchedCustomer.Where(cust => cust.id == appt.customerId); // This lambda expression finds the customer record associated with each appointment
                Customer customer = match.First();
                appt.customerName = customer.name;
            }

            AppointmentsView = CollectionViewSource.GetDefaultView(Appointments);
            CustomersView = CollectionViewSource.GetDefaultView(Customers);

            AppointmentsView.SortDescriptions.Add(new SortDescription(nameof(Appointment.start), ListSortDirection.Ascending));
            CustomersView.SortDescriptions.Add(new SortDescription(nameof(Customer.name), ListSortDirection.Ascending));

            InitializeComponent();
            _ = checkReminders(new CancellationToken());
        }

        private void WebPageClick(object sender, RoutedEventArgs e)
        {
            Hyperlink link = e.OriginalSource as Hyperlink;
            Process.Start(link.NavigateUri.AbsoluteUri);
        }

        public async Task checkReminders(CancellationToken cancellationToken)
        {
            while (true)
            {
                List<Appointment> upcoming = Appointments.findUpcoming(15);
                if (upcoming.Count > 0)
                {
                    upcoming.ForEach(appt =>
                    {
                        MessageBox.Show(
                            $"{App.GetLocalizedValue<string>("YouHaveUpcomingAppointment")} {appt.customerName} {App.GetLocalizedValue<string>("StartingAt")} {appt.start}", 
                            App.GetLocalizedValue<string>("UpcomingAppointment"),
                            MessageBoxButton.OK, MessageBoxImage.Exclamation
                        );
                    });
                }

                await Task.Delay(900000, cancellationToken);
                if (cancellationToken.IsCancellationRequested) break;
            }
        }

        private void Customer_Filter(object sender, FilterEventArgs e)
        {
            Customer customer = e.Item as Customer;
            e.Accepted = customer != null && customer.name.Contains(CustomerSearchQueryTextbox.Text);
        }

        private void AppointmentDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            AppointmentDataGrid.ItemsSource = AppointmentsView;
        }

        private void CustomerDataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            CustomerDataGrid.ItemsSource = CustomersView;
        }

        private void AppointmentDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedAppointment = (Appointment)AppointmentDataGrid.SelectedItem;
        }

        private void CustomerDataGrid_SelectionChanged(object sender, RoutedEventArgs e)
        {
            SelectedCustomer = (Customer)CustomerDataGrid.SelectedItem;
        }

        private void DeleteCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCustomer is null)
            {
                MessageBox.Show(App.GetLocalizedValue<string>("SelectCustomer"), App.GetLocalizedValue<string>("Customer"), MessageBoxButton.OK, MessageBoxImage.Error);
            } 
            else
            {
                MessageBoxResult confirm = MessageBox.Show(
                    App.GetLocalizedValue<string>("ConfirmDeleteCustomer"), 
                    App.GetLocalizedValue<string>("ConfirmAction"), 
                    MessageBoxButton.YesNo, MessageBoxImage.Question
                );

                if (confirm == MessageBoxResult.Yes)
                {
                    SelectedCustomer.delete();
                }
            }
        }

        private void NewCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            NewCustomer newCustomer = new NewCustomer();
            newCustomer.Show();
        }

        private void EditCustomerButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedCustomer is null)
            {
                MessageBox.Show(App.GetLocalizedValue<string>("SelectCustomer"), App.GetLocalizedValue<string>("Customer"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                EditCustomer editCustomer = new EditCustomer(SelectedCustomer);
                editCustomer.Show();
            }
        }

        private void NewAppointmentButton_Click(object sender, RoutedEventArgs e)
        {
            NewAppointment newAppointment = new NewAppointment();
            newAppointment.Show();
        }

        private void EditAppointmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedAppointment is null)
            {
                MessageBox.Show(App.GetLocalizedValue<string>("SelectAppointment"), App.GetLocalizedValue<string>("Appointment"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                EditAppointment editAppointment = new EditAppointment(SelectedAppointment);
                editAppointment.Show();
            }
        }

        private void DeleteAppointmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedAppointment is null)
            {
                MessageBox.Show(App.GetLocalizedValue<string>("SelectAppointment"), App.GetLocalizedValue<string>("Appointment"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBoxResult confirm = MessageBox.Show(
                    App.GetLocalizedValue<string>("ConfirmDeleteAppointment"), 
                    App.GetLocalizedValue<string>("ConfirmAction"), 
                    MessageBoxButton.YesNo, MessageBoxImage.Question
                );

                if (confirm == MessageBoxResult.Yes)
                {
                    SelectedAppointment.delete();
                }
            }
        }

        private void AllAppointmentsView_Click(object sender, RoutedEventArgs e)
        {
            AppointmentsView.Filter = null;
        }

        private void WeeklyView_Click(object sender, RoutedEventArgs e)
        {
            // This lambda function filters appointments up to a week away
            AppointmentsView.Filter = (appt) =>
            {
                Appointment appointment = appt as Appointment;
                DateTime oneWeek = (DateTime.Now).AddDays(7);
                return (appointment.start >= DateTime.Now && appointment.start < oneWeek);
            };
        }

        private void MonthlyView_Click(object sender, RoutedEventArgs e)
        {
            // This lambda function filters appointments up to a month away
            AppointmentsView.Filter = (appt) =>
            {
                Appointment appointment = appt as Appointment;
                DateTime oneMonth = (DateTime.Now).AddMonths(1);
                return (appointment.start >= DateTime.Now && appointment.start < oneMonth);
            };
        }

        private void FutureAppointmentsView_Click(object sender, RoutedEventArgs e)
        {
            // This lambda function finds appointments that are in the future
            AppointmentsView.Filter = (appt) =>
            {
                Appointment appointment = appt as Appointment;
                return appointment.start >= DateTime.Now;
            };
        }

        private void CustomerSearchQueryTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // This lambda function filters customers by user search query
            CustomersView.Filter = (c) =>
            {
                Customer customer = c as Customer;
                if (CustomerSearchQueryTextbox.Text == string.Empty) return true;

                int id = 0;
                int.TryParse(CustomerSearchQueryTextbox.Text, out id);

                return (
                    customer.id == id ||
                    customer.name.ToLower().Contains(CustomerSearchQueryTextbox.Text.ToLower()) ||
                    customer.phone.Contains(CustomerSearchQueryTextbox.Text) ||
                    customer.address.ToLower().Contains(CustomerSearchQueryTextbox.Text.ToLower()) ||
                    customer.address2.ToLower().Contains(CustomerSearchQueryTextbox.Text.ToLower()) ||
                    customer.city.ToLower().Contains(CustomerSearchQueryTextbox.Text.ToLower()) ||
                    customer.postalCode.ToLower().Contains(CustomerSearchQueryTextbox.Text.ToLower()) ||
                    customer.country.ToLower().Contains(CustomerSearchQueryTextbox.Text.ToLower())
                );
            };
        }

        private void ReportAppointmentTypesButton_Click(object sender, RoutedEventArgs e)
        {
            Report_AppointmentTypes report = new Report_AppointmentTypes(Appointments);
            report.Show();
        }

        private void ReportConsultantSchedulesButton_Click(object sender, RoutedEventArgs e)
        {
            Report_ConsultantSchedule report = new Report_ConsultantSchedule(Appointments);
            report.Show();
        }

        private void ReportConsultantHoursButton_Click(object sender, RoutedEventArgs e)
        {
            Report_ConsultantHours report = new Report_ConsultantHours(Appointments);
            report.Show();
        }
    }
}

using SchedulingSoftware.Classes;
using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace SchedulingSoftware
{
    /// <summary>
    /// Interaction logic for NewAppointment.xaml
    /// </summary>
    public partial class NewAppointment : Window
    {
        public NewAppointment()
        {
            InitializeComponent();
            foreach (var i in Enumerable.Range(0, 24))
            {
                ComboBoxItem startHourItem = new ComboBoxItem();
                ComboBoxItem endHourItem = new ComboBoxItem();
                string hourText = i.ToString() + ":";
                if (i < 10) hourText = "0" + i + ":";
                startHourItem.Content = hourText;
                endHourItem.Content = hourText;
                StartHour.Items.Add(startHourItem);
                EndHour.Items.Add(endHourItem);
            }

            foreach (var i in Enumerable.Range(0, 60))
            {
                ComboBoxItem startMinuteItem = new ComboBoxItem();
                ComboBoxItem endMinuteItem = new ComboBoxItem();
                string minuteText = i.ToString();
                if (i < 10) minuteText = "0" + i;
                startMinuteItem.Content = minuteText;
                endMinuteItem.Content = minuteText;
                StartMinute.Items.Add(startMinuteItem);
                EndMinute.Items.Add(endMinuteItem);
            }

            IOrderedEnumerable<Customer> sortedCustomers = ((MainWindow)Application.Current.MainWindow).Customers.OrderBy(c => c.name); // This lambda expression sorts customers by name by default

            foreach (Customer customer in sortedCustomers)
            {
                NewAppointmentCustomer.Items.Add($"{customer.name} ({customer.id})");
            }
        }

        private void NewAppointmentButton_Click(object sender, RoutedEventArgs e)
        {
            Hashtable details = new Hashtable();
            DateTime startDate;
            DateTime endDate;

            try
            {
                if (StartHour.SelectedItem is null) throw new Exception(App.GetLocalizedValue<string>("InvalidStartTime"));
                if (StartMinute.SelectedItem is null) throw new Exception(App.GetLocalizedValue<string>("InvalidStartTime"));

                try
                {
                    startDate = DateTime.Parse(StartDatepicker.Text);
                    string startHour = ((ComboBoxItem)StartHour.SelectedItem).Content.ToString();
                    string startMinute = ((ComboBoxItem)StartMinute.SelectedItem).Content.ToString();

                    startDate = startDate.AddHours(Convert.ToDouble(startHour.Replace(':', '\0')));
                    startDate = startDate.AddMinutes(Convert.ToDouble(startMinute.Replace(':', '\0')));

                    // Just following the rubric, but ideally we should just remove non-business hours from the timepicker
                    if (startDate.Hour < 9 || startDate.Hour > 17)
                    {
                        throw new Exception(App.GetLocalizedValue<string>("AppointmentOutsideBusinessHours"));
                    }
                }
                catch (Exception err)
                {
                    throw new Exception(err.Message);
                }

                if (EndHour.SelectedItem is null) throw new Exception(App.GetLocalizedValue<string>("InvalidEndTime"));
                if (EndMinute.SelectedItem is null) throw new Exception(App.GetLocalizedValue<string>("InvalidEndTime"));

                try
                {
                    endDate = DateTime.Parse(EndDatepicker.Text);
                    string endHour = ((ComboBoxItem)EndHour.SelectedItem).Content.ToString();
                    string endMinute = ((ComboBoxItem)EndMinute.SelectedItem).Content.ToString();

                    endDate = endDate.AddHours(Convert.ToDouble(endHour.Replace(':', '\0')));
                    endDate = endDate.AddMinutes(Convert.ToDouble(endMinute.Replace(':', '\0')));

                    if (endDate.Hour < 9 || endDate.Hour > 17)
                    {
                        throw new Exception(App.GetLocalizedValue<string>("AppointmentOutsideBusinessHours"));
                    }
                }
                catch (Exception err)
                {
                    throw new Exception(err.Message);
                }
                
                details["start"] = startDate;
                details["end"] = endDate;

                Regex customerIdRegex = new Regex(@"\((\d+)\)");
                Match customerIdMatch = customerIdRegex.Match(NewAppointmentCustomer.Text);
                Regex customerNameRegex = new Regex(@"(^.+) \(");
                Match customerNameMatch = customerNameRegex.Match(NewAppointmentCustomer.Text);
                if (!customerIdMatch.Success || !customerNameMatch.Success) throw new Exception(App.GetLocalizedValue<string>("SelectCustomer"));
                details["customerId"] = Convert.ToInt32(customerIdMatch.Groups[1].Value);
                details["customerName"] = customerNameMatch.Groups[1].Value;

                details["title"] = NewAppointmentTitle.Text;
                details["location"] = NewAppointmentLocation.Text;
                details["contact"] = NewAppointmentContact.Text;
                details["type"] = NewAppointmentType.Text;
                details["url"] = NewAppointmentUrl.Text;
                details["description"] = NewAppointmentDescription.Text;

                if ((string)details["type"] == string.Empty) throw new Exception(App.GetLocalizedValue<string>("SpecifyAppointmentType"));

                Appointment newAppointment = new Appointment(details);
                ((MainWindow)Application.Current.MainWindow).Appointments.Add(newAppointment);
                this.Close();
            }
            catch (Exception err)
            {
                App.db.Close();
                MessageBox.Show(err.Message, App.GetLocalizedValue<string>("Error"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

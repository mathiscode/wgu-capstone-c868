using SchedulingSoftware.Classes;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace SchedulingSoftware
{
    /// <summary>
    /// Interaction logic for EditAppointment.xaml
    /// </summary>
    public partial class EditAppointment : Window
    {
        public EditAppointment(Appointment selectedAppointment)
        {
            this.DataContext = selectedAppointment;

            InitializeComponent();

            foreach (var i in Enumerable.Range(0, 24))
            {
                string hourText = i.ToString() + ":";
                if (i < 10) hourText = "0" + i + ":";
                EditStartHour.Items.Add(hourText);
                EditEndHour.Items.Add(hourText);
            }

            foreach (var i in Enumerable.Range(0, 60))
            {
                string minuteText = i.ToString();
                if (i < 10) minuteText = "0" + i;
                EditStartMinute.Items.Add(minuteText);
                EditEndMinute.Items.Add(minuteText);
            }

            IOrderedEnumerable<Customer> sortedCustomers = ((MainWindow)Application.Current.MainWindow).Customers.OrderBy(c => c.name);

            foreach (Customer customer in sortedCustomers)
            {
                EditAppointmentCustomer.Items.Add($"{customer.name} ({customer.id})");
            }

            EditStartDatepicker.SelectedDate = selectedAppointment.start;
            EditEndDatepicker.SelectedDate = selectedAppointment.end;

            string startHour = selectedAppointment.start.Hour + ":";
            if (selectedAppointment.start.Hour < 10) startHour = "0" + selectedAppointment.start.Hour + ":";
            string startMinute = selectedAppointment.start.Minute.ToString();
            if (selectedAppointment.start.Minute < 10) startMinute = "0" + selectedAppointment.start.Minute;

            string endHour = selectedAppointment.end.Hour + ":";
            if (selectedAppointment.end.Hour < 10) endHour = "0" + selectedAppointment.end.Hour + ":";
            string endMinute = selectedAppointment.end.Minute.ToString();
            if (selectedAppointment.end.Minute < 10) endMinute = "0" + selectedAppointment.end.Minute;

            EditStartHour.Text = startHour;
            EditStartMinute.Text = startMinute;

            EditEndHour.Text = endHour;
            EditEndMinute.Text = endMinute;

            EditAppointmentCustomer.Text = $"{selectedAppointment.customerName} ({selectedAppointment.customerId})";
        }

        private void EditAppointmentButton_Click(object sender, RoutedEventArgs e)
        {
            DateTime startDate;
            DateTime endDate;

            try
            {
                Appointment appt = (Appointment)DataContext;
                if (EditStartHour.SelectedItem is null) throw new Exception(App.GetLocalizedValue<string>("InvalidStartTime"));
                if (EditStartMinute.SelectedItem is null) throw new Exception(App.GetLocalizedValue<string>("InvalidStartTime"));

                try
                {
                    startDate = DateTime.Parse(EditStartDatepicker.Text);
                    string startHour = (string)EditStartHour.SelectedItem;
                    string startMinute = (string)EditStartMinute.SelectedItem;

                    startDate = startDate.AddHours(Convert.ToDouble(startHour.Replace(':', '\0')));
                    startDate = startDate.AddMinutes(Convert.ToDouble(startMinute.Replace(':', '\0')));

                    // Just following the rubric, but ideally we should just remove non-business hours from the timepicker
                    if (startDate.Hour < 9 || startDate.Hour > 17)
                    {
                        throw new Exception(App.GetLocalizedValue<string>("AppointmentOutsideBusinessHours"));
                    }
                }
                catch
                {
                    throw new Exception(App.GetLocalizedValue<string>("InvalidStartDate"));
                }

                if (EditEndHour.SelectedItem is null) throw new Exception(App.GetLocalizedValue<string>("InvalidEndTime"));
                if (EditEndMinute.SelectedItem is null) throw new Exception(App.GetLocalizedValue<string>("InvalidEndTime"));

                try
                {
                    endDate = DateTime.Parse(EditEndDatepicker.Text);
                    string endHour = (string)EditEndHour.SelectedItem;
                    string endMinute = (string)EditEndMinute.SelectedItem;

                    endDate = endDate.AddHours(Convert.ToDouble(endHour.Replace(':', '\0')));
                    endDate = endDate.AddMinutes(Convert.ToDouble(endMinute.Replace(':', '\0')));

                    if (endDate.Hour < 9 || endDate.Hour > 17)
                    {
                        throw new Exception(App.GetLocalizedValue<string>("AppointmentOutsideBusinessHours"));
                    }
                }
                catch
                {
                    throw new Exception(App.GetLocalizedValue<string>("InvalidEndDate"));
                }

                Regex customerIdRegex = new Regex(@"\((\d+)\)");
                Match customerIdMatch = customerIdRegex.Match(EditAppointmentCustomer.Text);
                Regex customerNameRegex = new Regex(@"(^.+)\(");
                Match customerNameMatch = customerNameRegex.Match(EditAppointmentCustomer.Text);
                if (!customerIdMatch.Success || !customerNameMatch.Success) throw new Exception(App.GetLocalizedValue<string>("SelectCustomer"));

                appt.customerId = Convert.ToInt32(customerIdMatch.Groups[1].Value);
                appt.customerName = customerNameMatch.Groups[1].Value;
                appt.start = startDate;
                appt.end = endDate;
                appt.update();

                ((MainWindow)App.Current.MainWindow).AppointmentsView.Refresh();
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

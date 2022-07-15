using SchedulingSoftware.Classes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SchedulingSoftware
{
    /// <summary>
    /// Interaction logic for Report_ConsultantHours.xaml
    /// </summary>
    public partial class Report_ConsultantHours : Window
    {
        public Report_ConsultantHours(Appointments appointments)
        {
            InitializeComponent();

            DataGridTextColumn dateCol = new DataGridTextColumn();
            dateCol.Header = App.GetLocalizedValue<string>("Date");
            dateCol.Binding = new Binding("[date]");
            AppointmentDataGrid.Columns.Add(dateCol);

            List<User> users = User.all();

            foreach (User user in users)
            {
                DataGridTextColumn col = new DataGridTextColumn();
                col.Header = user.name;
                col.Binding = new Binding($"[{user.name}]");
                col.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                AppointmentDataGrid.Columns.Add(col);
            }

            List<Hashtable> data = new List<Hashtable>();
            List<Appointment> appts = appointments.Where(a => a.start.Month == DateTime.Now.Month && a.start.Year == DateTime.Now.Year).OrderBy(a => a.start).ToList();

            foreach (User user in users)
            {
                Hashtable result = new Hashtable();
                List<Appointment> userAppts = appts.Where(a => a.userId == user.id).ToList();

                DateTime current = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                int days = DateTime.DaysInMonth(current.Year, current.Month);
                for (int day = 1; day <= days; day++)
                {
                    List<Appointment> dayAppts = userAppts.Where(a => a.start.Year == current.Year && a.start.Month == current.Month && a.start.Day == day).ToList();
                    if (dayAppts.Count > 0)
                    {
                        double hours = 0;
                        Hashtable hourData = new Hashtable();
                        hourData.Add("date", (new DateTime(current.Year, current.Month, day)).ToShortDateString());

                        foreach (Appointment dayAppt in dayAppts)
                        {
                            TimeSpan span = dayAppt.end - dayAppt.start;
                            hours += span.TotalHours;
                        }

                        hourData.Add(user.name, hours);

                        data.Add(hourData);
                    }
                }
            }

            AppointmentDataGrid.ItemsSource = data;
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if ((bool)printDialog.ShowDialog().GetValueOrDefault())
            {
                Size pageSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
                AppointmentDataGrid.Measure(pageSize);
                AppointmentDataGrid.Arrange(new Rect(5, 5, pageSize.Width, pageSize.Height));
                printDialog.PrintVisual(AppointmentDataGrid, Title);
            }
        }
    }
}

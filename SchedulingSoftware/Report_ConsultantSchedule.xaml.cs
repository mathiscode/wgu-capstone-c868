using SchedulingSoftware.Classes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SchedulingSoftware
{
    /// <summary>
    /// Interaction logic for Report_ConsultantSchedule.xaml
    /// </summary>
    public partial class Report_ConsultantSchedule : Window
    {
        public Report_ConsultantSchedule(Appointments appointments)
        {
            InitializeComponent();

            DataGridTextColumn startCol = new DataGridTextColumn();
            startCol.Header = App.GetLocalizedValue<string>("Start");
            startCol.Binding = new Binding("[start]");
            AppointmentDataGrid.Columns.Add(startCol);

            DataGridTextColumn endCol = new DataGridTextColumn();
            endCol.Header = App.GetLocalizedValue<string>("End");
            endCol.Binding = new Binding("[end]");
            AppointmentDataGrid.Columns.Add(endCol);

            DataGridTextColumn userCol = new DataGridTextColumn();
            userCol.Header = App.GetLocalizedValue<string>("Username");
            userCol.Binding = new Binding("[user]");
            AppointmentDataGrid.Columns.Add(userCol);

            DataGridTextColumn customerCol = new DataGridTextColumn();
            customerCol.Header = App.GetLocalizedValue<string>("Customer");
            customerCol.Binding = new Binding("[customer]");
            AppointmentDataGrid.Columns.Add(customerCol);

            DataGridTextColumn typeCol = new DataGridTextColumn();
            typeCol.Header = App.GetLocalizedValue<string>("Type");
            typeCol.Binding = new Binding("[type]");
            typeCol.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            AppointmentDataGrid.Columns.Add(typeCol);

            List<Hashtable> data = new List<Hashtable>();
            List<Appointment> appts = appointments.OrderBy(a => a.start).ToList();
            List<User> users = User.all();

            foreach (Appointment a in appts)
            {
                Hashtable rowData = new Hashtable();
                rowData["start"] = a.start;
                rowData["end"] = a.end;
                rowData["user"] = users.Where(u => u.id == a.userId).First().name;
                rowData["customer"] = a.customerName;
                rowData["type"] = a.type;

                data.Add(rowData);
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

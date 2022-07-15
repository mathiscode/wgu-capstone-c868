using SchedulingSoftware.Classes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SchedulingSoftware
{
    /// <summary>
    /// Interaction logic for Report_AppointmentTypes.xaml
    /// </summary>
    public partial class Report_AppointmentTypes : Window
    {
        public Report_AppointmentTypes(Appointments appointments)
        {
            InitializeComponent();

            HashSet<string> types = new HashSet<string>();
            foreach (Appointment a in appointments)
            {
                types.Add(a.type);
            }

            DataGridTextColumn monthCol = new DataGridTextColumn();
            monthCol.Header = App.GetLocalizedValue<string>("Month");
            monthCol.Binding = new Binding("[month]");
            AppointmentDataGrid.Columns.Add(monthCol);

            foreach (string type in types)
            {
                DataGridTextColumn col = new DataGridTextColumn();
                col.Header = type;
                col.Binding = new Binding($"[{type}]");
                col.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                AppointmentDataGrid.Columns.Add(col);
            }

            List<Hashtable> data = new List<Hashtable>();

            for (int i = 0; i < 12; i++)
            {
                Hashtable typeCount = new Hashtable();
                List<Appointment> appts = appointments.Where(a => a.start.Month == i + 1).ToList();

                foreach (Appointment a in appts)
                {
                    if (typeCount[a.type] is null) typeCount[a.type] = 0;
                    typeCount[a.type] = (int)typeCount[a.type] + 1;
                }

                Hashtable rowData = new Hashtable();
                DateTime dt = new DateTime(1, i + 1, 1);

                rowData["month"] = dt.ToString("MMMM", CultureInfo.InvariantCulture);
                foreach (string type in typeCount.Keys)
                {
                    rowData[type] = typeCount[type];
                }

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

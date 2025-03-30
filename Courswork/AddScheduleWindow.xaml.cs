using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Courswork
{
    public partial class AddScheduleWindow : Window
    {
        private int _methodistID;

        private MPID4855073Entities db = new MPID4855073Entities();

        public AddScheduleWindow(int methodistID)
        {
            InitializeComponent();
            _methodistID = methodistID;
            LoadData();
        }

        private void LoadData()
        {
            // Загрузка игр
            GameComboBox.ItemsSource = db.Games.ToList();
            GameComboBox.DisplayMemberPath = "Title";
            GameComboBox.SelectedValuePath = "GameID";

            // Загрузка учащихся
            StudentListBox.ItemsSource = db.Children.ToList();
            StudentListBox.DisplayMemberPath = "Name";
            StudentListBox.SelectedValuePath = "ChildID";

            // Загрузка локаций
            LocationComboBox.ItemsSource = db.Locations.ToList();
            LocationComboBox.DisplayMemberPath = "Location";
            LocationComboBox.SelectedValuePath = "LocationID";
        }

        private void AddSchedule_Click(object sender, RoutedEventArgs e)
        {
            if (GameComboBox.SelectedItem == null || StudentListBox.SelectedItems.Count == 0 ||
                LocationComboBox.SelectedItem == null || string.IsNullOrWhiteSpace(TimeTextBox.Text) ||
                ScheduleDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            int gameID = (int)GameComboBox.SelectedValue;
            int locationID = (int)LocationComboBox.SelectedValue;
            DateTime date = ScheduleDatePicker.SelectedDate.Value;

            if (!TimeSpan.TryParse(TimeTextBox.Text, out TimeSpan time))
            {
                MessageBox.Show("Некорректный формат времени. Введите в формате ЧЧ:ММ.");
                return;
            }

            foreach (var selectedChild in StudentListBox.SelectedItems)
            {
                int childID = ((Children)selectedChild).ChildID;

                Schedule newSchedule = new Schedule
                {
                    GameID = gameID,
                    ChildID = childID,
                    LocationID = locationID,
                    Date = date,
                    Time = time,
                    MethodistID = _methodistID
                };

                db.Schedule.Add(newSchedule);
            }

            db.SaveChanges();
            MessageBox.Show("Занятие успешно добавлено!");
            this.Close();
        }
    }
}

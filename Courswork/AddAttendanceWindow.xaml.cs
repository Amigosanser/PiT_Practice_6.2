using System;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Courswork
{
    public partial class AddAttendanceWindow : Window
    {
        private MPID4855073Entities db = new MPID4855073Entities();

        public AddAttendanceWindow()
        {
            InitializeComponent();
            LoadSchedules();
        }

        // Загрузка занятий (без дублирования)
        private void LoadSchedules()
        {
            var schedules = db.Schedule
                .Join(db.Games, s => s.GameID, g => g.GameID, (s, g) => new
                {
                    s.ScheduleID,
                    g.Title,
                    s.Date,
                    s.Time
                })
                .GroupBy(s => new { s.ScheduleID, s.Title, s.Date, s.Time })
                .Select(s => new
                {
                    s.Key.ScheduleID,
                    DisplayText = s.Key.Title + " (" + s.Key.Date + " " + s.Key.Time + ")"
                })
                .ToList();

            ScheduleComboBox.ItemsSource = schedules;
            ScheduleComboBox.DisplayMemberPath = "DisplayText";
            ScheduleComboBox.SelectedValuePath = "ScheduleID";
        }


        // Загрузка учеников при выборе занятия
        private void ScheduleComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ScheduleComboBox.SelectedValue is int selectedScheduleID)
            {
                var students = db.Schedule
                    .Where(s => s.ScheduleID == selectedScheduleID)
                    .Join(db.Children, s => s.ChildID, c => c.ChildID, (s, c) => new
                    {
                        c.ChildID,
                        c.Name,
                        c.Age
                    })
                    .ToList();

                StudentsListBox.ItemsSource = students;
                StudentsListBox.DisplayMemberPath = "Name";
                StudentsListBox.SelectedValuePath = "ChildID";
            }
        }

        // Сохранение данных о посещаемости
        private void SaveAttendanceButton_Click(object sender, RoutedEventArgs e)
        {
            if (ScheduleComboBox.SelectedValue is int selectedScheduleID &&
                StatusComboBox.SelectedItem is ComboBoxItem selectedStatus &&
                StudentsListBox.SelectedItems.Count > 0)
            {
                string status = selectedStatus.Content.ToString();
                string remarks = RemarksTextBox.Text;

                List<Attendance> attendanceRecords = new List<Attendance>();

                foreach (var selectedItem in StudentsListBox.SelectedItems)
                {
                    dynamic student = selectedItem;
                    attendanceRecords.Add(new Attendance
                    {
                        ScheduleID = selectedScheduleID,
                        ChildID = student.ChildID,
                        Status = status,
                        Remarks = remarks
                    });
                }

                db.Attendance.AddRange(attendanceRecords);
                db.SaveChanges();

                MessageBox.Show("Посещаемость успешно сохранена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите занятие, ученика и статус.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Courswork
{
    public partial class AdmScheduleWindow : Window
    {
        private MPID4855073Entities _db = new MPID4855073Entities();
        private Schedule _selectedSchedule;
        private int _currentMethodistId;

        public AdmScheduleWindow()
        {
            InitializeComponent();
            LoadMethodists();
            SetEditMode(false);
            SetControlsEnabled(false);
        }

        private void LoadMethodists()
        {
            cmbMethodists.ItemsSource = _db.Methodists.ToList();
            cmbMethodists.SelectedIndex = -1;
            btnLoad.IsEnabled = false;
        }

        private void LoadData()
        {
            try
            {
                if (_currentMethodistId == 0)
                {
                    dgSchedule.ItemsSource = null;
                    return;
                }

                var scheduleData = _db.Schedule
                    .Where(s => s.MethodistID == _currentMethodistId)
                    .Join(_db.Games,
                        s => s.GameID,
                        g => g.GameID,
                        (s, g) => new { Schedule = s, GameTitle = g.Title })
                    .Join(_db.Children,
                        sg => sg.Schedule.ChildID,
                        c => c.ChildID,
                        (sg, c) => new { sg.Schedule, sg.GameTitle, ChildName = c.Name })
                    .Join(_db.Locations,
                        sgc => sgc.Schedule.LocationID,
                        l => l.LocationID,
                        (sgc, l) => new {
                            sgc.Schedule,
                            sgc.GameTitle,
                            sgc.ChildName,
                            LocationName = l.Location
                        })
                    .Join(_db.Methodists,
                        sgcl => sgcl.Schedule.MethodistID,
                        m => m.MethodistID,
                        (sgcl, m) => new
                        {
                            ScheduleID = sgcl.Schedule.ScheduleID,
                            Date = sgcl.Schedule.Date,
                            Time = sgcl.Schedule.Time,
                            GameTitle = sgcl.GameTitle,
                            ChildName = sgcl.ChildName,
                            LocationName = sgcl.LocationName,
                            MethodistName = m.Name,
                            GameID = sgcl.Schedule.GameID,
                            ChildID = sgcl.Schedule.ChildID,
                            LocationID = sgcl.Schedule.LocationID,
                            MethodistID = sgcl.Schedule.MethodistID
                        })
                    .OrderBy(x => x.Date)
                    .ThenBy(x => x.Time)
                    .ToList();

                dgSchedule.ItemsSource = scheduleData;

                cmbGames.ItemsSource = _db.Games.ToList();
                cmbChildren.ItemsSource = _db.Children.ToList();
                cmbLocations.ItemsSource = _db.Locations.ToList();

                dgSchedule.UnselectAll();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetControlsEnabled(bool enabled)
        {
            btnNew.IsEnabled = enabled;
            btnDelete.IsEnabled = enabled;
            btnRefresh.IsEnabled = enabled;
            dgSchedule.IsEnabled = enabled;
        }

        private void SetEditMode(bool editMode)
        {
            btnSave.Content = editMode ? "Сохранить" : "Добавить";
            btnCancel.Visibility = editMode ? Visibility.Visible : Visibility.Collapsed;
            if (!editMode) ClearForm();
        }

        private void ClearForm()
        {
            dpDate.SelectedDate = null;
            txtTime.Text = "";
            cmbGames.SelectedIndex = -1;
            cmbChildren.SelectedIndex = -1;
            cmbLocations.SelectedIndex = -1;
            _selectedSchedule = null;
        }

        private void cmbMethodists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnLoad.IsEnabled = cmbMethodists.SelectedValue != null;
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            _currentMethodistId = (int)cmbMethodists.SelectedValue;
            LoadData();
            SetControlsEnabled(true);
        }

        private void dgSchedule_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dynamic selected = dgSchedule.SelectedItem;
            if (selected != null)
            {
                dpDate.SelectedDate = selected.Date;
                txtTime.Text = selected.Time.ToString(@"hh\:mm");
                cmbGames.SelectedValue = selected.GameID;
                cmbChildren.SelectedValue = selected.ChildID;
                cmbLocations.SelectedValue = selected.LocationID;
                SetEditMode(true);
            }
            else
            {
                SetEditMode(false);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (dpDate.SelectedDate == null ||
                string.IsNullOrWhiteSpace(txtTime.Text) ||
                cmbGames.SelectedValue == null ||
                cmbChildren.SelectedValue == null ||
                cmbLocations.SelectedValue == null)
            {
                MessageBox.Show("Заполните все поля", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                TimeSpan time;
                if (!TimeSpan.TryParse(txtTime.Text, out time))
                {
                    MessageBox.Show("Введите время в формате ЧЧ:ММ (например, 14:30)",
                                  "Неверный формат времени",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_selectedSchedule == null) // Добавление
                {
                    var newSchedule = new Schedule
                    {
                        Date = dpDate.SelectedDate.Value,
                        Time = time,
                        GameID = (int)cmbGames.SelectedValue,
                        ChildID = (int)cmbChildren.SelectedValue,
                        LocationID = (int)cmbLocations.SelectedValue,
                        MethodistID = _currentMethodistId
                    };
                    _db.Schedule.Add(newSchedule);
                }
                else // Редактирование
                {
                    _selectedSchedule.Date = dpDate.SelectedDate.Value;
                    _selectedSchedule.Time = time;
                    _selectedSchedule.GameID = (int)cmbGames.SelectedValue;
                    _selectedSchedule.ChildID = (int)cmbChildren.SelectedValue;
                    _selectedSchedule.LocationID = (int)cmbLocations.SelectedValue;
                }

                _db.SaveChanges();
                LoadData();
                MessageBox.Show("Данные сохранены успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedSchedule == null)
            {
                MessageBox.Show("Выберите запись для удаления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить запись от {_selectedSchedule.Date.ToShortDateString()}?",
                                       "Подтверждение удаления",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _db.Schedule.Remove(_selectedSchedule);
                    _db.SaveChanges();
                    LoadData();
                    MessageBox.Show("Запись удалена успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            SetEditMode(false);
            dgSchedule.UnselectAll();
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddScheduleWindow(_currentMethodistId);
            if (addWindow.ShowDialog() == true)
            {
                LoadData();
            }
        }

       

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }
    }
}
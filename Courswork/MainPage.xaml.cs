using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media;
using System;

namespace Courswork
{
    public partial class MainPage : Page
    {
        private Users _currentUser;

        public MainPage(Users user)
        {
            InitializeComponent();
            _currentUser = user;

            // Устанавливаем имя пользователя в UI
            //txtUsername.Text = _currentUser.FullName;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_currentUser.RoleID == 1) // Показываем только администратору
            {
                AdmUsersButton.Visibility = Visibility.Visible;
                AdmScheduleButton.Visibility = Visibility.Visible;
                AddScheduleButton.Visibility = Visibility.Visible;
                AddGameButton.Visibility = Visibility.Visible;
                AddGameCategoriesButton.Visibility = Visibility.Visible;
                AddAttendanceButton.Visibility = Visibility.Visible;
            }
            else if (_currentUser.RoleID == 2) // Показываем методисту
            {
                AddScheduleButton.Visibility = Visibility.Visible;
                AddGameButton.Visibility = Visibility.Visible;
                AddGameCategoriesButton.Visibility = Visibility.Visible;
                AddAttendanceButton.Visibility = Visibility.Visible;
            }
            else
            {
                AddScheduleButton.Visibility = Visibility.Hidden;
                AddGameButton.Visibility = Visibility.Hidden;
                AddGameCategoriesButton.Visibility = Visibility.Hidden;
                AddAttendanceButton.Visibility = Visibility.Hidden;
            }
        }



        // Обработчик кнопки меню пользователя
        private void btnUserMenu_Click(object sender, RoutedEventArgs e)
        {
            pnlUserMenu.Visibility = pnlUserMenu.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        // Обработчик кнопки "Смена пароля"
        private void ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            //NavigationService?.Navigate(new ChangePasswordPage());
        }

        // Обработчик кнопки "Выйти"
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new LoginPage());
        }

        // Обработчик вкладки "Расписание"
        private void OpenSchedulePage(object sender, RoutedEventArgs e)
        {
            using (var db = new MPID4855073Entities())
            {
                List<dynamic> scheduleData = new List<dynamic>(); // Данные для отображения
                string windowTitle = "Расписание"; // Название окна

                if (_currentUser.RoleID == 3) // Если текущий пользователь - учащийся
                {
                    // Находим ChildID по UserID
                    var child = db.Children
                                  .FirstOrDefault(c => c.UserID == _currentUser.UserID);

                    if (child == null)
                    {
                        MessageBox.Show("Учащийся не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Загружаем расписание для учащегося
                    scheduleData = (from s in db.Schedule
                                    join g in db.Games on s.GameID equals g.GameID
                                    join l in db.Locations on s.LocationID equals l.LocationID
                                    where s.ChildID == child.ChildID
                                    select new
                                    {
                                        Дата = s.Date,
                                        Время = s.Time,
                                        Игра = g.Title,
                                        Локация = l.Location
                                    }).ToList<dynamic>();

                    windowTitle = "Мое расписание";
                }
                else if (_currentUser.RoleID == 4) // Если текущий пользователь - родитель
                {
                    // Находим ParentID по UserID
                    var parent = db.Parents
                                   .FirstOrDefault(p => p.UserID == _currentUser.UserID);

                    if (parent == null)
                    {
                        MessageBox.Show("Родитель не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Получаем список детей родителя
                    var children = db.Children
                                     .Where(c => c.ParentID == parent.ParentID)
                                     .Select(c => new { c.ChildID, Имя = c.Name })
                                     .ToList();

                    if (children.Count == 0)
                    {
                        MessageBox.Show("У вас нет зарегистрированных детей.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Окно выбора ребенка
                    Window selectChildWindow = new Window
                    {
                        Title = "Выберите ребенка",
                        Width = 300,
                        Height = 200,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                    };

                    ListBox childrenList = new ListBox
                    {
                        ItemsSource = children,
                        DisplayMemberPath = "Имя",
                        SelectedValuePath = "ChildID",
                        Margin = new Thickness(10)
                    };

                    Button selectButton = new Button
                    {
                        Content = "Выбрать",
                        Margin = new Thickness(10),
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    int selectedChildID = -1; // ID выбранного ребенка

                    selectButton.Click += (s, args) =>
                    {
                        if (childrenList.SelectedValue != null)
                        {
                            selectedChildID = (int)childrenList.SelectedValue;
                            selectChildWindow.Close();
                        }
                        else
                        {
                            MessageBox.Show("Пожалуйста, выберите ребенка.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    };

                    StackPanel panel = new StackPanel();
                    panel.Children.Add(childrenList);
                    panel.Children.Add(selectButton);

                    selectChildWindow.Content = panel;
                    selectChildWindow.ShowDialog(); // Ждём выбора

                    if (selectedChildID == -1) return; // Если не выбрали, выходим

                    // Загружаем расписание для выбранного ребенка
                    scheduleData = (from s in db.Schedule
                                    join g in db.Games on s.GameID equals g.GameID
                                    join l in db.Locations on s.LocationID equals l.LocationID
                                    where s.ChildID == selectedChildID
                                    select new
                                    {
                                        Дата = s.Date,
                                        Время = s.Time,
                                        Игра = g.Title,
                                        Локация = l.Location
                                    }).ToList<dynamic>();

                    windowTitle = "Расписание ребенка";
                }
                else if (_currentUser.RoleID == 2) // Если текущий пользователь - методист
                {
                    // Находим MethodistID по UserID
                    var methodist = db.Methodists
                                      .FirstOrDefault(m => m.UserID == _currentUser.UserID);

                    if (methodist == null)
                    {
                        MessageBox.Show("Методист не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Загружаем расписание для всех игр методиста
                    scheduleData = (from s in db.Schedule
                                    join g in db.Games on s.GameID equals g.GameID
                                    join l in db.Locations on s.LocationID equals l.LocationID
                                    join c in db.Children on s.ChildID equals c.ChildID
                                    where g.MethodistID == methodist.MethodistID
                                    select new
                                    {
                                        Дата = s.Date,
                                        Время = s.Time,
                                        Игра = g.Title,
                                        Локация = l.Location,
                                        Участник = c.Name
                                    }).ToList<dynamic>();

                    windowTitle = "Расписание методиста";
                }
                else if (_currentUser.RoleID == 1) // Если текущий пользователь - администратор
                {
                    scheduleData = (from s in db.Schedule
                                    join g in db.Games on s.GameID equals g.GameID
                                    join l in db.Locations on s.LocationID equals l.LocationID
                                    join c in db.Children on s.ChildID equals c.ChildID
                                    join u in db.Users on c.UserID equals u.UserID
                                    join p in db.Parents on c.ParentID equals p.ParentID into parentJoin
                                    from parent in parentJoin.DefaultIfEmpty()
                                    join meth in db.Methodists on g.MethodistID equals meth.MethodistID into methJoin
                                    from methodist in methJoin.DefaultIfEmpty()
                                    select new
                                    {
                                        Дата = s.Date,
                                        Время = s.Time,
                                        Игра = g.Title,
                                        Локация = l.Location,
                                        Участник = c.Name,
                                        Логин_участника = u.FullName,
                                        Родитель = parent != null ? parent.Name : "Не указан",
                                        Методист = methodist != null ? methodist.Name : "Не указан",
                                        ID_Расписания = s.ScheduleID // Можно добавить ID для возможного редактирования
                                    }).ToList<dynamic>();

                    windowTitle = "Полное расписание (администратор)";
                }
                else
                {
                    MessageBox.Show("Доступ запрещен.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Создаем окно для отображения расписания
                Window scheduleWindow = new Window
                {
                    Title = windowTitle,
                    Width = 600,
                    Height = 400,
                    Content = new DataGrid
                    {
                        ItemsSource = scheduleData,
                        AutoGenerateColumns = true,
                        IsReadOnly = true
                    }
                };

                scheduleWindow.Show();
            }
        }

        // Обработчик вкладки "Посещаемость"
        private void OpenAttendancePage(object sender, RoutedEventArgs e)
        {
            using (var db = new MPID4855073Entities())
            {
                int selectedChildID = -1;
                List<dynamic> attendanceData = null;
                string windowTitle = "Посещаемость";

                if (_currentUser.RoleID == 4) // Родитель
                {
                    var parent = db.Parents
                                 .FirstOrDefault(p => p.UserID == _currentUser.UserID);

                    if (parent == null)
                    {
                        MessageBox.Show("Родитель не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var children = db.Children
                                   .Where(c => c.ParentID == parent.ParentID)
                                   .Select(c => new { c.ChildID, Имя = c.Name })
                                   .ToList();

                    if (children.Count == 0)
                    {
                        MessageBox.Show("У вас нет зарегистрированных детей.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    Window selectChildWindow = new Window
                    {
                        Title = "Выберите ребенка",
                        Width = 300,
                        Height = 200,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                    };

                    ListBox childrenList = new ListBox
                    {
                        ItemsSource = children,
                        DisplayMemberPath = "Имя",
                        SelectedValuePath = "ChildID",
                        Margin = new Thickness(10)
                    };

                    Button selectButton = new Button
                    {
                        Content = "Выбрать",
                        Margin = new Thickness(10),
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    selectButton.Click += (s, args) =>
                    {
                        if (childrenList.SelectedValue != null)
                        {
                            selectedChildID = (int)childrenList.SelectedValue;
                            selectChildWindow.Close();
                        }
                        else
                        {
                            MessageBox.Show("Пожалуйста, выберите ребенка.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    };

                    StackPanel panel = new StackPanel();
                    panel.Children.Add(childrenList);
                    panel.Children.Add(selectButton);

                    selectChildWindow.Content = panel;
                    selectChildWindow.ShowDialog();

                    if (selectedChildID == -1) return;

                    attendanceData = (from a in db.Attendance
                                      join s in db.Schedule on a.ScheduleID equals s.ScheduleID
                                      join g in db.Games on s.GameID equals g.GameID
                                      join l in db.Locations on s.LocationID equals l.LocationID
                                      where a.ChildID == selectedChildID
                                      select new
                                      {
                                          Дата = s.Date,
                                          Время = s.Time,
                                          Игра = g.Title,
                                          Локация = l.Location,
                                          Статус = a.Status
                                      }).ToList<dynamic>();

                    windowTitle = "Посещаемость ребенка";
                }
                else if (_currentUser.RoleID == 3) // Учащийся
                {
                    var child = db.Children
                                .FirstOrDefault(c => c.UserID == _currentUser.UserID);

                    if (child == null)
                    {
                        MessageBox.Show("Учащийся не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    selectedChildID = child.ChildID;

                    attendanceData = (from a in db.Attendance
                                      join s in db.Schedule on a.ScheduleID equals s.ScheduleID
                                      join g in db.Games on s.GameID equals g.GameID
                                      join l in db.Locations on s.LocationID equals l.LocationID
                                      where a.ChildID == selectedChildID
                                      select new
                                      {
                                          Дата = s.Date,
                                          Время = s.Time,
                                          Игра = g.Title,
                                          Локация = l.Location,
                                          Статус = a.Status
                                      }).ToList<dynamic>();

                    windowTitle = "Моя посещаемость";
                }
                else if (_currentUser.RoleID == 2 || _currentUser.RoleID == 1) // Методист / Администратор
                {
                    Window modeWindow = new Window
                    {
                        Title = "Режим просмотра посещаемости",
                        Width = 300,
                        Height = 150,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                    };

                    StackPanel modePanel = new StackPanel { Margin = new Thickness(10) };

                    RadioButton allAttendanceRadio = new RadioButton
                    {
                        Content = "Вся посещаемость",
                        IsChecked = true,
                        Margin = new Thickness(5)
                    };

                    RadioButton childAttendanceRadio = new RadioButton
                    {
                        Content = "Посещаемость ребенка",
                        Margin = new Thickness(5)
                    };

                    Button confirmButton = new Button
                    {
                        Content = "Продолжить",
                        Margin = new Thickness(5),
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    bool viewAll = true;

                    allAttendanceRadio.Checked += (s, args) => viewAll = true;
                    childAttendanceRadio.Checked += (s, args) => viewAll = false;

                    confirmButton.Click += (s, args) => modeWindow.Close();

                    modePanel.Children.Add(allAttendanceRadio);
                    modePanel.Children.Add(childAttendanceRadio);
                    modePanel.Children.Add(confirmButton);
                    modeWindow.Content = modePanel;
                    modeWindow.ShowDialog();

                    if (viewAll)
                    {
                        attendanceData = (from a in db.Attendance
                                          join s in db.Schedule on a.ScheduleID equals s.ScheduleID
                                          join g in db.Games on s.GameID equals g.GameID
                                          join l in db.Locations on s.LocationID equals l.LocationID
                                          join c in db.Children on a.ChildID equals c.ChildID
                                          join u in db.Users on c.UserID equals u.UserID
                                          select new
                                          {
                                              Дата = s.Date,
                                              Время = s.Time,
                                              Игра = g.Title,
                                              Локация = l.Location,
                                              Участник = c.Name,
                                              Логин = u.FullName,
                                              Статус = a.Status
                                          }).ToList<dynamic>();

                        windowTitle = "Вся посещаемость (администратор)";
                    }
                    else
                    {
                        var children = db.Children
                                       .Select(c => new { c.ChildID, Имя = c.Name })
                                       .ToList();

                        Window selectChildWindow = new Window
                        {
                            Title = "Выберите ребенка",
                            Width = 300,
                            Height = 300,
                            WindowStartupLocation = WindowStartupLocation.CenterScreen
                        };

                        ListBox childrenList = new ListBox
                        {
                            ItemsSource = children,
                            DisplayMemberPath = "Имя",
                            SelectedValuePath = "ChildID",
                            Margin = new Thickness(10)
                        };

                        Button selectButton = new Button
                        {
                            Content = "Выбрать",
                            Margin = new Thickness(10),
                            HorizontalAlignment = HorizontalAlignment.Center
                        };

                        selectButton.Click += (s, args) =>
                        {
                            if (childrenList.SelectedValue != null)
                            {
                                selectedChildID = (int)childrenList.SelectedValue;
                                selectChildWindow.Close();
                            }
                            else
                            {
                                MessageBox.Show("Пожалуйста, выберите ребенка.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        };

                        StackPanel panel = new StackPanel();
                        panel.Children.Add(childrenList);
                        panel.Children.Add(selectButton);

                        selectChildWindow.Content = panel;
                        selectChildWindow.ShowDialog();

                        if (selectedChildID == -1) return;

                        attendanceData = (from a in db.Attendance
                                          join s in db.Schedule on a.ScheduleID equals s.ScheduleID
                                          join g in db.Games on s.GameID equals g.GameID
                                          join l in db.Locations on s.LocationID equals l.LocationID
                                          join c in db.Children on a.ChildID equals c.ChildID
                                          where a.ChildID == selectedChildID
                                          select new
                                          {
                                              Дата = s.Date,
                                              Время = s.Time,
                                              Игра = g.Title,
                                              Локация = l.Location,
                                              Статус = a.Status
                                          }).ToList<dynamic>();

                        windowTitle = $"Посещаемость ребенка (ID: {selectedChildID})";
                    }
                }
                else
                {
                    MessageBox.Show("Доступ запрещен.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (attendanceData == null || attendanceData.Count == 0)
                {
                    MessageBox.Show("Нет данных о посещаемости.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                Window attendanceWindow = new Window
                {
                    Title = windowTitle,
                    Width = 800,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Content = new DataGrid
                    {
                        ItemsSource = attendanceData,
                        AutoGenerateColumns = true,
                        IsReadOnly = true,
                        CanUserSortColumns = true,
                        CanUserResizeColumns = true
                    }
                };

                attendanceWindow.Show();
            }
        }


        // Обработчик вкладки "Оставить отзыв"
        private void OpenFeedbackPage(object sender, RoutedEventArgs e)
        {
            // Создаем окно для ввода отзыва
            Window feedbackWindow = new Window
            {
                Title = "Оставить отзыв",
                Width = 400,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            // Контейнер для элементов ввода
            StackPanel stackPanel = new StackPanel { Margin = new Thickness(10) };

            // Поле выбора игры
            ComboBox gameComboBox = new ComboBox { Margin = new Thickness(0, 5, 0, 5) };
            using (var db = new MPID4855073Entities())
            {
                gameComboBox.ItemsSource = db.Games.Select(g => new { g.GameID, g.Title }).ToList();
                gameComboBox.DisplayMemberPath = "Title";
                gameComboBox.SelectedValuePath = "ID";
            }

            // Поле для комментария
            TextBox commentBox = new TextBox
            {
                Height = 80,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Margin = new Thickness(0, 5, 0, 5)
            };

            // "Placeholder" для TextBox
            TextBlock placeholderText = new TextBlock
            {
                Text = "Введите ваш отзыв...",
                Foreground = Brushes.Gray,
                Margin = new Thickness(5, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            // Поле для оценки
            ComboBox ratingComboBox = new ComboBox
            {
                ItemsSource = new List<int> { 1, 2, 3, 4, 5 },
                SelectedIndex = 4, // По умолчанию 5 звезд
                Margin = new Thickness(0, 5, 0, 5)
            };

            // Кнопка отправки отзыва
            Button submitButton = new Button
            {
                Content = "Отправить отзыв",
                Margin = new Thickness(0, 10, 0, 0)
            };

            submitButton.Click += (s, eArgs) =>
            {
                if (gameComboBox.SelectedItem != null && !string.IsNullOrWhiteSpace(commentBox.Text))
                {
                    using (var db = new MPID4855073Entities())
                    {
                        var newFeedback = new Feedback
                        {
                            GameID = (int)gameComboBox.SelectedValue,
                            UserID = _currentUser.UserID, // Текущий пользователь
                            Comment = commentBox.Text,
                            Rating = (int)ratingComboBox.SelectedItem
                        };

                        db.Feedback.Add(newFeedback);
                        db.SaveChanges();
                    }

                    MessageBox.Show("Спасибо за ваш отзыв!", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                    feedbackWindow.Close();
                }
                else
                {
                    MessageBox.Show("Выберите игру и введите комментарий!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };

            // Добавляем элементы в окно
            stackPanel.Children.Add(new TextBlock { Text = "Выберите игру:" });
            stackPanel.Children.Add(gameComboBox);
            stackPanel.Children.Add(new TextBlock { Text = "Ваш комментарий:" });
            stackPanel.Children.Add(commentBox);
            stackPanel.Children.Add(new TextBlock { Text = "Оценка (1-5):" });
            stackPanel.Children.Add(ratingComboBox);
            stackPanel.Children.Add(submitButton);

            feedbackWindow.Content = stackPanel;
            feedbackWindow.ShowDialog();
        }

        private void AddSchedulePage(object sender, RoutedEventArgs e)
        {
            using (var db = new MPID4855073Entities())
            {
                var methodist = db.Methodists.FirstOrDefault(m => m.UserID == _currentUser.UserID);

                if (methodist != null)
                {
                    AddScheduleWindow addScheduleWindow = new AddScheduleWindow(methodist.MethodistID);
                    addScheduleWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Методист не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            
        }

        private void AddGamePage(object sender, RoutedEventArgs e)
        {

            using (var db = new MPID4855073Entities())
            {
                var methodist = db.Methodists.FirstOrDefault(m => m.UserID == _currentUser.UserID);

                if (methodist != null)
                {
                    AddGameWindow addGameWindow = new AddGameWindow(methodist.MethodistID);
                    addGameWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Методист не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }


        }

        private void AddGameCategoriesPage(object sender, RoutedEventArgs e)
        {
            AddGameCategoryWindow addCategoryWindow = new AddGameCategoryWindow();
            addCategoryWindow.ShowDialog();
        }

        private void AddAttendancePage(object sender, RoutedEventArgs e)
        {
            AddAttendanceWindow AddAttendanceWindow = new AddAttendanceWindow();
            AddAttendanceWindow.ShowDialog();
        }

        private void AdmUsersPage(object sender, RoutedEventArgs e)
        {
            AdmUsersWindow AdmUsersWindow = new AdmUsersWindow();
            AdmUsersWindow.ShowDialog();
        }

        private void AdmSchedulePage(object sender, RoutedEventArgs e)
        {
            AdmScheduleWindow AdmScheduleWindow = new AdmScheduleWindow();
            AdmScheduleWindow.ShowDialog();

        }


    }
}

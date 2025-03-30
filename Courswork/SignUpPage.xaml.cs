using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace Courswork
{
    public partial class SignUpPage : Page
    {
        public SignUpPage()
        {
            InitializeComponent();
        }

        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }


        // Обработчик выбора роли
        private void cmbRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbRole.SelectedItem is ComboBoxItem selectedItem)
            {
                string role = selectedItem.Content.ToString();

                // Показываем/скрываем нужные панели
                pnlVerificationWord.Visibility = (role == "Методист") ? Visibility.Visible : Visibility.Collapsed;
                pnlContactInfo.Visibility = (role == "Родитель") ? Visibility.Visible : Visibility.Collapsed;
                pnlStudentInfo.Visibility = (role == "Учащийся") ? Visibility.Visible : Visibility.Collapsed;

                // Если выбран "Учащийся", загружаем список родителей
                if (role == "Учащийся")
                {
                    using (var db = new MPID4855073Entities())
                    {
                        var parents = db.Parents.Select(u => u.Name).ToList();
                        cmbParent.ItemsSource = parents;
                    }
                }
            }
        }
        // Обработчик кнопки "Зарегистрироваться"
        private void SignUp_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;
            string confirmPassword = txtConfirmPassword.Password;
            string role = (cmbRole.SelectedItem as ComboBoxItem)?.Content.ToString();
            string verificationWord = txtVerificationWord.Text;

            // Проверка на пустые поля
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка совпадения паролей
            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Дополнительная проверка для роли "Методист"
            if (role == "Методист" && string.IsNullOrEmpty(verificationWord))
            {
                MessageBox.Show("Пожалуйста, введите кодовое слово.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (role == "Методист" && verificationWord != "Judge")
            {
                MessageBox.Show("Неправильное кодовое слово.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new MPID4855073Entities()) // Контекст БД
            {
                // Проверка, существует ли пользователь с таким логином
                var existingUser = db.Users.AsNoTracking().FirstOrDefault(u => u.FullName == username);
                if (existingUser != null)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Хэширование пароля перед сохранением
                string hashedPassword = GetHash(password);
                int roleid = -1;

                if (role == "Методист")
                    roleid = 2;
                if (role == "Учащийся")
                    roleid = 3;
                if (role == "Родитель")
                    roleid = 4;

                // Создание нового пользователя
                Users newUser = new Users
                {
                    FullName = username,
                    Password = hashedPassword,
                    RoleID = roleid

                };
                db.Users.Add(newUser);
                db.SaveChanges();

                if (role == "Методист")
                {
                    Methodists newMethodists = new Methodists
                    {
                        Name = username,
                        ExperienceYears = 0,
                        HourlyWage = 600,
                        UserID = newUser.UserID

                    };
                    db.Methodists.Add(newMethodists);
                    db.SaveChanges();
                }

                if (role == "Учащийся")
                {
                    if (!int.TryParse(txtAge.Text.Trim(), out int age))
                    {
                        MessageBox.Show("Некорректный возраст.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string parentName = cmbParent.SelectedItem as string;
                    var parent = db.Parents.FirstOrDefault(p => p.Name == parentName);
                    if (parent == null)
                    {
                        MessageBox.Show("Выбранный родитель не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    Children newChildren = new Children
                    {
                        Name = username,
                        Age = age,
                        ParentID = parent.ParentID,
                        UserID = newUser.UserID
                    };
                    db.Children.Add(newChildren);
                    db.SaveChanges();
                }
                if (role == "Родитель")
                {
                    string contactInfo = txtContactInfo.Text.Trim();
                    if (string.IsNullOrEmpty(contactInfo))
                    {
                        MessageBox.Show("Введите контактную информацию.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    Parents newParent = new Parents
                    {
                        Name = username,
                        ContactInfo = contactInfo,
                        UserID = newUser.UserID
                    };
                    db.Parents.Add(newParent);
                    db.SaveChanges();
                }
            }

            // Сообщение об успешной регистрации
            MessageBox.Show($"Регистрация успешна: {username}, Роль: {role}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            // Переход на страницу авторизации
            NavigationService?.Navigate(new LoginPage());
        }


        // Обработчик перехода на страницу авторизации
        private void OpenLoginPage(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new LoginPage());
        }
    }
}
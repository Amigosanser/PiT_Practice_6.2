using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace Courswork
{
    public partial class LoginPage : Page
    {
        // Путь к файлу для хранения данных
        private readonly string _credentialsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "login_credentials.txt");

        public LoginPage()
        {
            InitializeComponent();

            // Загружаем сохраненные данные при открытии страницы
            LoadCredentials();
        }

        // Загрузка сохраненных данных из файла
        private void LoadCredentials()
        {
            if (File.Exists(_credentialsFilePath))
            {
                string[] lines = File.ReadAllLines(_credentialsFilePath);
                if (lines.Length == 2)
                {
                    txtUsername.Text = lines[0];
                    txtPassword.Password = lines[1];
                    chkRememberMe.IsChecked = true; // Устанавливаем галочку
                }
            }
        }

        // Сохранение данных в файл
        private void SaveCredentials(string username, string password)
        {
            File.WriteAllLines(_credentialsFilePath, new[] { username, password });
        }

        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

        // Очистка файла
        private void ClearCredentials()
        {
            if (File.Exists(_credentialsFilePath))
            {
                File.Delete(_credentialsFilePath);
            }
        }

        // Обработчик кнопки "Войти"
        private void Login_Click(object sender, RoutedEventArgs e)
        {
             string username = txtUsername.Text;
            string password = txtPassword.Password;

            // Проверка на пустые поля
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Логика авторизации
            string _password = GetHash(password);

            using (var db = new MPID4855073Entities()) // Используем контекст БД
            {
                var user = db.Users
                    .AsNoTracking()
                    .FirstOrDefault(u => u.FullName == username && u.Password == _password);

                if (user == null)
                {
                    MessageBox.Show("Пользователь с такими данными не найден!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверяем роль пользователя и открываем соответствующее окно

                MessageBox.Show("Авторизация успешна!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Сохраняем данные, если выбрана опция "Оставаться в системе"
                if (chkRememberMe.IsChecked == true)
                {
                    SaveCredentials(username, password);
                }
                else
                {
                    ClearCredentials(); // Очищаем файл, если галочка снята
                }

                // Переход на главную страницу (замените на нужную страницу)
                NavigationService?.Navigate(new MainPage(user));

            }

        }

        // Обработчик изменения состояния галочки "Оставаться в системе"
        private void chkRememberMe_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (chkRememberMe.IsChecked == false)
            {
                ClearCredentials(); // Очищаем файл, если галочка снята
            }
        }

        // Обработчик перехода на страницу регистрации
        private void OpenRegisterPage(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new SignUpPage());
        }

        // Обработчик перехода на страницу восстановления пароля
        private void OpenForgotPasswordPage(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new ForgotPasswordPage());
        }
    }
}
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Courswork
{
    public partial class ForgotPasswordPage : Page
    {
        public ForgotPasswordPage()
        {
            InitializeComponent();
        }

        // Обработчик кнопки "Восстановить пароль"
        private void RestorePassword_Click(object sender, RoutedEventArgs e)
        {
            string emailOrLogin = txtEmailOrLogin.Text;

            // Проверка на пустое поле
            if (string.IsNullOrEmpty(emailOrLogin))
            {
                MessageBox.Show("Пожалуйста, введите логин.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Логика восстановления пароля (например, отправка email)
            MessageBox.Show($"Запрос на восстановление пароля отправлен для: {emailOrLogin}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Обработчик ссылки "Назад к авторизации"
        private void OpenLoginPage(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new LoginPage()); // Переход на страницу авторизации
        }
    }
}
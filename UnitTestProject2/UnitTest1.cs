using System;
using System.Windows;
using System.Windows.Controls;
using Courswork.Pages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject2
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        private void EntrButton_Click(object sender, RoutedEventArgs e)
        {
            Auth(TextBoxlogin.Text, PasswordBox.Password);
        }
        [TestMethod]
        public bool Auth(string login, string password)
        {
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль!");
                return false;
            }
            using (var db = new Entities())
            {
                var user = db.Users.AsNoTracking().FirstorDefault(u => u.Login == login && u.Password == password);

                if (user == null)
                {
                    MessageBox.Show("Пользователь с такими данными не найден!");
                    return false;
                }
                MessageBox.Show("Пользователь успешно найден!");
                TextBoxLogin.Clear();
                PasswordBox.Clear();
                return true;
            }
        }
        [TestMethod]
        public void TestMethod1()
        {
            var page = new AuthPage();
            Assert.IsTrue(page.Auth("test", "test"));
            Assert.IsFalse(page.Auth("user1", "12345"));
            Assert.IsFalse(page.Auth("", ""));
            Assert.IsFalse(page.Auth(" ", " "));
        }
    }
}
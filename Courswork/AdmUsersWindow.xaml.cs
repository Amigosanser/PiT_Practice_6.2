using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Courswork
{
    public partial class AdmUsersWindow : Window
    {
        private MPID4855073Entities _db = new MPID4855073Entities();
        private Users _selectedUser;

        public AdmUsersWindow()
        {
            InitializeComponent();
            LoadUsers();
            LoadRoles();
            SetEditMode(false);
        }

        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

        private void LoadUsers()
        {
            try
            {
                var usersWithRoles = _db.Users
                    .Join(_db.Roles,
                        user => user.RoleID,
                        role => role.RoleID,
                        (user, role) => new
                        {
                            user.UserID,
                            user.FullName,
                            user.Password,
                            RoleName = role.RoleName
                        })
                    .ToList();

                dgUsers.ItemsSource = usersWithRoles;
                dgUsers.UnselectAll();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}");
            }
        }

        private void LoadRoles()
        {
            cmbRoles.ItemsSource = _db.Roles.ToList();
        }

        private void SetEditMode(bool editMode)
        {
            if (editMode)
            {
                txtFormTitle.Text = "Редактирование пользователя";
                btnSave.Content = "Сохранить";
                btnCancel.Visibility = Visibility.Visible;
            }
            else
            {
                txtFormTitle.Text = "Добавление нового пользователя";
                btnSave.Content = "Добавить";
                btnCancel.Visibility = Visibility.Collapsed;
                ClearForm();
            }
        }

        private void ClearForm()
        {
            txtFullName.Text = "";
            pwdPassword.Password = "";
            cmbRoles.SelectedIndex = -1;
            _selectedUser = null;
        }

        private void dgUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dynamic selectedItem = dgUsers.SelectedItem;

            if (selectedItem != null)
            {
                _selectedUser = _db.Users.Find(selectedItem.UserID);

                if (_selectedUser != null)
                {
                    txtFullName.Text = _selectedUser.FullName;
                    pwdPassword.Password = _selectedUser.Password;
                    cmbRoles.SelectedValue = _selectedUser.RoleID;

                    SetEditMode(true);
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Введите ФИО пользователя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbRoles.SelectedValue == null)
            {
                MessageBox.Show("Выберите роль пользователя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string fullName = txtFullName.Text.Trim();
                int currentUserId = _selectedUser?.UserID ?? 0;

                // Проверяем, существует ли пользователь с таким именем (кроме текущего редактируемого)
                bool nameExists = _db.Users.Any(u =>
                    u.FullName == fullName &&
                    u.UserID != currentUserId);

                if (nameExists)
                {
                    MessageBox.Show("Пользователь с таким именем уже существует", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_selectedUser == null) // Добавление
                {
                    var newUser = new Users
                    {
                        FullName = fullName,
                        Password = GetHash(pwdPassword.Password),
                        RoleID = (int)cmbRoles.SelectedValue
                    };
                    _db.Users.Add(newUser);
                }
                else // Редактирование
                {
                    _selectedUser.FullName = fullName;
                    _selectedUser.Password = GetHash(pwdPassword.Password);
                    _selectedUser.RoleID = (int)cmbRoles.SelectedValue;
                }

                _db.SaveChanges();
                LoadUsers();
                MessageBox.Show("Данные сохранены успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для удаления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить пользователя {_selectedUser.FullName}?",
                                       "Подтверждение удаления",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _db.Users.Remove(_selectedUser);
                    _db.SaveChanges();
                    LoadUsers();
                    MessageBox.Show("Пользователь удален успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            dgUsers.UnselectAll();
            SetEditMode(false);
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }
    }
}
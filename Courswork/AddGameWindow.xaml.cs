using System;
using System.Linq;
using System.Windows;

namespace Courswork
{
    public partial class AddGameWindow : Window
    {
        private int _methodistID;

        public AddGameWindow(int methodistID)
        {
            InitializeComponent();
            _methodistID = methodistID;
            LoadCategories();
        }

        private void LoadCategories()
        {
            using (var db = new MPID4855073Entities())
            {
                var categories = db.GameCategories.ToList();
                CategoryComboBox.ItemsSource = categories;
            }
        }

        private void AddGame_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleTextBox.Text.Trim();
            string description = DescriptionTextBox.Text.Trim();
            string ageGroup = AgeGroupTextBox.Text.Trim();
            var selectedCategory = CategoryComboBox.SelectedItem as GameCategories;

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description) || string.IsNullOrEmpty(ageGroup) || selectedCategory == null)
            {
                MessageBox.Show("Заполните все поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new MPID4855073Entities())
            {
                Games newGame = new Games
                {
                    Title = title,
                    Description = description,
                    AgeGroup = ageGroup,
                    CategoryID = selectedCategory.CategoryID,
                    MethodistID = _methodistID
                };

                db.Games.Add(newGame);
                db.SaveChanges();

                MessageBox.Show("Игра успешно добавлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
        }
    }
}

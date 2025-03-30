using System;
using System.Linq;
using System.Windows;

namespace Courswork
{
    public partial class AddGameCategoryWindow : Window
    {
        public AddGameCategoryWindow()
        {
            InitializeComponent();
        }

        private void AddCategory_Click(object sender, RoutedEventArgs e)
        {
            string categoryName = CategoryNameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(categoryName))
            {
                MessageBox.Show("Введите название категории!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new MPID4855073Entities())
            {
                // Проверяем, есть ли уже такая категория
                if (db.GameCategories.Any(c => c.CategoryName == categoryName))
                {
                    MessageBox.Show("Такая категория уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Добавляем новую категорию
                GameCategories newCategory = new GameCategories { CategoryName = categoryName };
                db.GameCategories.Add(newCategory);
                db.SaveChanges();

                MessageBox.Show("Категория успешно добавлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
        }
    }
}

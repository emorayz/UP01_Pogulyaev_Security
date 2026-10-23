using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UP01_Pogulyaev_Security.Model;

namespace UP01_Pogulyaev_Security.Pages
{
    /// <summary>
    /// Таблица категорий угроз ИБ: просмотр, добавление, редактирование, удаление.
    /// </summary>
    public partial class CategoriesTabPage : Page
    {
        public CategoriesTabPage()
        {
            InitializeComponent();
            DataGridCategory.ItemsSource = Entities.GetContext().ThreatCategories.ToList();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Title = "Таблица категорий угроз";
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                Entities.GetContext().ChangeTracker.Entries().ToList().ForEach(x => x.Reload());
                DataGridCategory.ItemsSource = Entities.GetContext().ThreatCategories.ToList();
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddCategoryPage(null));
        }

        /// <summary>
        /// Удаление выбранных категорий с подтверждением.
        /// Удаление каскадное: инциденты этой категории удаляются вместе с ней.
        /// </summary>
        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var categoryForRemoving = DataGridCategory.SelectedItems.Cast<ThreatCategory>().ToList();
            if (categoryForRemoving.Count == 0)
            {
                MessageBox.Show("Выберите записи для удаления!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (MessageBox.Show($"Вы точно хотите удалить записи в количестве {categoryForRemoving.Count()} элементов?",
                "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    Entities.GetContext().ThreatCategories.RemoveRange(categoryForRemoving);
                    Entities.GetContext().SaveChanges();
                    MessageBox.Show("Данные успешно удалены!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    DataGridCategory.ItemsSource = Entities.GetContext().ThreatCategories.ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString(), "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddCategoryPage((sender as Button).DataContext as ThreatCategory));
        }
    }
}

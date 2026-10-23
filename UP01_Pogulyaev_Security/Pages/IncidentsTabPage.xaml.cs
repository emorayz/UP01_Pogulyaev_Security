using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UP01_Pogulyaev_Security.Model;

namespace UP01_Pogulyaev_Security.Pages
{
    /// <summary>
    /// Таблица инцидентов ИБ: просмотр, поиск, фильтрация,
    /// добавление, редактирование, удаление.
    /// </summary>
    public partial class IncidentsTabPage : Page
    {
        public IncidentsTabPage()
        {
            InitializeComponent();
            DataGridIncident.ItemsSource = Entities.GetContext().SecurityIncidents.ToList();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Title = "Таблица инцидентов ИБ";
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                Entities.GetContext().ChangeTracker.Entries().ToList().ForEach(x => x.Reload());
                UpdateIncidents();
            }
        }

        private void clearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            nameFilterTextBox.Text = "";
            statusComboBox.SelectedIndex = 0;
            UpdateIncidents();
        }

        private void nameFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateIncidents();
        }

        private void statusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateIncidents();
        }

        /// <summary>
        /// Обновление списка инцидентов с учётом поиска и фильтра по статусу.
        /// </summary>
        private void UpdateIncidents()
        {
            if (!IsInitialized)
                return;
            try
            {
                List<SecurityIncident> incidents = Entities.GetContext().SecurityIncidents.ToList();

                if (!string.IsNullOrWhiteSpace(nameFilterTextBox.Text))
                {
                    incidents = incidents
                        .Where(x => x.Name.ToLower().Contains(nameFilterTextBox.Text.ToLower()))
                        .ToList();
                }

                if (statusComboBox.SelectedIndex > 0)
                {
                    string status = (statusComboBox.SelectedItem as ComboBoxItem).Content.ToString();
                    incidents = incidents.Where(x => x.Status == status).ToList();
                }

                DataGridIncident.ItemsSource = incidents.OrderByDescending(x => x.Date).ToList();
            }
            catch (Exception)
            {
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddIncidentPage(null));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var incidentsForRemoving = DataGridIncident.SelectedItems.Cast<SecurityIncident>().ToList();
            if (incidentsForRemoving.Count == 0)
            {
                MessageBox.Show("Выберите записи для удаления!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (MessageBox.Show($"Вы точно хотите удалить записи в количестве {incidentsForRemoving.Count()} элементов?",
                "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    Entities.GetContext().SecurityIncidents.RemoveRange(incidentsForRemoving);
                    Entities.GetContext().SaveChanges();
                    MessageBox.Show("Данные успешно удалены!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    UpdateIncidents();
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
            NavigationService?.Navigate(new AddIncidentPage((sender as Button).DataContext as SecurityIncident));
        }
    }
}

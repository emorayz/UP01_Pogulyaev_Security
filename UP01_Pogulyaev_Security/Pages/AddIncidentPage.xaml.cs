using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UP01_Pogulyaev_Security.Model;

namespace UP01_Pogulyaev_Security.Pages
{
    /// <summary>
    /// Страница добавления/редактирования инцидента ИБ.
    /// </summary>
    public partial class AddIncidentPage : Page
    {
        private SecurityIncident _currentIncident = new SecurityIncident();

        public AddIncidentPage(SecurityIncident selectedIncident)
        {
            InitializeComponent();

            // Загрузка списков категорий и пользователей из базы данных
            CBCategory.ItemsSource = Entities.GetContext().ThreatCategories.ToList();
            CBUser.ItemsSource = Entities.GetContext().Users.ToList();

            if (selectedIncident != null)
                _currentIncident = selectedIncident;
            else
                _currentIncident.Date = DateTime.Now;

            DataContext = _currentIncident;
            CBSeverity.Text = _currentIncident.Severity.ToString();
            CBStatus.Text = string.IsNullOrEmpty(_currentIncident.Status) ? "Открыт" : _currentIncident.Status;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Title = _currentIncident.ID == 0
                ? "Добавление инцидента"
                : "Редактирование инцидента";
        }

        /// <summary>
        /// Ограничение ввода в поле IP-адреса: только цифры и точки.
        /// </summary>
        private void TBIPAddress_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9.]+$");
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();
            if (string.IsNullOrWhiteSpace(_currentIncident.Name))
                errors.AppendLine("Укажите название инцидента!");
            if (CBCategory.SelectedValue == null)
                errors.AppendLine("Выберите категорию угрозы!");
            else
                _currentIncident.ThreatCategoryID = (int)CBCategory.SelectedValue;
            if (CBUser.SelectedValue == null)
                errors.AppendLine("Выберите сотрудника!");
            else
                _currentIncident.UserID = (int)CBUser.SelectedValue;
            if (DPDate.SelectedDate == null)
                errors.AppendLine("Укажите дату!");
            else
                _currentIncident.Date = DPDate.SelectedDate.Value;
            if (string.IsNullOrWhiteSpace(CBSeverity.Text))
                errors.AppendLine("Укажите критичность!");
            else
                _currentIncident.Severity = int.Parse(CBSeverity.Text);
            if (string.IsNullOrWhiteSpace(CBStatus.Text))
                errors.AppendLine("Укажите статус!");
            else
                _currentIncident.Status = CBStatus.Text;

            // Проверка формата IP-адреса, если он указан
            if (!string.IsNullOrWhiteSpace(_currentIncident.IPAddress) &&
                !Regex.IsMatch(_currentIncident.IPAddress,
                    @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$"))
                errors.AppendLine("IP-адрес указан в неверном формате!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_currentIncident.ID == 0)
                Entities.GetContext().SecurityIncidents.Add(_currentIncident);
            try
            {
                Entities.GetContext().SaveChanges();
                MessageBox.Show("Данные успешно сохранены!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TBName.Text = "";
            CBCategory.SelectedIndex = -1;
            CBUser.SelectedIndex = -1;
            DPDate.SelectedDate = DateTime.Now;
            CBSeverity.SelectedIndex = 0;
            CBStatus.SelectedIndex = 0;
            TBIPAddress.Text = "";
            TBDescription.Text = "";
        }
    }
}

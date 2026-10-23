using System;
using System.Windows;
using System.Windows.Controls;

namespace UP01_Pogulyaev_Security
{
    /// <summary>
    /// Главное окно приложения: содержит фрейм со страницами,
    /// кнопку "Назад", переключатель темы и отображение текущего времени.
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _themeInitializing = true;

        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new Pages.AuthPage());
        }

        /// <summary>
        /// При загрузке окна запускаем таймер обновления даты/времени
        /// и выставляем в списке тему, сохранённую с прошлого сеанса.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.IsEnabled = true;
            timer.Tick += (o, t) => { DateTimeNow.Text = DateTime.Now.ToString(); };
            timer.Start();

            // Восстановление выбранной ранее темы в переключателе
            foreach (ComboBoxItem item in ComboBoxTheme.Items)
            {
                if ((string)item.Tag == Properties.Settings.Default.Theme)
                {
                    ComboBoxTheme.SelectedItem = item;
                    break;
                }
            }
            _themeInitializing = false;
        }

        /// <summary>
        /// Выход из приложения с подтверждением.
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Вы действительно хотите выйти?", "Выход",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                e.Cancel = true;
        }

        /// <summary>
        /// Возврат на предыдущую страницу фрейма.
        /// </summary>
        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
                MainFrame.GoBack();
        }

        /// <summary>
        /// Смена темы приложения нажатием на пункт списка.
        /// Выбранная тема сохраняется и восстанавливается при следующем запуске.
        /// </summary>
        private void ComboBoxTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_themeInitializing || !IsLoaded)
                return;
            if (ComboBoxTheme.SelectedItem is ComboBoxItem selected)
                App.ApplyTheme((string)selected.Tag);
        }
    }
}

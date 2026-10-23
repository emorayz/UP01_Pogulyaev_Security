using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UP01_Pogulyaev_Security.Model;

namespace UP01_Pogulyaev_Security.Pages
{
    /// <summary>
    /// Страница смены пароля пользователя.
    /// </summary>
    public partial class ChangePassPage : Page
    {
        public ChangePassPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Title = "Смена пароля";
        }

        /// <summary>
        /// Сохранение нового пароля после всех проверок.
        /// </summary>
        private void ButtonSavePass_Click(object sender, RoutedEventArgs e)
        {
            // Проверка заполнения всех полей
            if (string.IsNullOrEmpty(CurrentPasswordBox.Password) ||
                string.IsNullOrEmpty(NewPasswordBox.Password) ||
                string.IsNullOrEmpty(ConfirmPasswordBox.Password) ||
                string.IsNullOrEmpty(TbLogin.Text))
            {
                MessageBox.Show("Все поля обязательны к заполнению!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка логина и текущего пароля
            string hashedPass = AuthPage.GetHash(CurrentPasswordBox.Password);
            var user = Entities.GetContext().Users
                .FirstOrDefault(u => u.Login == TbLogin.Text && u.Password == hashedPass);
            if (user == null)
            {
                MessageBox.Show("Текущий пароль/Логин неверный!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка формата нового пароля
            if (NewPasswordBox.Password.Length >= 6)
            {
                bool en = true;
                bool number = false;
                for (int i = 0; i < NewPasswordBox.Password.Length; i++)
                {
                    if (NewPasswordBox.Password[i] >= '0' && NewPasswordBox.Password[i] <= '9') number = true;
                    else if (!((NewPasswordBox.Password[i] >= 'A' && NewPasswordBox.Password[i] <= 'Z')
                        || (NewPasswordBox.Password[i] >= 'a' && NewPasswordBox.Password[i] <= 'z'))) en = false;
                }
                if (!en)
                {
                    MessageBox.Show("Используйте только английскую раскладку!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                else if (!number)
                {
                    MessageBox.Show("Добавьте хотя бы одну цифру!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                else if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
                {
                    MessageBox.Show("Пароли не совпадают!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Сохранение нового пароля в хэшированном виде
                user.Password = AuthPage.GetHash(NewPasswordBox.Password);
                Entities.GetContext().SaveChanges();
                MessageBox.Show("Пароль успешно изменен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService?.Navigate(new AuthPage());
            }
            else
            {
                MessageBox.Show("Пароль слишком короткий, должно быть минимум 6 символов!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Плейсхолдеры полей ввода
        private void TbLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblLoginHint.Visibility = TbLogin.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void CurrentPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblCurrentHint.Visibility = CurrentPasswordBox.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblNewHint.Visibility = NewPasswordBox.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblConfirmHint.Visibility = ConfirmPasswordBox.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }
    }
}

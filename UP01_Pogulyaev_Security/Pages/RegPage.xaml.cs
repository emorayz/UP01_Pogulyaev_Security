using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UP01_Pogulyaev_Security.Model;

namespace UP01_Pogulyaev_Security.Pages
{
    /// <summary>
    /// Страница регистрации новых пользователей.
    /// </summary>
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();
            comboBxRole.SelectedIndex = 0; // роль по умолчанию — Пользователь
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Title = "Регистрация";
        }

        /// <summary>
        /// Регистрация пользователя со всеми проверками:
        /// заполнение полей, уникальность логина, формат пароля, совпадение паролей.
        /// </summary>
        private void regButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка заполнения всех полей
            if (string.IsNullOrEmpty(txtbxLog.Text) || string.IsNullOrEmpty(txtbxFIO.Text) ||
                string.IsNullOrEmpty(passBxFrst.Password) || string.IsNullOrEmpty(passBxScnd.Password))
            {
                MessageBox.Show("Заполните все поля!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Entities db = Entities.GetContext();

            // Проверка уникальности логина
            var existingUser = db.Users.AsNoTracking()
                .FirstOrDefault(u => u.Login == txtbxLog.Text);
            if (existingUser != null)
            {
                MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка формата пароля: минимум 6 символов, только английская раскладка,
            // хотя бы одна цифра
            if (passBxFrst.Password.Length >= 6)
            {
                bool en = true;
                bool number = false;
                for (int i = 0; i < passBxFrst.Password.Length; i++)
                {
                    if (passBxFrst.Password[i] >= '0' && passBxFrst.Password[i] <= '9') number = true;
                    else if (!((passBxFrst.Password[i] >= 'A' && passBxFrst.Password[i] <= 'Z')
                        || (passBxFrst.Password[i] >= 'a' && passBxFrst.Password[i] <= 'z'))) en = false;
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
                else if (passBxFrst.Password != passBxScnd.Password)
                {
                    // Проверка совпадения паролей
                    MessageBox.Show("Пароли не совпадают!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                else
                {
                    // Все проверки пройдены — сохраняем пользователя с хэшированным паролем
                    User userObject = new User
                    {
                        FIO = txtbxFIO.Text,
                        Login = txtbxLog.Text,
                        Password = AuthPage.GetHash(passBxFrst.Password),
                        Role = comboBxRole.Text
                    };
                    db.Users.Add(userObject);
                    db.SaveChanges();
                    MessageBox.Show("Пользователь успешно зарегистрирован!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    txtbxLog.Clear();
                    passBxFrst.Clear();
                    passBxScnd.Clear();
                    comboBxRole.SelectedIndex = 0;
                    txtbxFIO.Clear();
                    return;
                }
            }
            else
            {
                MessageBox.Show("Пароль слишком короткий, должно быть минимум 6 символов!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }

        // Плейсхолдеры полей ввода
        private void txtbxLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblLogHitn.Visibility = txtbxLog.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void passBxFrst_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblPassHitn.Visibility = passBxFrst.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void passBxScnd_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblPassSecHitn.Visibility = passBxScnd.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void txtbxFIO_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblFioHitn.Visibility = txtbxFIO.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }
    }
}

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UP01_Pogulyaev_Security.Model;

namespace UP01_Pogulyaev_Security.Pages
{
    /// <summary>
    /// Страница авторизации пользователей.
    /// После трёх неверных попыток входа выводится капча.
    /// </summary>
    public partial class AuthPage : Page
    {
        private int failedAttempts = 0;

        public AuthPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Динамический заголовок окна — отражает содержимое страницы.
        /// </summary>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Title = "Авторизация";
        }

        /// <summary>
        /// Хэширование пароля алгоритмом SHA1.
        /// </summary>
        public static string GetHash(String password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password))
                    .Select(x => x.ToString("X2")));
            }
        }

        /// <summary>
        /// Обработчик кнопки "Вход": проверка заполнения полей,
        /// поиск пользователя в БД по логину и хэшу пароля.
        /// </summary>
        private void ButtonEnter_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBoxLogin.Text) || string.IsNullOrEmpty(PasswordBox.Password))
            {
                MessageBox.Show("Введите логин и пароль!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string hashedPassword = GetHash(PasswordBox.Password);
            using (var db = new Entities())
            {
                var user = db.Users.AsNoTracking()
                    .FirstOrDefault(u => u.Login == TextBoxLogin.Text && u.Password == hashedPassword);

                if (user == null)
                {
                    MessageBox.Show("Пользователь с такими данными не найден!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    failedAttempts++;
                    if (failedAttempts >= 3)
                    {
                        if (captcha.Visibility != Visibility.Visible)
                            CaptchaSwitch();
                        CaptchaChange();
                    }
                    return;
                }
                else
                {
                    MessageBox.Show($"Добро пожаловать, {user.FIO}!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    switch (user.Role)
                    {
                        case "User":
                            NavigationService?.Navigate(new UserPage());
                            break;
                        case "Admin":
                            NavigationService?.Navigate(new AdminPage());
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Включение/отключение отображения капчи.
        /// </summary>
        public void CaptchaSwitch()
        {
            switch (captcha.Visibility)
            {
                case Visibility.Visible:
                    TextBoxLogin.Clear();
                    PasswordBox.Clear();
                    captcha.Visibility = Visibility.Hidden;
                    captchaInput.Visibility = Visibility.Hidden;
                    labelCaptcha.Visibility = Visibility.Hidden;
                    submitCaptcha.Visibility = Visibility.Hidden;
                    labelLogin.Visibility = Visibility.Visible;
                    labelPass.Visibility = Visibility.Visible;
                    TextBoxLogin.Visibility = Visibility.Visible;
                    txtHintLogin.Visibility = Visibility.Visible;
                    PasswordBox.Visibility = Visibility.Visible;
                    txtHintPass.Visibility = Visibility.Visible;
                    ButtonChangePassword.Visibility = Visibility.Visible;
                    ButtonEnter.Visibility = Visibility.Visible;
                    ButtonReg.Visibility = Visibility.Visible;
                    return;
                case Visibility.Hidden:
                    captcha.Visibility = Visibility.Visible;
                    captchaInput.Visibility = Visibility.Visible;
                    labelCaptcha.Visibility = Visibility.Visible;
                    submitCaptcha.Visibility = Visibility.Visible;
                    labelLogin.Visibility = Visibility.Hidden;
                    labelPass.Visibility = Visibility.Hidden;
                    TextBoxLogin.Visibility = Visibility.Hidden;
                    txtHintLogin.Visibility = Visibility.Hidden;
                    PasswordBox.Visibility = Visibility.Hidden;
                    txtHintPass.Visibility = Visibility.Hidden;
                    ButtonChangePassword.Visibility = Visibility.Hidden;
                    ButtonEnter.Visibility = Visibility.Hidden;
                    ButtonReg.Visibility = Visibility.Hidden;
                    return;
            }
        }

        /// <summary>
        /// Генерация новой капчи из 6 случайных символов.
        /// </summary>
        public void CaptchaChange()
        {
            String allowchar = "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z";
            allowchar += ",a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,y,z";
            allowchar += ",1,2,3,4,5,6,7,8,9,0";
            char[] a = { ',' };
            String[] ar = allowchar.Split(a);
            String pwd = "";
            Random r = new Random();
            for (int i = 0; i < 6; i++)
            {
                pwd += ar[(r.Next(0, ar.Length))];
            }
            captcha.Text = pwd;
        }

        /// <summary>
        /// Проверка введённой капчи.
        /// </summary>
        private void submitCaptcha_Click(object sender, RoutedEventArgs e)
        {
            if (captchaInput.Text != captcha.Text)
            {
                MessageBox.Show("Неверно введена капча", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                captchaInput.Clear();
                CaptchaChange();
            }
            else
            {
                MessageBox.Show("Капча введена успешно, можете продолжить авторизацию", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                CaptchaSwitch();
                failedAttempts = 0;
            }
        }

        /// <summary>
        /// Запрет копирования/вырезания/вставки текста капчи.
        /// </summary>
        private void textBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        // Плейсхолдеры полей ввода
        private void TextBoxLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtHintLogin.Visibility = TextBoxLogin.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            txtHintPass.Visibility = PasswordBox.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        // Переходы между страницами
        private void ButtonReg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegPage());
        }

        private void ButtonChangePassword_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ChangePassPage());
        }
    }
}

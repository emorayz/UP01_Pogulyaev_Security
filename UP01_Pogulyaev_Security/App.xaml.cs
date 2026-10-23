using System;
using System.Windows;

namespace UP01_Pogulyaev_Security
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Восстанавливаем тему, выбранную пользователем в прошлый сеанс
            ApplyTheme(Properties.Settings.Default.Theme);
        }

        /// <summary>
        /// Применяет словарь стилей (тему) ко всему приложению.
        /// </summary>
        /// <param name="themeName">"Light" - тема по Руководству по стилю, "Dark" - собственная тема</param>
        public static void ApplyTheme(string themeName)
        {
            var uri = new Uri(themeName == "Dark" ? "DictionaryDark.xaml" : "Dictionary.xaml", UriKind.Relative);
            ResourceDictionary resourceDict = LoadComponent(uri) as ResourceDictionary;
            Current.Resources.Clear();
            Current.Resources.MergedDictionaries.Add(resourceDict);

            // Сохраняем выбор темы, чтобы он не сбрасывался при выходе
            Properties.Settings.Default.Theme = themeName;
            Properties.Settings.Default.Save();
        }
    }
}

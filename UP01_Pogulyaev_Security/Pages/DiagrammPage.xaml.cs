using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;
using UP01_Pogulyaev_Security.Model;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;

namespace UP01_Pogulyaev_Security.Pages
{
    /// <summary>
    /// Страница диаграмм: отображение инцидентов ИБ выбранного сотрудника
    /// по категориям угроз, экспорт отчётов в Word, PDF и Excel.
    /// </summary>
    public partial class DiagrammPage : Page
    {
        private Entities _context = new Entities();

        public DiagrammPage()
        {
            InitializeComponent();

            // Область построения диаграммы
            ChartIncidents.ChartAreas.Add(new ChartArea("Main"));

            // Набор данных: количество инцидентов по категориям угроз
            var currentSeries = new Series("Инциденты")
            {
                IsValueShownAsLabel = true
            };
            ChartIncidents.Series.Add(currentSeries);

            // Загрузка данных в списки выбора
            CmbUser.ItemsSource = _context.Users.ToList();
            CmbDiagram.ItemsSource = Enum.GetValues(typeof(SeriesChartType));
            CmbUser.SelectedIndex = 0;
            CmbDiagram.SelectedItem = SeriesChartType.Column;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Title = "Диаграммы и отчёты";
        }

        /// <summary>
        /// Обновление диаграммы: количество инцидентов выбранного сотрудника
        /// в разрезе категорий угроз.
        /// </summary>
        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            if (CmbUser.SelectedItem is User currentUser &&
                CmbDiagram.SelectedItem is SeriesChartType currentType)
            {
                Series currentSeries = ChartIncidents.Series.FirstOrDefault();
                currentSeries.ChartType = currentType;
                currentSeries.Points.Clear();
                var categoriesList = _context.ThreatCategories.ToList();
                foreach (var category in categoriesList)
                {
                    currentSeries.Points.AddXY(category.Name,
                        _context.SecurityIncidents.ToList()
                            .Where(u => u.UserID == currentUser.ID && u.ThreatCategoryID == category.ID)
                            .Count());
                }
            }
        }

        /// <summary>
        /// Экспорт отчёта об инцидентах в Microsoft Word (.docx и .pdf).
        /// Для каждого сотрудника выводится таблица количества инцидентов
        /// по категориям, а также самый критичный и самый незначительный инцидент.
        /// </summary>
        private void BtnExportWord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var allUsers = _context.Users.ToList();
                var allCategories = _context.ThreatCategories.ToList();

                var application = new Word.Application();
                Word.Document document = application.Documents.Add();

                foreach (var user in allUsers)
                {
                    // Заголовок страницы — ФИО сотрудника
                    Word.Paragraph userParagraph = document.Paragraphs.Add();
                    Word.Range userRange = userParagraph.Range;
                    userRange.Text = user.FIO;
                    userParagraph.set_Style("Заголовок 1");
                    userRange.ParagraphFormat.Alignment =
                        Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    userRange.InsertParagraphAfter();
                    document.Paragraphs.Add(); // пустая строка

                    // Таблица инцидентов по категориям
                    Word.Paragraph tableParagraph = document.Paragraphs.Add();
                    Word.Range tableRange = tableParagraph.Range;
                    Word.Table incidentsTable = document.Tables.Add(tableRange,
                        allCategories.Count() + 1, 2);
                    incidentsTable.Borders.InsideLineStyle =
                        incidentsTable.Borders.OutsideLineStyle =
                        Word.WdLineStyle.wdLineStyleSingle;
                    incidentsTable.Range.Cells.VerticalAlignment =
                        Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                    // Названия колонок
                    Word.Range cellRange;
                    cellRange = incidentsTable.Cell(1, 1).Range;
                    cellRange.Text = "Категория угрозы";
                    cellRange = incidentsTable.Cell(1, 2).Range;
                    cellRange.Text = "Количество инцидентов";
                    incidentsTable.Rows[1].Range.Font.Name = "Times New Roman";
                    incidentsTable.Rows[1].Range.Font.Size = 14;
                    incidentsTable.Rows[1].Range.Bold = 1;
                    incidentsTable.Rows[1].Range.ParagraphFormat.Alignment =
                        Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    // Заполнение таблицы данными
                    for (int i = 0; i < allCategories.Count(); i++)
                    {
                        var currentCategory = allCategories[i];
                        cellRange = incidentsTable.Cell(i + 2, 1).Range;
                        cellRange.Text = currentCategory.Name;
                        cellRange.Font.Name = "Times New Roman";
                        cellRange.Font.Size = 12;
                        cellRange = incidentsTable.Cell(i + 2, 2).Range;
                        cellRange.Text = user.SecurityIncidents.ToList()
                            .Where(u => u.ThreatCategoryID == currentCategory.ID)
                            .Count().ToString() + " шт.";
                        cellRange.Font.Name = "Times New Roman";
                        cellRange.Font.Size = 12;
                    }
                    document.Paragraphs.Add(); // пустая строка

                    // Самый критичный инцидент (максимальная критичность)
                    SecurityIncident maxIncident = user.SecurityIncidents
                        .OrderByDescending(u => u.Severity).FirstOrDefault();
                    if (maxIncident != null)
                    {
                        Word.Paragraph maxParagraph = document.Paragraphs.Add();
                        Word.Range maxRange = maxParagraph.Range;
                        maxRange.Text = $"Самый критичный инцидент - {maxIncident.Name} " +
                            $"(критичность {maxIncident.Severity}) от {maxIncident.Date:dd.MM.yyyy}";
                        maxParagraph.set_Style("Заголовок 2");
                        maxRange.Font.Color = Word.WdColor.wdColorDarkRed;
                        maxRange.InsertParagraphAfter();
                    }

                    // Самый незначительный инцидент (минимальная критичность)
                    SecurityIncident minIncident = user.SecurityIncidents
                        .OrderBy(u => u.Severity).FirstOrDefault();
                    if (minIncident != null)
                    {
                        Word.Paragraph minParagraph = document.Paragraphs.Add();
                        Word.Range minRange = minParagraph.Range;
                        minRange.Text = $"Самый незначительный инцидент - {minIncident.Name} " +
                            $"(критичность {minIncident.Severity}) от {minIncident.Date:dd.MM.yyyy}";
                        minParagraph.set_Style("Заголовок 2");
                        minRange.Font.Color = Word.WdColor.wdColorDarkGreen;
                        minRange.InsertParagraphAfter();
                    }

                    // Разрыв страницы между сотрудниками
                    if (user != allUsers.LastOrDefault())
                        document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
                }

                // Нижний колонтитул с номером страницы
                foreach (Word.Section section in document.Sections)
                {
                    Word.HeaderFooter footer =
                        section.Footers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                    footer.PageNumbers.Add(Word.WdPageNumberAlignment.wdAlignPageNumberCenter);
                }

                // Верхний колонтитул с текущей датой
                foreach (Word.Section section in document.Sections)
                {
                    Word.Range headerRange =
                        section.Headers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                    headerRange.Text = DateTime.Now.ToString("dd/MM/yyyy");
                    headerRange.ParagraphFormat.Alignment =
                        Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    headerRange.Font.Size = 10;
                }

                // Сохранение документа в .docx и .pdf
                string basePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                document.SaveAs2(basePath + @"\SecurityIncidents.docx");
                document.SaveAs2(basePath + @"\SecurityIncidents.pdf",
                    Word.WdSaveFormat.wdFormatPDF);

                application.Visible = true;
                MessageBox.Show("Отчёт сохранён на рабочем столе: SecurityIncidents.docx и SecurityIncidents.pdf",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Экспорт отчёта об инцидентах в Microsoft Excel (.xlsx).
        /// Для каждого сотрудника создаётся отдельный лист
        /// с количеством инцидентов по категориям угроз.
        /// </summary>
        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var allUsers = _context.Users.ToList();
                var allCategories = _context.ThreatCategories.ToList();

                var application = new Excel.Application();
                Excel.Workbook workbook = application.Workbooks.Add();
                // Убираем лишние листы, оставляем один
                while (workbook.Worksheets.Count > 1)
                    workbook.Worksheets[workbook.Worksheets.Count].Delete();

                bool firstSheet = true;
                foreach (var user in allUsers)
                {
                    Excel.Worksheet worksheet;
                    if (firstSheet)
                    {
                        worksheet = workbook.Worksheets[1];
                        firstSheet = false;
                    }
                    else
                    {
                        worksheet = workbook.Worksheets.Add(After: workbook.Worksheets[workbook.Worksheets.Count]);
                    }

                    // Имя листа — ФИО сотрудника (не более 31 символа, без запрещённых знаков)
                    string sheetName = user.FIO;
                    foreach (char c in new char[] { '\\', '/', '?', '*', '[', ']', ':' })
                        sheetName = sheetName.Replace(c, ' ');
                    worksheet.Name = sheetName.Length > 31 ? sheetName.Substring(0, 31) : sheetName;

                    // Заголовки колонок
                    worksheet.Cells[1, 1] = "Категория угрозы";
                    worksheet.Cells[1, 2] = "Количество инцидентов";
                    Excel.Range headerRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 2]];
                    headerRange.Font.Bold = true;

                    // Данные по категориям
                    for (int i = 0; i < allCategories.Count(); i++)
                    {
                        worksheet.Cells[i + 2, 1] = allCategories[i].Name;
                        worksheet.Cells[i + 2, 2] = user.SecurityIncidents.ToList()
                            .Where(u => u.ThreatCategoryID == allCategories[i].ID)
                            .Count();
                    }
                    worksheet.Columns.AutoFit();
                }

                // Сохранение книги
                string basePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                workbook.SaveAs(basePath + @"\SecurityIncidents.xlsx");

                application.Visible = true;
                MessageBox.Show("Отчёт сохранён на рабочий стол: SecurityIncidents.xlsx",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

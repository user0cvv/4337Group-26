using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using WpfGitAppVS20.Models;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace WpfGitAppVS20
{
    public partial class _4337_Sakaev : Window
    {
        private AppDbContext db = new AppDbContext();

        public _4337_Sakaev()
        {
            InitializeComponent();

            ExcelPackage.License.SetNonCommercialPersonal("Арнил Сакаев");

            db.Database.EnsureCreated();
            LoadData();
        }

        private void LoadData()
        {
            clientsGrid.ItemsSource = null;
            clientsGrid.ItemsSource = db.Clients.ToList();
        }

        private int CalculateAge(DateTime birthDate)
        {
            int age = DateTime.Now.Year - birthDate.Year;
            if (DateTime.Now < birthDate.AddYears(age))
                age--;
            return age;
        }

        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var package = new ExcelPackage(new FileInfo(openFileDialog.FileName)))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        int rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            string fullName = worksheet.Cells[row, 1].Text;
                            string code = worksheet.Cells[row, 2].Text;
                            string birthDateText = worksheet.Cells[row, 3].Text;
                            string email = worksheet.Cells[row, 9].Text;

                            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(code))
                                continue;

                            DateTime birthDate = DateTime.Parse(birthDateText);
                            int age = CalculateAge(birthDate);

                            bool exists = db.Clients.Any(c => c.ClientCode == code);
                            if (exists) continue;

                            Client client = new Client
                            {
                                ClientCode = code,
                                FullName = fullName,
                                Email = email,
                                BirthDate = birthDate,
                                Age = age
                            };

                            db.Clients.Add(client);
                        }

                        db.SaveChanges();
                        LoadData();

                        MessageBox.Show("Данные из Excel успешно импортированы!");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка импорта Excel: " + ex.Message);
                }
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var group1 = db.Clients.Where(c => c.Age >= 20 && c.Age <= 29).ToList();
                var group2 = db.Clients.Where(c => c.Age >= 30 && c.Age <= 39).ToList();
                var group3 = db.Clients.Where(c => c.Age >= 40).ToList();

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx";
                saveFileDialog.FileName = "ExportedClients.xlsx";

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var package = new ExcelPackage())
                    {
                        CreateExcelSheet(package, "20-29", group1);
                        CreateExcelSheet(package, "30-39", group2);
                        CreateExcelSheet(package, "40+", group3);

                        package.SaveAs(new FileInfo(saveFileDialog.FileName));
                    }

                    MessageBox.Show("Экспорт в Excel выполнен успешно!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка экспорта Excel: " + ex.Message);
            }
        }

        private void CreateExcelSheet(ExcelPackage package, string sheetName, List<Client> clients)
        {
            var worksheet = package.Workbook.Worksheets.Add(sheetName);

            worksheet.Cells[1, 1].Value = "Код клиента";
            worksheet.Cells[1, 2].Value = "ФИО";
            worksheet.Cells[1, 3].Value = "E-mail";

            int row = 2;
            foreach (var client in clients)
            {
                worksheet.Cells[row, 1].Value = client.ClientCode;
                worksheet.Cells[row, 2].Value = client.FullName;
                worksheet.Cells[row, 3].Value = client.Email;
                row++;
            }
        }

        private void btnImportJson_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON files (*.json)|*.json";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string json = File.ReadAllText(openFileDialog.FileName);

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    List<JsonClient> jsonClients = JsonSerializer.Deserialize<List<JsonClient>>(json, options);

                    if (jsonClients == null || jsonClients.Count == 0)
                    {
                        MessageBox.Show("Файл JSON пустой или имеет неверный формат.");
                        return;
                    }

                    foreach (var item in jsonClients)
                    {
                        bool exists = db.Clients.Any(c => c.ClientCode == item.CodeClient);
                        if (exists) continue;

                        DateTime birthDate = DateTime.ParseExact(item.BirthDate, "dd.MM.yyyy", null);
                        int age = CalculateAge(birthDate);

                        Client client = new Client
                        {
                            ClientCode = item.CodeClient,
                            FullName = item.FullName,
                            Email = item.E_mail,
                            BirthDate = birthDate,
                            Age = age
                        };

                        db.Clients.Add(client);
                    }

                    db.SaveChanges();
                    LoadData();

                    MessageBox.Show("Данные из JSON успешно импортированы!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка импорта JSON: " + ex.Message);
                }
            }
        }

        private void btnExportWord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var group1 = db.Clients.Where(c => c.Age >= 20 && c.Age <= 29).ToList();
                var group2 = db.Clients.Where(c => c.Age >= 30 && c.Age <= 39).ToList();
                var group3 = db.Clients.Where(c => c.Age >= 40).ToList();

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Word files (*.docx)|*.docx";
                saveFileDialog.FileName = "ClientsByAge.docx";

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (var document = DocX.Create(saveFileDialog.FileName))
                    {
                        AddWordCategory(document, "Категория 1 (20–29 лет)", group1);
                        document.InsertSectionPageBreak();

                        AddWordCategory(document, "Категория 2 (30–39 лет)", group2);
                        document.InsertSectionPageBreak();

                        AddWordCategory(document, "Категория 3 (40+ лет)", group3);

                        document.Save();
                    }

                    MessageBox.Show("Экспорт в Word выполнен успешно!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка экспорта Word: " + ex.Message);
            }
        }

        private void AddWordCategory(DocX document, string title, List<Client> clients)
        {
            document.InsertParagraph(title)
                    .Bold()
                    .FontSize(16)
                    .SpacingAfter(15);

            var table = document.AddTable(clients.Count + 1, 3);
            table.Design = TableDesign.TableGrid;

            table.Rows[0].Cells[0].Paragraphs[0].Append("Код клиента").Bold();
            table.Rows[0].Cells[1].Paragraphs[0].Append("ФИО").Bold();
            table.Rows[0].Cells[2].Paragraphs[0].Append("E-mail").Bold();

            for (int i = 0; i < clients.Count; i++)
            {
                table.Rows[i + 1].Cells[0].Paragraphs[0].Append(clients[i].ClientCode);
                table.Rows[i + 1].Cells[1].Paragraphs[0].Append(clients[i].FullName);
                table.Rows[i + 1].Cells[2].Paragraphs[0].Append(clients[i].Email);
            }

            document.InsertTable(table);
        }
    }
}
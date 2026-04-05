using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using WpfGitAppVS20.Models;

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

                        MessageBox.Show("Данные успешно импортированы!");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка импорта: " + ex.Message);
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
                        CreateSheet(package, "20-29", group1);
                        CreateSheet(package, "30-39", group2);
                        CreateSheet(package, "40+", group3);

                        package.SaveAs(new FileInfo(saveFileDialog.FileName));
                    }

                    MessageBox.Show("Экспорт выполнен успешно!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка экспорта: " + ex.Message);
            }
        }

        private void CreateSheet(ExcelPackage package, string sheetName, List<Client> clients)
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
    }
}
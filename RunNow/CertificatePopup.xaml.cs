using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RunNow.View
{
    public partial class CertificatePopup : Window
    {
        public string SelectedCertificate { get; private set; }
        private List<Certificate> _certificates = new();

        public CertificatePopup()
        {
            InitializeComponent();
            LoadCertificates();
            SearchBox.TextChanged += SearchBox_TextChanged;
        }

        private void LoadCertificates()
        {
            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "certificates.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                _certificates = JsonSerializer.Deserialize<List<Certificate>>(json);
                CertificateListBox.ItemsSource = _certificates;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = SearchBox.Text.Trim();
            CertificateListBox.ItemsSource = _certificates
                .Where(c => c.name.Contains(keyword)).ToList();
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (CertificateListBox.SelectedItem is Certificate selected)
            {
                SelectedCertificate = selected.name;
                DialogResult = true;
                Close();
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "자격증 검색")
            {
                SearchBox.Text = "";
                SearchBox.Foreground = Brushes.Black;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "자격증 검색";
                SearchBox.Foreground = Brushes.Gray;
            }
        }
    }

    public class Certificate
    {
        public string code { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string series { get; set; }

        public override string ToString() => name;
    }
}

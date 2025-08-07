using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace RunNow.ViewModels
{
    public partial class CertificatePopupViewModel : ObservableObject
    {
        [ObservableProperty] private string searchKeyword;
        [ObservableProperty] private ObservableCollection<Certificate> filteredCertificates;
        [ObservableProperty] private Certificate selectedCertificate;

        private List<Certificate> _allCertificates;

        public CertificatePopupViewModel()
        {
            LoadCertificates();
        }

        private void LoadCertificates()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "certificates.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                _allCertificates = JsonSerializer.Deserialize<List<Certificate>>(json) ?? new List<Certificate>();
                FilteredCertificates = new ObservableCollection<Certificate>(_allCertificates);
            }
            else
            {
                _allCertificates = new List<Certificate>();
                FilteredCertificates = new ObservableCollection<Certificate>();
            }
        }

        partial void OnSearchKeywordChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                FilteredCertificates = new ObservableCollection<Certificate>(_allCertificates);
            }
            else
            {
                FilteredCertificates = new ObservableCollection<Certificate>(
                    _allCertificates.Where(c => c.name.Contains(value))
                );
            }
        }

        [RelayCommand]
        private void Confirm()
        {
            if (SelectedCertificate != null)
            {
                // DialogService에서 결과 전달
                CloseAction?.Invoke(SelectedCertificate.name);
            }
        }

        public Action<string> CloseAction { get; set; }
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

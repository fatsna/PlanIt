using System;
using System.ComponentModel;

namespace PlanIt.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string name;
        private string userId;
        private string password;
        private DateTime? birthDate;
        private bool isMale;
        private bool isFemale;
        private string address;
        private string phoneNumber;
        public float[] FaceEmbedding { get; set; }  // 👈 추가

        public string Name
        {
            get => name;
            set { name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string UserId
        {
            get => userId;
            set { userId = value; OnPropertyChanged(nameof(UserId)); }
        }

        public string Password
        {
            get => password;
            set { password = value; OnPropertyChanged(nameof(Password)); }
        }

        public DateTime? BirthDate
        {
            get => birthDate;
            set { birthDate = value; OnPropertyChanged(nameof(BirthDate)); }
        }

        public bool IsMale
        {
            get => isMale;
            set { isMale = value; OnPropertyChanged(nameof(IsMale)); }
        }

        public bool IsFemale
        {
            get => isFemale;
            set { isFemale = value; OnPropertyChanged(nameof(IsFemale)); }
        }

        public string Address
        {
            get => address;
            set { address = value; OnPropertyChanged(nameof(Address)); }
        }

        public string PhoneNumber
        {
            get => phoneNumber;
            set { phoneNumber = value; OnPropertyChanged(nameof(PhoneNumber)); }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

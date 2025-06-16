using OtpNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace LockBox
{
    public class ServiceEntryViewModel : INotifyPropertyChanged
    {
        private string _serv_name;
        private string _serv_email;
        private string _serv_password;
        private string _serv_mfasec;
        private string _otpCode;
        private string _algorithm;
        private double _otpProgress;
        private System.Timers.Timer _timer;

        public int Db_id { get; set; }
        public string Serv_name
        {
            get => _serv_name;
            set
            {
                _serv_name = value;
                OnPropertyChanged();
            }
        }
        public string Serv_email
        {
            get => _serv_email;
            set
            {
                _serv_email = value;
                OnPropertyChanged();
            }
        }
        public string Serv_password
        {
            get => _serv_password;
            set
            {
                _serv_password = value;
                OnPropertyChanged();
            }
        }
        public string Serv_mfasec
        {
            get => _serv_mfasec;
            set
            {
                _serv_mfasec = value;
                OnPropertyChanged();
            }
        }
        public string OtpCode
        {
            get => _otpCode;
            set
            {
                _otpCode = value;
                OnPropertyChanged();
            }
        }
        public string Algorithm
        {
            get => _algorithm;
            set
            {
                _algorithm = value;
                OnPropertyChanged();
            }
        }
        public double OtpProgress
        {
            get => _otpProgress;
            set
            {
                _otpProgress = value;
                OnPropertyChanged();
            }
        }

        public ServiceEntryViewModel()
        {
            StartOtpTimer();
        }

        private void StartOtpTimer()
        {
            _timer = new System.Timers.Timer(1000); // Update every second
            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            var totp = new Totp(Base32Encoding.ToBytes(Serv_mfasec), mode: OtpHashMode.Sha1);
            var timeRemaining = totp.RemainingSeconds();
            OtpProgress = timeRemaining / 30.0; // Assuming OTP updates every 30 seconds
            if (timeRemaining == 0)
            {
                UpdateOtpCode();
            }
        }

        public void UpdateOtpCode()
        {
            try
            {
                Debug.WriteLine($"Generating OTP code for {Serv_name}");
                OtpCode = string.IsNullOrEmpty(Serv_mfasec) ? "MFA Not Set" : ServiceEntry.GetCurrentCode(new ServiceEntry(Serv_name, Serv_email, Serv_password, Serv_mfasec, Algorithm));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
       
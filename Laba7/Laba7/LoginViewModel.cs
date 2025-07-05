using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace Laba7
{
    public class LoginViewModel: INotifyPropertyChanged
    {
        private bool _isEmployee = true;
        private string _login;
        private string _password;
        private string _phoneNumber;
        private int? _code;
        private bool _isLoading = false;
        private double _progress;
        private string _errorMessage;

        public ICommand ToggleRoleCommand { get; }
        public ICommand SendCodeCommand { get; }
        private readonly Random _random = new Random();
        private Window _codeWindow;

        public ICommand LoginCommand { get; }
        public ICommand CancelLoadCommand { get; }

        public LoginViewModel()
        {
            ToggleRoleCommand = new RelayCommand(ToggleRole);
            SendCodeCommand = new RelayCommand(SendCode);
            LoginCommand = new RelayCommand(Loginning);
            CancelLoadCommand = new RelayCommand(CancelLoad);
        }

        public bool IsEmployee
        {
            get => _isEmployee;
            set => SetProperty(ref _isEmployee, value);
        }

        public string Login
        {
            get => _login;
            set
            {
                SetProperty(ref _login, value.Replace("8", "").Replace("+7", ""));
            }
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                var cleaned = value.Replace("8", "").Replace("+7", "");
                SetProperty(ref _phoneNumber, cleaned);
            }
        }

        public int? Code
        {
            get => _code;
            set => SetProperty(ref _code, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public double Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private void ToggleRole(object obj) => IsEmployee = !IsEmployee;

        private void SendCode(object obj)
        {
            if (!IsPhoneNumberValid(PhoneNumber))
            {
                MessageBox.Show("Номер телефона не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Code = _random.Next(1000, 9999);
            _codeWindow = new CodeWindow{ DataContext = this };
            _codeWindow.Show();
        }

        private bool IsPhoneNumberValid(string number)
        {
            var validNumbers = new[] { "9012345678", "9212345678" };
            return !string.IsNullOrEmpty(number) && validNumbers.Contains(number);
        }

        private async void Loginning(object obj)
        {
            if (IsEmployee)
            {
                if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(Password))
                {
                    ErrorMessage = "Введите логин и пароль";
                    return;
                }
            }
            else
            {
                if (!Code.HasValue)
                {
                    ErrorMessage = "Сначала получите код";
                    return;
                }
            }

            IsLoading = true;
            Progress = 0;

            await Task.Run(async () =>
            {
                for (int i = 0; i <= 100; i += 10)
                {
                    if (!IsLoading) break;

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        Progress = i;
                    });

                    await Task.Delay(200);
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (IsLoading)
                    {
                        var mainWindow = new MainWindow();
                        mainWindow.Show();
                        foreach (Window window in Application.Current.Windows)
                            window.Close();
                    }
                });
            });
        }

        private void CancelLoad(object obj)
        {
            IsLoading = false;
            Progress = 0;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}

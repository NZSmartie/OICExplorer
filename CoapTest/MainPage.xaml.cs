using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CoapTest.Extensions;
using CoapTest.Models;
using CoapTest.Services;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CoapTest
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        private Command _refreshDevicesCommand = null;

        public Command RefreshDevicesCommand
        {
            get => _refreshDevicesCommand ?? (_refreshDevicesCommand = new Command(RefreshDevices, () => !IsDevicesRefreshing));
        }

        private bool _isDevicesRefreshing = false;

        public bool IsDevicesRefreshing
        {
            get => _isDevicesRefreshing;
            set {
                _isDevicesRefreshing = value;
                PropertyChanged?.Invoke(this, () => IsDevicesRefreshing);
            }
        }


        private readonly IDeviceService _deviceService = DependencyService.Get<CoapService>();

        private ObservableCollection<IDevice> _devices = new ObservableCollection<IDevice>();
        public ObservableCollection<IDevice> Devices
        {
            get => _devices;
            set
            {
                _devices = value;
                PropertyChanged?.Invoke(this, () => Devices);
            }
        }

        private IDevice _selectedDevice = null;
        public IDevice SelectedDevice
        {
            get => _selectedDevice; set
            {
                _selectedDevice = value;
                PropertyChanged?.Invoke(this, () => SelectedDevice);
            }
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;

            _deviceService.NewDevice += OnNewDeviceHandller;

            RefreshDevices();
        }

        private void OnNewDeviceHandller(object sender, NewCoapDeviceEventArgs eventArgs)
        {
            Device.BeginInvokeOnMainThread(() => {
                Devices.Add(eventArgs.CoapDevice);
            });
        }

        private void RefreshDevices()
        {
            IsDevicesRefreshing = true;
            SelectedDevice = null;
            Devices.Clear();

            _deviceService.Discover();

            Task.Run(async () =>
            {
                // Some arbatary delay, as the devices will respond over a lesiure period. 5 seconds is the default period
                await Task.Delay(5000);
                Device.BeginInvokeOnMainThread(() => IsDevicesRefreshing = false);
            });
        }

        private void OnDeviceSelected(object sender, SelectedItemChangedEventArgs e)
        {
            Navigation.PushAsync(new ResourcePage()
            {
                CoapDevice = SelectedDevice
            });
        }
    }
}

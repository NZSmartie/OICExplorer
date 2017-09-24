using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CoapTest.Extensions;
using CoapTest.Services;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using OICNet;

namespace CoapTest
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        private Command _refreshDevicesCommand;

        public Command RefreshDevicesCommand => _refreshDevicesCommand ?? (_refreshDevicesCommand = new Command(RefreshDevices, () => !IsDevicesRefreshing));

        private bool _isDevicesRefreshing;

        public bool IsDevicesRefreshing
        {
            get => _isDevicesRefreshing;
            set {
                _isDevicesRefreshing = value;
                PropertyChanged?.Invoke(this, () => IsDevicesRefreshing);
            }
        }


        private readonly OicService _deviceService = DependencyService.Get<OicService>();

        private ObservableCollection<OicDevice> _devices = new ObservableCollection<OicDevice>();
        public ObservableCollection<OicDevice> Devices
        {
            get => _devices;
            set
            {
                _devices = value;
                PropertyChanged?.Invoke(this, () => Devices);
            }
        }

        private OicDevice _selectedDevice;
        public OicDevice SelectedDevice
        {
            get => _selectedDevice;
            set
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

        private void OnNewDeviceHandller(object sender, NewOicDeviceEventArgs eventArgs)
        {
            Device.BeginInvokeOnMainThread(() => {
                Devices.Add(eventArgs.Device);
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
                OicDevice = SelectedDevice
            });
        }
    }
}

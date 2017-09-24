using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Rg.Plugins.Popup.Extensions;

using CoapTest.Extensions;
using CoapTest.Services;
using Newtonsoft.Json;
using OICNet;

namespace CoapTest
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ResourcePage : MasterDetailPage, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler PropertyChanged;

        private readonly OicService _coapService = DependencyService.Get<OicService>();

        private Command _executeResourceCommand;
        public Command ExecuteResourceCommand => _executeResourceCommand ?? (_executeResourceCommand = new Command<OicRequestOperation>(DoThatThing, obj => DeviceResoucesListView.SelectedItem != null));

        private FormattedString _messageLog = new FormattedString();
        public FormattedString MessageLog
        {
            get => _messageLog;
            set
            {
                _messageLog = value;
                PropertyChanged?.Invoke(this, () => MessageLog);
            }
        }

        private string _resourceUri = "";
        public string ResourceUri
        {
            get => _resourceUri; set
            {
                _resourceUri = value;
                PropertyChanged?.Invoke(this, () => ResourceUri);
            }
        }

        public IOicResource SelectedResource => ((IOicResource) DeviceResoucesListView.SelectedItem);

        private OicDevice _device;
        public OicDevice OicDevice
        {
            get => _device;
            set
            {
                _device = value;
                _repository = _coapService.GetRepository(_device);
                PropertyChanged?.Invoke(this, () => OicDevice);
            }
        }

        private IOicResourceRepository _repository;

        public IOicResourceRepository Repository { get => _repository; } 

        public ResourcePage()
        {
            BindingContext = this;
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            IsPresented = true;

            _coapService.NewDevice += OnNewDevice;
            //_coapService.MessageReceived += OnCoapMessageReceived;
        }

        protected override void OnDisappearing()
        {
            _coapService.NewDevice -= OnNewDevice;
            //_coapService.MessageReceived -= OnCoapMessageReceived;
        }

        private void OnNewDevice(object sender, NewOicDeviceEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"For new device evvent for {e.Device.Name}");
        }

        private async void DoThatThing(OicRequestOperation method)
        {
            if(SelectedResource==null)
                return;

            var request = OicRequest.Create(SelectedResource.RelativeUri);
            var response = await Repository.RetrieveAsync(request);

            SelectedResource.UpdateFields(response.Resource);
        }

        private void OnDeviceResouceSelected(object sender, SelectedItemChangedEventArgs e)
        {
            _executeResourceCommand?.ChangeCanExecute();
            if(MasterBehavior == MasterBehavior.Popover)
                IsPresented = false;

            if (e.SelectedItem == null)
                return;

            ResourceUri = new UriBuilder {
                Scheme = "oic",
                Host = (OicDevice as OicRemoteDevice).Endpoint.Authority,
                Path = SelectedResource.RelativeUri
            }.ToString();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            Navigation.PushPopupAsync(new Dialogs.ColourPickerDialog());
        }
    }
}

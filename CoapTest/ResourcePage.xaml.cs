using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using CoapTest.Extensions;
using CoapTest.Models;
using CoapTest.Services;
using CoAP.Net;

namespace CoapTest
{
    public enum CommandResourceType
    {
        Get = 1, Put = 2, Post = 3, Delete = 4
    };

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ResourcePage : MasterDetailPage, INotifyPropertyChanged
    {
        new public event PropertyChangedEventHandler PropertyChanged;

        private CoapService _coapService = DependencyService.Get<CoapService>();

        public readonly Dictionary<string, CoAP.Net.Options.ContentFormatType> AcceptList = new Dictionary<string, CoAP.Net.Options.ContentFormatType>
        {
            {"text/plain", CoAP.Net.Options.ContentFormatType.TextPlain },
            {"application/link-format", CoAP.Net.Options.ContentFormatType.ApplicationLinkFormat },
            {"application/xml", CoAP.Net.Options.ContentFormatType.ApplicationXml },
            {"application/octet-stream", CoAP.Net.Options.ContentFormatType.ApplicationOctetStream },
            {"application/exi", CoAP.Net.Options.ContentFormatType.ApplicationExi },
            {"application/json", CoAP.Net.Options.ContentFormatType.ApplicationJson },
            {"applicaiton/cbor",CoAP.Net.Options.ContentFormatType.ApplicationCbor },
        };

        public readonly Dictionary<string, CoAP.Net.CoapMessageType> MessageTypeList = new Dictionary<string, CoAP.Net.CoapMessageType>
        {
            {"Confirmable", CoapMessageType.Confirmable},
            {"Non-Confirmable", CoapMessageType.NonConfirmable },
            {"Acknowledgement", CoapMessageType.Acknowledgement },
            {"Reset", CoapMessageType.Reset},
        };

        private Command _executeResourceCommand;
        public Command ExecuteResourceCommand { get => _executeResourceCommand ?? (_executeResourceCommand = new Command<CommandResourceType>(obj => DoThatThing(obj), obj => DeviceResoucesListView.SelectedItem != null)); }

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

        private CoapDevice _device;
        public CoapDevice CoapDevice
        {
            get => _device;
            set
            {
                _device = value;
                PropertyChanged?.Invoke(this, () => CoapDevice);
            }
        }

        public ResourcePage()
        {
            BindingContext = this;
            InitializeComponent();

            AcceptPicker.Items.Add("(Default)");
            foreach (var acceptType in AcceptList.Keys)
                AcceptPicker.Items.Add(acceptType);

            AcceptPicker.SelectedIndex = 0;

            foreach(var messageType in MessageTypeList.Keys)
                MessageTypePicker.Items.Add(messageType);

            MessageTypePicker.SelectedIndex = 0;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            IsPresented = true;

            _coapService.NewDevice += OnNewDevice;
            _coapService.MessageReceived += OnCoapMessageReceived;
        }

        protected override void OnDisappearing()
        {
            _coapService.NewDevice -= OnNewDevice;
            _coapService.MessageReceived -= OnCoapMessageReceived;
        }

        private void OnCoapMessageReceived(object sender, CoapMessageEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                MessageLog.Spans.Add(new Span
                {
                    Text = string.Format("Received message from {0} - {1}\n", CoapDevice.HostName, e.Message),
                    ForegroundColor = Color.LightSkyBlue
                });
                if (e.Message.Options.Contains<CoAP.Net.Options.ContentFormat>())
                {
                    var contentType = e.Message.Options.Get<CoAP.Net.Options.ContentFormat>().MediaType;
                    if (new[] { CoAP.Net.Options.ContentFormatType.TextPlain, CoAP.Net.Options.ContentFormatType.ApplicationJson, CoAP.Net.Options.ContentFormatType.ApplicationXml, CoAP.Net.Options.ContentFormatType.ApplicationLinkFormat }.Contains(contentType))
                    {
                        MessageLog.Spans.Add(new Span
                        {
                            Text = Encoding.UTF8.GetString(e.Message.Payload) + "\n"
                        });
                    }
                    else
                    {
                        MessageLog.Spans.Add(new Span
                        {
                            Text = string.Format("Binary Data ({0} bytes)\n", e.Message.Payload.Length)
                        });
                    }
                }
                else if ((e.Message.Payload?.Length ?? 0) > 0)
                {
                    // Assume text/plain UTF-8
                    MessageLog.Spans.Add(new Span
                    {
                        Text = Encoding.UTF8.GetString(e.Message.Payload) + "\n"
                    });
                }
            });
        }

        private void OnNewDevice(object sender, NewCoapDeviceEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("For new device evvent for {0}", e.CoapDevice.HostName));
        }

        private void DoThatThing(CommandResourceType action)
        {
            var message = CoapMessage.CreateFromUri(ResourceUri);
            message.Code = (CoapMessageCode)action;

            if (AcceptPicker.SelectedIndex > 0)
                message.Options.Add(new CoAP.Net.Options.Accept(AcceptList[(string)AcceptPicker.SelectedItem]));

            message.Type = MessageTypeList[(string)MessageTypePicker.SelectedItem];

            _coapService.SendMessage(CoapDevice, message);

            MessageLog.Spans.Add(new Span {
                Text = string.Format("Sending Message to {0}: {1}\n", CoapDevice.HostName, message),
                ForegroundColor = Color.PaleVioletRed
            });
        }

        private void OnDeviceResouceSelected(object sender, SelectedItemChangedEventArgs e)
        {
            _executeResourceCommand?.ChangeCanExecute();
            if(MasterBehavior == MasterBehavior.Popover)
                IsPresented = false;

            if (e.SelectedItem != null)
            {
                ResourceUri = "coap://" + CoapDevice.HostName + ((CoapResource)e.SelectedItem).URIReference;
            }
        }
    }
}

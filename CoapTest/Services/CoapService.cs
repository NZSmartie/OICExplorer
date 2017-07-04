using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

using CoapTest.Extensions;
using CoapTest.Models;

using CoAP.Net;
using Xamarin.Forms;

[assembly: Dependency(typeof(CoapTest.Services.CoapService))]
namespace CoapTest.Services
{

    public class CoapMessageEventArgs : EventArgs
    {
        public IDevice Device { get; set; }

        public CoapMessage Message { get; set; }
    }

    public class NewCoapDeviceEventArgs : EventArgs
    {
        public IDevice CoapDevice;
    }

    public class CoapService : IDeviceService
    {
        public const string MessageSend = "MessageSend";

        public readonly Dictionary<string, CoAP.Net.Options.ContentFormatType> MediaList = new Dictionary<string, CoAP.Net.Options.ContentFormatType>
        {
            {"text/plain", CoAP.Net.Options.ContentFormatType.TextPlain },
            {"application/link-format", CoAP.Net.Options.ContentFormatType.ApplicationLinkFormat },
            {"application/xml", CoAP.Net.Options.ContentFormatType.ApplicationXml },
            {"application/octet-stream", CoAP.Net.Options.ContentFormatType.ApplicationOctetStream },
            {"application/exi", CoAP.Net.Options.ContentFormatType.ApplicationExi },
            {"application/json", CoAP.Net.Options.ContentFormatType.ApplicationJson },
            {"applicaiton/cbor",CoAP.Net.Options.ContentFormatType.ApplicationCbor },
        };

        private readonly UdpEndPoint _localEndpoint;
        private readonly CoapClient _coapClient;
        private readonly List<IPAddress> _localAddresses = new List<IPAddress>();

        public List<CoapDevice> CoapDevices = new List<CoapDevice>();

        public event EventHandler<NewCoapDeviceEventArgs> NewDevice;

        public event EventHandler<CoapMessageEventArgs> MessageReceived;

        private Task _addressTask;

        public CoapService()
        {
            _localEndpoint = new UdpEndPoint(new IPEndPoint(IPAddress.Any, 0));
            _localEndpoint.Bind();
            _coapClient = new CoapClient(_localEndpoint);

            _coapClient.OnMessageReceived += CoapOnMessageReceived;

            _coapClient.Listen();

            _addressTask = Task.Run(async () =>
            {
                _localAddresses.AddRange(await Dns.GetHostAddressesAsync(Dns.GetHostName()));
            });
        }

        

        public async void SendMessage(IDevice device, CoapMessage message)
        {
            var udpDevice = device as Device<UdpEndPoint> ??
                            throw new ArgumentException($"{nameof(device)} is not of CoapDevice<UdpEndPoint>");

            await _coapClient.SendAsync(message, udpDevice.EndPoint);
        }

        private void CoapOnMessageReceived(object sender, CoapMessageReceivedEventArgs e)
        {
            var device = CoapDevices.FirstOrDefault(d => d.EndPoint == e.Endpoint);
            bool newDevice = false;
            if (device == null)
            {
                newDevice = true;
                device = new CoapDevice(e.Endpoint as UdpEndPoint);
                CoapDevices.Add(device);
            }

            var contentType = e.Message.Options.Get<CoAP.Net.Options.ContentFormat>();
            if(contentType != null)
            {
                if (contentType.MediaType == CoAP.Net.Options.ContentFormatType.ApplicationLinkFormat)
                    device.Resources = CoreLinkFormat.Parse(System.Text.Encoding.UTF8.GetString(e.Message.Payload))
                        .Select(r => new CoapResourceWrapper(r) as IDeviceResource).ToList();
            }

            System.Diagnostics.Debug.WriteLine("Received message from {0}\n\t- Length: {1}\n\t- Payload: {2}",
                e.Endpoint.ToString(),
                e.Message.Payload?.Length ?? 0,
                e.Message.Payload != null ? System.Text.Encoding.UTF8.GetString(e.Message.Payload) : "");

            if (newDevice)
                NewDevice?.Invoke(this, new NewCoapDeviceEventArgs { CoapDevice = device });

            MessageReceived?.Invoke(this, new CoapMessageEventArgs { Device = device, Message = e.Message });
        }

        /// <summary>
        /// Sends out a simple GET request via multicast. Any supported device will eventually respond
        /// </summary>
        /// <remarks>
        /// Servers replying to multicast messages may wait a random leisure period spanning from a few sends up to a minute.
        /// Discovery isn't a fast process. The default leisure period is 5 seconds
        /// </remarks>
        public async void Discover()
        {
            await _addressTask;

            var message = new CoapMessage
            {
                Type = CoapMessageType.NonConfirmable,
                Code = CoapMessageCode.Get,
                Options = new List<CoapOption>
                {
                    new CoAP.Net.Options.Accept(CoAP.Net.Options.ContentFormatType.ApplicationLinkFormat),
                    new CoAP.Net.Options.UriPath(".well-known"),
                    new CoAP.Net.Options.UriPath("core"),
                }
            };
            using (var endpoint = new UdpEndPoint(new IPEndPoint(IPAddress.Parse(CoAP.Net.Consts.MulticastIPv4), CoAP.Net.Consts.Port)))
            {
                foreach (var address in _localAddresses)
                {
                    // Todo: Support IPv6 Multicasting
                    if (address.AddressFamily != AddressFamily.InterNetwork)
                        continue;
                    try
                    {
                        _localEndpoint.Client.Client.SetSocketOption(
                            SocketOptionLevel.IP,
                            SocketOptionName.MulticastInterface,
                            address.GetAddressBytes());

                        await _coapClient.SendAsync(message, endpoint);
                    }
                    catch(SocketException se)
                    {
                        System.Diagnostics.Debug.WriteLine($"{se.GetType().FullName} occured with message {se.Message}");
                    }
                }
            }
        }

        public void Stop()
        {
            _coapClient.Dispose();
        }
    }
}

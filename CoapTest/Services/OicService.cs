using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using CoAPNet;
using OICNet;
using OICNet.CoAP;

using Xamarin.Forms;
using CoAPNet.Udp;

[assembly: Dependency(typeof(CoapTest.Services.OicService))]
namespace CoapTest.Services
{

    public class OicMessageEventArgs : EventArgs
    {
        public OicDevice Device { get; set; }

        public OicResponse Message { get; set; }
    }

    public class NewOicDeviceEventArgs : EventArgs
    {
        public OicDevice Device;
    }

    public class OicService
    {
        public const string MessageSend = "MessageSend";

        private readonly OicClient _client;

        private readonly OicResourceDiscoverClient _resourceDiscoveryClient;

        public List<OicDevice> Devices = new List<OicDevice>();

        public event EventHandler<NewOicDeviceEventArgs> NewDevice;

        public OicService()
        {
            _client = new OicClient();

            _resourceDiscoveryClient = new OicResourceDiscoverClient(_client);
            _resourceDiscoveryClient.NewDevice += OnNewDevice;

            Task.Run(async () =>
            {
                var addresses = await Dns.GetHostAddressesAsync(Dns.GetHostName());

                foreach (var address in addresses)
                {
                    // Todo: Support IPv6 Multicasting
                    if (address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    var localEndpoint = new CoapUdpEndPoint(address);
                    await localEndpoint.BindAsync();
                    localEndpoint.Client.Client.SetSocketOption(
                        SocketOptionLevel.IP,
                        SocketOptionName.MulticastInterface,
                        address.GetAddressBytes());

                    _client.AddTransport(new OicCoapTransport(localEndpoint));
                }
            });
        }

        public IOicResourceRepository GetRepository(OicDevice device)
        {
            return new OicRemoteResourceRepository(device as OicRemoteDevice, _client);
        }

        private void OnNewDevice(object sender, OicNewDeviceEventArgs e)
        {
            Devices.Add(e.Device);
            NewDevice?.Invoke(this, new NewOicDeviceEventArgs { Device = e.Device });
        }

        private void OnReceivedMessage(object sender, OicDeviceReceivedMessageEventArgs e)
        {
            //var device = Devices.FirstOrDefault(d => (d as OicDevice)?.Endpoint == e.Device.Endpoint);

            //var contentType = e.Message.Options.Get<CoAP.Net.Options.ContentFormat>();
            //if (contentType != null)
            //{
            //    if (contentType.MediaType == CoAP.Net.Options.ContentFormatType.ApplicationLinkFormat)
            //        device.Resources = CoreLinkFormat.Parse(Encoding.UTF8.GetString(e.Message.Payload))
            //            .Select(r => new OicResourceWrapper(r) as IDeviceResource).ToList();
            //}

            //System.Diagnostics.Debug.WriteLine("Received message from {0}\n\t- Length: {1}",
            //    e.Device.Endpoint.Authority,
            //    e.Message.Payload?.Length ?? 0);

            //MessageReceived?.Invoke(this, new CoapMessageEventArgs { Device = device, Message = e.Message });
        }

        /// <summary>
        /// Sends out a simple GET request via multicast. Any supported device will eventually respond
        /// </summary>
        /// <remarks>
        /// Servers replying to multicast messages may wait a random leisure period spanning from a few sends up to a minute.
        /// Discovery isn't a fast process. The default leisure period is 5 seconds
        /// </remarks>
        public async Task Discover()
        {
            await _resourceDiscoveryClient.DiscoverAsync(true);
        }

        public void Stop()
        {
            _resourceDiscoveryClient.Dispose();
            _client.Dispose();
        }
    }
}

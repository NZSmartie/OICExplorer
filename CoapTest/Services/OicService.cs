using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Splat;

using CoAPNet.Udp;
using OICNet;
using OICNet.CoAP;

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

        private readonly ILogger<OicService> _logger;

        public event EventHandler<NewOicDeviceEventArgs> NewDevice;

        public ObservableCollection<OicRemoteDevice> Devices => _resourceDiscoveryClient.Devices;

        public OicService(ILogger<OicService> logger = null)
        {
            _logger = logger ?? Locator.Current.GetLogger<OicService>();

            _client = new OicClient(Locator.Current.GetLogger<OicClient>());

            _resourceDiscoveryClient = new OicResourceDiscoverClient(_client, Locator.Current.GetLogger<OicResourceDiscoverClient>());
            _resourceDiscoveryClient.NewDevice += OnNewDevice;

            Task.Run(async () =>
            {
                var addresses = await Dns.GetHostAddressesAsync(Dns.GetHostName());

                foreach (var address in addresses)
                {
                    // Todo: Support IPv6 Multicasting
                    if (address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    _logger?.LogInformation($"Binding to local interface ({address})");
                    _client.AddTransport(new OicCoapTransport(new CoapUdpEndPoint(address, logger: Locator.Current.GetLogger<CoapUdpEndPoint>())));
                }
            });
        }

        public IOicResourceRepository GetRepository(OicDevice device)
        {
            return new OicRemoteResourceRepository(device as OicRemoteDevice, _client);
        }

        private void OnNewDevice(object sender, OicNewDeviceEventArgs e)
        {
            NewDevice?.Invoke(this, new NewOicDeviceEventArgs { Device = e.Device });
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
            _logger?.LogDebug("Sending discovery request and clearing device cache");
            await _resourceDiscoveryClient.DiscoverAsync(clearCached: true);
        }

        public void Stop()
        {
            _resourceDiscoveryClient.Dispose();
            _client.Dispose();
        }
    }
}

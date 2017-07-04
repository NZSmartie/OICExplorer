using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using CoAP.Net;

namespace CoapTest.Services
{
    public class UdpEndPoint : ICoapEndpoint, IDisposable
    {
        private UdpClient _udpClient;
        private readonly IPEndPoint _endpoint;
        private readonly IPAddress _multicastAddress = IPAddress.Parse(CoAP.Net.Consts.MulticastIPv4);

        public IPEndPoint Endpoint { get { return (IPEndPoint)_udpClient?.Client.LocalEndPoint ?? _endpoint; } }

        public UdpClient Client { get { return _udpClient; } }

        public bool CanReceive
        {
            get
            {
                return _udpClient != null && _udpClient.Client.LocalEndPoint != null;
            }
        }

        private bool _isMulticast = false;

        public bool IsMulticast { get => _isMulticast; private set => _isMulticast = value; }

        public bool IsSecure => false;

        public UdpEndPoint(UdpClient udpClient)
        {
            _udpClient = udpClient ?? throw new ArgumentNullException("udpClient");
            _endpoint = (IPEndPoint)_udpClient.Client.LocalEndPoint;
        }

        public UdpEndPoint(IPEndPoint endpoint)
        {
            _endpoint = endpoint ?? throw new ArgumentNullException("endpoint");
        }

        public void Bind()
        {
            if (_udpClient == null)
                _udpClient = new UdpClient(_endpoint) { EnableBroadcast = true };
        }

        public void Dispose()
        {
            if(_udpClient != null)
                _udpClient.Dispose();
        }

        public async Task<CoapPayload> ReceiveAsync()
        {
            if (_udpClient == null)
                Bind();

            var result = await _udpClient.ReceiveAsync();
            return new CoapPayload
            {
                Payload = result.Buffer,
                Endpoint = new UdpEndPoint(result.RemoteEndPoint),
            };
        }

        public async Task SendAsync(CoapPayload payload)
        {
            if (_udpClient == null)
                Bind();

            var udpDestEndpoint = payload.Endpoint as UdpEndPoint;
            if (udpDestEndpoint == null)
                throw new ArgumentException();

            try
            {
                await _udpClient.SendAsync(payload.Payload, payload.Payload.Length, udpDestEndpoint.Endpoint);
            }
            catch(SocketException se)
            {
                Debug.WriteLine($"Failed to send data. {se.GetType().FullName} (0x{se.HResult:x}): {se.Message}");
            }
        }

        public override string ToString()
        {
            return _endpoint.ToString();
        }
    }
}

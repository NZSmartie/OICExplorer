using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CoAP.Net;

using CoapTest.Extensions;
using CoapTest.Services;

namespace CoapTest.Models
{
    public class CoapDevice : INotifyPropertyChanged
    {
        private readonly UdpEndPoint _endPoint;
        private string _hostName;

        public event PropertyChangedEventHandler PropertyChanged;

        public UdpEndPoint EndPoint { get => _endPoint; }
        public string HostName
        {
            get => _hostName ?? _endPoint.Endpoint.Address.ToString();
            set
            {
                _hostName = value;
                PropertyChanged?.Invoke(this, () => HostName);
            }
        }

        public List<CoapResource> Resources { get; set; }

        public CoapDevice(UdpEndPoint udpEndPoint)
        {
            _endPoint = udpEndPoint ?? throw new ArgumentNullException("udpEndPoint");
        }
    }
}

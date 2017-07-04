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
    public interface IDevice
    {
        List<IDeviceResource> Resources { get; set; }

        string BaseURI { get; }

        string Name { get; }
    }

    public interface IDeviceResource
    {
        string URIReference { get; }

        string Media { get; }

        string Name { get; }

        string Type { get; }

        List<string> ResourceTypes { get; }

        List<string> InterfaceDescription { get; }
}

    public class Device<TEndpoint> : INotifyPropertyChanged, IDevice where TEndpoint : class
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public TEndpoint EndPoint { get; }

        public virtual string BaseURI => null;

        public virtual string Name => null;

        public List<IDeviceResource> Resources { get; set; }

        public Device(TEndpoint udpEndPoint)
        {
            EndPoint = udpEndPoint ?? throw new ArgumentNullException(nameof(udpEndPoint));
        }
    }
}

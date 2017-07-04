using System.Collections.Generic;
using CoapTest.Models;

using CoAP.Net;
using Xamarin.Forms;

[assembly: Dependency(typeof(CoapTest.Services.CoapService))]
namespace CoapTest.Services
{
    public class CoapResourceWrapper : IDeviceResource
    {
        public string URIReference => _resource.URIReference;

        public string Media => _resource.Media;

        public string Name => _resource.Title;

        public string Type => _resource.Type;

        public List<string> ResourceTypes => _resource.ResourceTypes;

        public List<string> InterfaceDescription => _resource.InterfaceDescription;

        private readonly CoapResource _resource;

        public CoapResourceWrapper(CoapResource resource)
        {
            _resource = resource;
        }
    }
}

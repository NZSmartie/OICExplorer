using System.Linq;
using System.Text;
using Xamarin.Forms;

using CoapTest.Models;

[assembly: Dependency(typeof(CoapTest.Services.CoapService))]
namespace CoapTest.Services
{
    public class CoapDevice : Device<UdpEndPoint>
    {
        public override string BaseURI { get; }

        public override string Name => EndPoint.Endpoint.Address.ToString();

        public CoapDevice(UdpEndPoint udpEndPoint) : base(udpEndPoint)
        {
            var baseUriBuilder = new StringBuilder()
                .Append(udpEndPoint.IsSecure ? "coaps://" : "coap://")
                .Append(udpEndPoint.Endpoint.Address);

            if (udpEndPoint.Endpoint.Port != CoAP.Net.Consts.PortDTLS && udpEndPoint.Endpoint.Port != CoAP.Net.Consts.Port)
                baseUriBuilder.Append($":{udpEndPoint.Endpoint.Port}");

            BaseURI = baseUriBuilder.Append("/").ToString();
        }
    }
}

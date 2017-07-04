using System;
using CoAP.Net;
using CoapTest.Models;

namespace CoapTest.Services
{
    public enum DeviceMessageType
    {
        Confirmable,
        NonConfirmable,
        Acknowledgement,
        Reset
    };

    public interface IDeviceService
    {
        event EventHandler<CoapMessageEventArgs> MessageReceived;
        event EventHandler<NewCoapDeviceEventArgs> NewDevice;

        void Discover();
        void SendMessage(IDevice device, CoapMessage message);
        void Stop();
    }
}
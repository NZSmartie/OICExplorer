using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;
using Splat;

using OICNet;

namespace CoapTest.ViewModels
{
    public class DeviceCellViewModel : ReactiveObject, IRoutableViewModel, ISupportsActivation
    {
        public OicDevice Device { get; }

        public string Name => Device.Name;

        public Guid DeviceId => Device.DeviceId;

        private ObservableAsPropertyHelper<int> _resources;
        public int Resources => _resources.Value;

        public string UrlPathSegment => null;

        public IScreen HostScreen { get; }

        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        public DeviceCellViewModel(OicDevice device, IScreen hostScreen = null)
        {
            Device = device;
            HostScreen = hostScreen ?? Locator.Current.GetService<IScreen>();

            this.WhenActivated((CompositeDisposable disposables) =>
            {
                _resources =
                    this.WhenAnyValue(x => x.Device.Resources)
                        .Select(r => r.Count)
                        .ToProperty(this, r => r.Resources)
                        .DisposeWith(disposables);
            });
        }
    }
}

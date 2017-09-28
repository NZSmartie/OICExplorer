using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using ReactiveUI;
using Splat;

using OICNet;

using CoapTest.Services;

namespace CoapTest.ViewModels
{
    public class DevicesViewModel : ReactiveObject, IRoutableViewModel, ISupportsActivation
    {
        private readonly OicService _oicService;

        public IReactiveDerivedList<OicDevice> Devices { get; private set; }

        private OicDevice _selectedDevice;
        public OicDevice SelectedDevice
        {
            get => _selectedDevice;
            set => this.RaiseAndSetIfChanged(ref _selectedDevice, value);
        }

        public ReactiveCommand DiscoverDevicesCommand { get; private set; }

        private ObservableAsPropertyHelper<bool> _isRefreshing;
        public bool IsRefreshing => _isRefreshing.Value;

        public string UrlPathSegment => "Devices";

        public IScreen HostScreen { get; }

        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        public DevicesViewModel(OicService oicService = null, IScreen homeScreen = null)
        {
            _oicService = oicService ?? Locator.Current.GetService<OicService>();
            HostScreen = homeScreen ?? Locator.Current.GetService<IScreen>();


            DiscoverDevicesCommand = 
                ReactiveCommand.CreateFromObservable(() => Observable.FromAsync(_oicService.Discover));

            this.WhenActivated((CompositeDisposable disposables) =>
            {
                _isRefreshing =
                    DiscoverDevicesCommand
                        .IsExecuting
                        .Select(x => x)
                        .ToProperty(this, x => x.IsRefreshing, true)
                        .DisposeWith(disposables);

                Devices = _oicService.Devices
                            .CreateDerivedCollection(x => (OicDevice)x, scheduler: RxApp.MainThreadScheduler)
                            .DisposeWith(disposables);
            });
        }
    }
}

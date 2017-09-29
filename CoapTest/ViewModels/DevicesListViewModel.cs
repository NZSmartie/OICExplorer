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
    public class DevicesListViewModel : ReactiveObject, IRoutableViewModel, ISupportsActivation
    {
        private readonly OicService _oicService;

        public IReactiveDerivedList<DeviceViewModel> Devices { get; private set; }

        private DeviceViewModel _selectedDevice;
        public DeviceViewModel SelectedDevice
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

        public DevicesListViewModel(OicService oicService = null, IScreen homeScreen = null)
        {
            _oicService = oicService ?? Locator.Current.GetService<OicService>();
            HostScreen = homeScreen ?? Locator.Current.GetService<IScreen>();


            DiscoverDevicesCommand = 
                ReactiveCommand.CreateFromObservable(() => Observable.FromAsync(_oicService.Discover));

            this.WhenActivated((CompositeDisposable disposables) =>
            {
                SelectedDevice = null;

                _isRefreshing =
                    DiscoverDevicesCommand
                        .IsExecuting
                        .Select(x => x)
                        .ToProperty(this, x => x.IsRefreshing, true)
                        .DisposeWith(disposables);

                Devices = _oicService.Devices
                            .CreateDerivedCollection(x => new DeviceViewModel(x), scheduler: RxApp.MainThreadScheduler)
                            .DisposeWith(disposables);

                this.WhenAnyValue(x => x.SelectedDevice)
                    .Where(d => d != null)
                    .Subscribe(d => LoadSelectedDevice(d))
                    .DisposeWith(disposables);

            });
        }

        private void LoadSelectedDevice(DeviceViewModel device)
        {
            HostScreen
                .Router
                .Navigate
                .Execute(device)
                .Subscribe();
        }
    }
}

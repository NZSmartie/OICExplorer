using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using ReactiveUI;
using Splat;

using OICNet;

using OICExplorer.Services;

namespace OICExplorer.ViewModels
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

        DeviceViewModel _deviceAppearing;
        public DeviceViewModel DeviceAppearing
        {
            get { return _deviceAppearing; }
            set { this.RaiseAndSetIfChanged(ref _deviceAppearing, value); }
        }

        public ReactiveCommand<bool, Unit> DiscoverDevicesCommand { get; private set; }

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
                ReactiveCommand.CreateFromTask<bool>(_oicService.Discover);

            this.WhenActivated((CompositeDisposable disposables) =>
            {
                // Ensure no device is selected, otherwise NavigateToDevice() will be invoked.
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
                    .Subscribe(d => NavigateToDevice(d))
                    .DisposeWith(disposables);

                // Automatically scan for devices every 30 seconds
                Observable
                    .Timer(TimeSpan.Zero, TimeSpan.FromSeconds(30))
                    .Select(_ => false)
                    .InvokeCommand(DiscoverDevicesCommand)
                    .DisposeWith(disposables);
            });
        }

        private void NavigateToDevice(DeviceViewModel device)
        {
            HostScreen
                .Router
                .NavigateToModal()
                .Execute(device)
                .Subscribe();
        }
    }
}

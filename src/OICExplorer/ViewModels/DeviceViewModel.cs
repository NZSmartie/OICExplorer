using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;
using Splat;

using OICNet;

namespace OICExplorer.ViewModels
{
    public class DeviceViewModel : ReactiveObject, IRoutableViewModel, ISupportsActivation
    {
        private readonly OicDevice _device;

        public string Name => _device.Name;

        public Guid DeviceId => _device.DeviceId;

        public ReactiveList<ResourceCellViewModel> Resources { get; } 
            = new ReactiveList<ResourceCellViewModel>();

        private ObservableAsPropertyHelper<int> _resourceCount;
        public int ResourceCount => _resourceCount.Value;


        //private FormattedString _messageLog = new FormattedString();
        //public FormattedString MessageLog
        //{
        //    get => _messageLog;
        //    set => this.RaiseAndSetIfChanged(ref _messageLog, value);
        //}

        //private string _resourceUri = "";
        //public string ResourceUri
        //{
        //    get => _resourceUri; set
        //    {
        //        _resourceUri = value;
        //        PropertyChanged?.Invoke(this, () => ResourceUri);
        //    }
        //}

        //public IOicResource SelectedResource => ((IOicResource) DeviceResoucesListView.SelectedItem);

        //private OicDevice _device;
        //public OicDevice OicDevice
        //{
        //    get => _device;
        //    set
        //    {
        //        _device = value;
        //        _repository = _coapService.GetRepository(_device);
        //        PropertyChanged?.Invoke(this, () => OicDevice);
        //    }
        //}

        //private IOicResourceRepository _repository;

        //public IOicResourceRepository Repository { get => _repository; } 

        //private async void DoThatThing(OicRequestOperation method)
        //{
        //    if (SelectedResource == null)
        //        return;

        //    var request = OicRequest.Create(SelectedResource.RelativeUri);
        //    var response = await Repository.RetrieveAsync(request);

        //    SelectedResource.UpdateFields(response.Resource);
        //}

        //private void OnDeviceResouceSelected(object sender, SelectedItemChangedEventArgs e)
        //{
        //    _executeResourceCommand?.ChangeCanExecute();
        //    if (MasterBehavior == MasterBehavior.Popover)
        //        IsPresented = false;

        //    if (e.SelectedItem == null)
        //        return;

        //    ResourceUri = new UriBuilder
        //    {
        //        Scheme = "oic",
        //        Host = (OicDevice as OicRemoteDevice).Endpoint.Authority,
        //        Path = SelectedResource.RelativeUri
        //    }.ToString();
        //}

        //private void Button_Clicked(object sender, EventArgs e)
        //{
        //    Navigation.PushPopupAsync(new Dialogs.ColourPickerDialog());
        //}

        public string UrlPathSegment => Name;

        public IScreen HostScreen { get; }

        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        public DeviceViewModel(OicDevice device, IScreen hostScreen = null)
        {
            _device = device;

            HostScreen = hostScreen ?? Locator.Current.GetService<IScreen>();

            this.WhenActivated((CompositeDisposable disposables) =>
            {
                this.WhenAnyValue(x => x._device.Resources)
                    .Select(r => r.Select(x => new ResourceCellViewModel(x)))
                    .SelectMany(r => r)
                    .Subscribe(r => Resources.Add(r))
                    .DisposeWith(disposables);

                _resourceCount =
                    this.WhenAnyValue(x => x._device.Resources)
                        .Select(r => r.Count)
                        .ToProperty(this, r => r.ResourceCount)
                        .DisposeWith(disposables);

            });
        }
    }
}

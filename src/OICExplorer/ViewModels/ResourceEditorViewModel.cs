using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;
using Splat;

using OICNet;
using System.Threading.Tasks;

namespace OICExplorer.ViewModels
{
    public class ResourceEditorViewModel : ReactiveObject, IRoutableViewModel, ISupportsActivation
    {
        private readonly IOicResourceRepository _repository;

        private readonly IOicResource _resource;

        public string Name => _resource.Name;

        public string RelativeUri { get; set; }

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

        public enum ResourceOperation
        {
            Retreive,
            CreateOrUpdate,
            Create,
            Delete,
        }

        private async Task<IOicResource> PerformOperationAsync(ResourceOperation operation)
        {
            var request = OicRequest.Create(RelativeUri);

            OicResponse response;
            switch (operation)
            {
                case ResourceOperation.Retreive:
                    response = await _repository.RetrieveAsync(request);
                    break;
                case ResourceOperation.Create:
                    response = await _repository.CreateAsync(request, _resource);
                    break;
                case ResourceOperation.CreateOrUpdate:
                    response = await _repository.CreateOrUpdateAsync(request, _resource);
                    break;
                case ResourceOperation.Delete:
                    // TODO: what will happen on success? do we update our parent device and then destroy this view model?
                    response = await _repository.DeleteAsync(request);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return response.Resource;
        }

        public ReactiveCommand RetreiveResourceCommand { get; }
        public ReactiveCommand CreateResourceCommand { get; }
        public ReactiveCommand CreateOrUpdateResourceCommand { get; }
        public ReactiveCommand DeleteResourceCommand { get; }

        //private void Button_Clicked(object sender, EventArgs e)
        //{
        //    Navigation.PushPopupAsync(new Dialogs.ColourPickerDialog());
        //}

        public string UrlPathSegment => Name;

        public IScreen HostScreen { get; }

        public ViewModelActivator Activator { get; } = new ViewModelActivator();

        public ResourceEditorViewModel(IOicResourceRepository repository, IOicResource resource, IScreen hostScreen = null)
        {
            _repository = repository;
            _resource = resource;

            RelativeUri = _resource.RelativeUri;

            HostScreen = hostScreen ?? Locator.Current.GetService<IScreen>();

            RetreiveResourceCommand = ReactiveCommand.CreateFromTask(async () => await PerformOperationAsync(ResourceOperation.Retreive));
            CreateResourceCommand = ReactiveCommand.CreateFromTask(async () => await PerformOperationAsync(ResourceOperation.Create));
            CreateOrUpdateResourceCommand = ReactiveCommand.CreateFromTask(async () => await PerformOperationAsync(ResourceOperation.CreateOrUpdate));
            DeleteResourceCommand = ReactiveCommand.CreateFromTask(async () => await PerformOperationAsync(ResourceOperation.Delete));

            this.WhenActivated((CompositeDisposable disposables) =>
            {
                
            });
        }
    }
}

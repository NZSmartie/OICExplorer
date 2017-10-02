using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;
using Splat;

using OICNet;
using OICExplorer.Services;

namespace OICExplorer.ViewModels
{
    public class DeviceViewModel : ReactiveObject, IRoutableViewModel, ISupportsActivation
    {
        private readonly OicDevice _device;

        private readonly OicService _oicService;

        public string Name => _device.Name;

        public Guid DeviceId => _device.DeviceId;

        public ReactiveList<ResourceCellViewModel> Resources { get; } 
            = new ReactiveList<ResourceCellViewModel>();

        private ResourceCellViewModel _selectedResource;
        public ResourceCellViewModel SelectedResource
        {
            get => _selectedResource;
            set => this.RaiseAndSetIfChanged(ref _selectedResource, value);
        }

        private ObservableAsPropertyHelper<ResourceEditorViewModel> _activeResource;
        public ResourceEditorViewModel ActiveResource => _activeResource.Value;

        private ObservableAsPropertyHelper<int> _resourceCount;
        public int ResourceCount => _resourceCount.Value;


        //private FormattedString _messageLog = new FormattedString();
        //public FormattedString MessageLog
        //{
        //    get => _messageLog;
        //    set => this.RaiseAndSetIfChanged(ref _messageLog, value);
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
            _oicService = Locator.Current.GetService<OicService>();
            _device = device;

            HostScreen = hostScreen ?? Locator.Current.GetService<IScreen>();

            this.WhenActivated((CompositeDisposable disposables) =>
            {
                SelectedResource = null;

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

                _activeResource =
                    this.WhenAnyValue(vm => vm.SelectedResource)
                        .Where(r => r != null)
                        .Select(r => new ResourceEditorViewModel(_oicService.GetRepository(_device), r.Resource))
                        .ToProperty(this, r => r.ActiveResource)
                        .DisposeWith(disposables);

            });
        }
    }
}

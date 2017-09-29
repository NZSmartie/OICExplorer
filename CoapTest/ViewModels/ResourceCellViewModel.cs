using System.Reactive.Disposables;
using OICNet;
using ReactiveUI;

namespace CoapTest.ViewModels
{
    public class ResourceCellViewModel : ReactiveObject
    {
        private readonly IOicResource _resource;

        // TODO: Image property

        public string Name => _resource.Name;

        public string RelativeUri => _resource.RelativeUri;

        public ResourceCellViewModel(IOicResource resource)
        {
            _resource = resource;
        }
    }
}

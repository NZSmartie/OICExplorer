using System.Reactive.Disposables;
using OICNet;
using ReactiveUI;

namespace OICExplorer.ViewModels
{
    public class ResourceCellViewModel : ReactiveObject
    {
        public IOicResource Resource { get; }

        // TODO: Image property

        public string Name => Resource.Name;

        public string RelativeUri => Resource.RelativeUri;

        public ResourceCellViewModel(IOicResource resource)
        {
            Resource = resource;
        }
    }
}

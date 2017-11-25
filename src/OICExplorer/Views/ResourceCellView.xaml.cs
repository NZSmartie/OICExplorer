using System.Reactive.Disposables;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ReactiveUI;
using ReactiveUI.XamForms;

using OICExplorer.ViewModels;

namespace OICExplorer.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ResourceCellView : ReactiveViewCell<ResourceCellViewModel>
    {
        protected readonly CompositeDisposable SubscriptionDisposables = new CompositeDisposable();

        public ResourceCellView()
        {
            InitializeComponent();

            // TODO: Move this to  helper class
            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    ResourceThumbnail.Source = ImageSource.FromFile("blank.png");
                    break;
                default:
                    ResourceThumbnail.Source = ImageSource.FromFile("Assets/blank.png");
                    break;
            }

            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel, vm => vm.Name, v => v.ResourceName.Text)
                    .DisposeWith(SubscriptionDisposables);

                this.OneWayBind(ViewModel, vm => vm.RelativeUri, v => v.ResourceUri.Text)
                    .DisposeWith(SubscriptionDisposables);
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            SubscriptionDisposables.Clear();
        }
    }
}
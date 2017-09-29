using ReactiveUI;
using ReactiveUI.XamForms;
using System.Reactive.Disposables;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using OICExplorer.ViewModels;

namespace OICExplorer.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DeviceCellView : ReactiveViewCell<DeviceViewModel>
	{
        protected readonly CompositeDisposable SubscriptionDisposables = new CompositeDisposable();

        public DeviceCellView()
		{
            InitializeComponent();

            // TODO: Move this to  helper class
            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    Thumbnail.Source = ImageSource.FromFile("node.png");
                    break;
                default:
                    Thumbnail.Source = ImageSource.FromFile("Assets/node.png");
                    break;
            }

            this.WhenActivated(disposables =>
            {   
                this.OneWayBind(ViewModel, vm => vm.Name, v => v.HostName.Text)
                    .DisposeWith(SubscriptionDisposables);

                this.OneWayBind(ViewModel, vm => vm.ResourceCount, v => v.Resources.Text, x => $"{x} Resource{(x!=1?"s":"")}")
                    .DisposeWith(SubscriptionDisposables);
            });
		}
	}
}
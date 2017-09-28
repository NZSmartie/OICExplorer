using CoapTest.Converters;
using CoapTest.ViewModels;
using ReactiveUI;
using ReactiveUI.XamForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CoapTest.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DeviceCellView : ReactiveViewCell<DeviceCellViewModel>
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

                this.OneWayBind(ViewModel, vm => vm.Resources, v => v.Resources.Text, x => $"{x} Resource{(x!=1?"s":"")}")
                    .DisposeWith(SubscriptionDisposables);
            });
		}
	}
}
using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Disposables;
using ReactiveUI;
using ReactiveUI.XamForms;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Rg.Plugins.Popup.Extensions;

using CoapTest.Extensions;
using CoapTest.Services;
using Newtonsoft.Json;
using OICNet;
using CoapTest.ViewModels;

namespace CoapTest
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DeviceView : ReactiveMasterDetailPage<DeviceViewModel>
    {
        public DeviceView()
        {
            InitializeComponent();

            // TODO: Move this to  helper class
            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    DeviceThumbnail.Source = ImageSource.FromFile("node.png");
                    break;
                default:
                    DeviceThumbnail.Source = ImageSource.FromFile("Assets/node.png");
                    break;
            }

            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel, vm => vm.Name, v => v.DeviceHostName.Text)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel, vm => vm.ResourceCount, v => v.DeviceResources.Text, x => $"{x} Resource{(x != 1 ? "s" : "")}")
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel, vm => vm.Resources, v => v.ResoucesListView.ItemsSource)
                    .DisposeWith(disposables);
            });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            IsPresented = true;
        }
    }
}

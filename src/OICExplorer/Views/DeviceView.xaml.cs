using System;
using System.Linq;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using ReactiveUI;
using ReactiveUI.XamForms;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

//using Rg.Plugins.Popup.Extensions;

using OICExplorer.Extensions;
using OICExplorer.Services;
using Newtonsoft.Json;
using OICNet;
using OICExplorer.ViewModels;

namespace OICExplorer
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

                this.OneWayBind(ViewModel, vm => vm.Resources, v => v.ResourcesListView.ItemsSource)
                    .DisposeWith(disposables);

                this.Bind(ViewModel, vm => vm.SelectedResource, v => v.ResourcesListView.SelectedItem)
                    .DisposeWith(disposables);

                ViewModel.WhenAnyValue(vm => vm.SelectedResource)
                         .Where(_ => MasterBehavior == MasterBehavior.Popover)
                         .Subscribe(r => IsPresented = r != null)
                         .DisposeWith(disposables);

                this.OneWayBind(ViewModel, vm => vm.ActiveResource, v => v.ResourceViewModel.ViewModel)
                    .DisposeWith(disposables);
            });
        }

        protected override bool OnBackButtonPressed()
        {
            if(Device.RuntimePlatform == Device.Android)
            {
                if (!IsPresented)
                    IsPresented = true;
                else
                    Navigation.PopAsync();

                return true;
            }

            return base.OnBackButtonPressed();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Device.RuntimePlatform == Device.Android)
                IsPresented = true;
        }
    }
}

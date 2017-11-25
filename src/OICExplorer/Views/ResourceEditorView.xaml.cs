using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using ReactiveUI;
using ReactiveUI.XamForms;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using OICExplorer.Services;
using Newtonsoft.Json;
using OICNet;
using OICExplorer.ViewModels;

namespace OICExplorer
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ResourceEditorView : ReactiveContentPage<ResourceEditorViewModel>
    {
        public ResourceEditorView()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                this.Bind(ViewModel, vm => vm.RelativeUri, v => v.ResourceUriEntry.Text)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel, vm => vm.Values.Values, v => v.ValueListView.ItemsSource)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel, 
                        vm => vm.RetreiveResourceCommand, 
                        v => v.RetreiveToolbarItem)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                        vm => vm.CreateResourceCommand,
                        v => v.CreateToolbarItem)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                        vm => vm.CreateOrUpdateResourceCommand,
                        v => v.CreateOrUpdateToolbarItem)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                        vm => vm.DeleteResourceCommand,
                        v => v.DeleteToolbarItem)
                    .DisposeWith(disposables);
            });
        }
    }
}

using ReactiveUI;
using ReactiveUI.XamForms;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using OICExplorer.ViewModels;
using System.Reflection;

namespace OICExplorer.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ValueCellView : ReactiveViewCell<ValueViewModel>
	{
        protected readonly CompositeDisposable SubscriptionDisposables = new CompositeDisposable();

        protected Type ViewModelType;

        public ValueCellView ()
		{
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel, vm => vm.Name, v => v.Name.Text)
                    .DisposeWith(SubscriptionDisposables);

                this.OneWayBind(ViewModel, vm => vm.ValueType, v => v.ViewModelType)
                    .DisposeWith(SubscriptionDisposables);

                this.WhenAnyValue(x => x.ViewModel)
                    .Subscribe(_ => GetValue())
                    .DisposeWith(disposables);
            });
		}

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            SubscriptionDisposables.Clear();
        }

        protected void GetValue()
        {
            switch (ViewModel.Value)
            {
                case int intViewModel:
                    Value.Text = intViewModel.ToString();
                    return;
                case bool boolViewModel:
                    Value.Text = boolViewModel.ToString();
                    return;
                case string stringViewModel:
                    Value.Text = stringViewModel;
                    return;
                case IList<string> stringsViewModel:
                    Value.Text = string.Join(", ", stringsViewModel);
                    return;
            }

            if (ViewModel.Value.GetType().GetTypeInfo().IsEnum)
            {
                var values = new List<string>();
                var enumValue = ViewModel.Value as Enum;

                foreach (Enum e in Enum.GetValues(enumValue.GetType()))
                    if (enumValue.HasFlag(e))
                        values.Add(e.ToString());

                Value.Text = "(enum) "+ string.Join(", ", values);
                return;
            }

            Value.Text = "(unsupported)";
        }
    }
}
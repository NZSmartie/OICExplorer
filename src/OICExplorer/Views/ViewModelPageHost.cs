// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MS-PL license.
// See the LICENSE file in the project root for more information.

using ReactiveUI;
using Splat;
using System;
using System.Reactive.Linq;
using Xamarin.Forms;

namespace OICExplorer.Views
{
    // TODO: Update documentation for ViewModelPageHost and submit a PR to ReactiveUI
    /// <summary>
    /// This content view will automatically load and host the view for the given view model. The view model whose view is
    /// to be displayed should be assigned to the <see cref="ViewModel"/> property. Optionally, the chosen view can be
    /// customized by specifying a contract via <see cref="ViewContractObservable"/> or <see cref="ViewContract"/>.
    /// </summary>
    public class ViewModelPageHost : Page, IPageContainer<Page>, IViewFor
    {
        /// <summary>
        /// The view model whose associated view is to be displayed.
        /// </summary>
        public object ViewModel
        {
            get { return GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ViewModel"/> property.
        /// </summary>
        public static readonly BindableProperty ViewModelProperty = BindableProperty.Create(
            nameof(ViewModel),
            typeof(object),
            typeof(ViewModelPageHost),
            default(object),
            BindingMode.OneWay);

        /// <summary>
        /// The content to display when <see cref="ViewModel"/> is <see langword="null"/>.
        /// </summary>
        public Page DefaultPage
        {
            get { return (Page)GetValue(DefaultContentProperty); }
            set { SetValue(DefaultContentProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="DefaultPage"/> property.
        /// </summary>
        public static readonly BindableProperty DefaultContentProperty = BindableProperty.Create(
            nameof(DefaultPage),
            typeof(Page),
            typeof(ViewModelPageHost),
            default(Page),
            BindingMode.OneWay);

        /// <summary>
        /// The contract to use when resolving the view for the given view model.   
        /// </summary>
        public IObservable<string> ViewContractObservable
        {
            get { return (IObservable<string>)GetValue(ViewContractObservableProperty); }
            set { SetValue(ViewContractObservableProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ViewContractObservable"/> property.
        /// </summary>
        public static readonly BindableProperty ViewContractObservableProperty = BindableProperty.Create(
            nameof(ViewContractObservable),
            typeof(IObservable<string>),
            typeof(ViewModelPageHost),
            Observable.Never<string>(),
            BindingMode.OneWay);

        private string viewContract;

        /// <summary>
        /// A fixed contract to use when resolving the view for the given view model.
        /// </summary>
        /// <remarks>
        /// This property is a mere convenience so that a fixed contract can be assigned directly in XAML.
        /// </remarks>
        public string ViewContract
        {
            get { return this.viewContract; }
            set { ViewContractObservable = Observable.Return(value); }
        }

        /// <summary>
        /// Can be used to override the view locator to use when resolving the view. If unspecified, <see cref="ViewLocator.Current"/> will be used.
        /// </summary>
        public IViewLocator ViewLocator { get; set; }

        static readonly BindablePropertyKey CurrentPagePropertyKey = BindableProperty.CreateReadOnly(nameof(ContentPage), typeof(Page), typeof(ViewModelPageHost), null);
        public static readonly BindableProperty CurrentPageProperty = CurrentPagePropertyKey.BindableProperty;

        IPageController PageController => this;

        public Page CurrentPage
        {
            get { return (Page)GetValue(CurrentPageProperty); }
            private set { SetValue(CurrentPagePropertyKey, value); }
        }

        protected override bool OnBackButtonPressed()
        {
            if (CurrentPage != null)
            {
                bool handled = CurrentPage.SendBackButtonPressed();
                if (handled)
                    return true;
            }

            return base.OnBackButtonPressed();
        }

        void SetPage(Page page)
        {
            if (CurrentPage != null)
                PageController.InternalChildren.Remove(CurrentPage);
            if (page != null)
                PageController.InternalChildren.Add(page);

            CurrentPage = page;

            // Bug: Xamarin.Forms will not show CurrentPage.ToolbatItems; clearing our toolbar items seems to trigger an update (investigate this)
            ToolbarItems.Clear();

            if (CurrentPage != null)
                SetInheritedBindingContext(CurrentPage, BindingContext);

        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (CurrentPage != null)
                SetInheritedBindingContext(CurrentPage, BindingContext);
        }

        public ViewModelPageHost()
        {
            // NB: InUnitTestRunner also returns true in Design Mode
            if (ModeDetector.InUnitTestRunner())
            {
                ViewContractObservable = Observable.Never<string>();
                return;
            }

            ViewContractObservable = Observable.Return(default(string));

            var vmAndContract = Observable.CombineLatest(
                this.WhenAnyValue(x => x.ViewModel),
                this.WhenAnyObservable(x => x.ViewContractObservable),
                (vm, contract) => new { ViewModel = vm, Contract = contract, });

            this.WhenActivated(() =>
            {
                return new[] {
                    vmAndContract.Subscribe(x => {
                        this.viewContract = x.Contract;

                        if (x.ViewModel == null) {
                            SetPage(DefaultPage);
                            return;
                        }

                        var viewLocator = ViewLocator ?? ReactiveUI.ViewLocator.Current;
                        var view = viewLocator.ResolveView(x.ViewModel, x.Contract) ?? viewLocator.ResolveView(x.ViewModel, null)
                            ?? throw new Exception($"Couldn't find view for '{x.ViewModel}'.");

                        var castView = view as Page
                            ?? throw new Exception($"View '{view.GetType().FullName}' is not a subclass of '{typeof(View).FullName}'.");

                        view.ViewModel = x.ViewModel;
                        SetPage(castView);
                    })};
            });
        }
    }
}
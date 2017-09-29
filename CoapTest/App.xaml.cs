using System;
using ReactiveUI;
using ReactiveUI.XamForms;
using Splat;
using Xamarin.Forms;

using CoapTest.Services;
using CoapTest.Views;
using CoapTest.ViewModels;

namespace CoapTest
{
    public partial class App : Application, IScreen
    {
        public RoutingState Router { get; protected set; }

        public App()
        {
            InitializeComponent();

            var services = Locator.CurrentMutable;

#if DEBUG
            // Debug logging
            services.RegisterConstant<ILogger>(new MyDebugLogger { Level = LogLevel.Debug });
#endif

            // Register logger for all require generic uses of Microsoft.Extensions.Logging.ILogger<T>
            services.RegisterLogger<OicService>()
                    .RegisterLogger<OICNet.OicClient>()
                    .RegisterLogger<OICNet.OicResourceDiscoverClient>();
            // App-wide services
            services.RegisterLazySingleton(() => new OicService(), typeof(OicService));
            // Register required ReactiveUI classes
            services.RegisterConstant<IScreen>(this);
            // View models
            services.Register<IViewFor<DevicesListViewModel>>(() => new DevicesListView());
            services.Register<IViewFor<DeviceViewModel>>(() => new DeviceView());

            Router = new RoutingState();
            Router.NavigateAndReset
                  .Execute(new DevicesListViewModel())
                  .Subscribe();

            MainPage = new RoutedViewHost();
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        protected override void OnSleep()
        {
            base.OnSleep();
        }

        protected override void OnResume()
        {
            base.OnResume();
        }
    }
}

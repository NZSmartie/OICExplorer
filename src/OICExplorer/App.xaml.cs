using System;
using ReactiveUI;
using ReactiveUI.XamForms;
using Splat;
using Xamarin.Forms;

using OICExplorer.Services;
using OICExplorer.Views;
using OICExplorer.ViewModels;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace OICExplorer
{
    public partial class App : Application, IScreen
    {
        public RoutingState Router { get; protected set; }

        public App()
        {
            InitializeComponent();

            // Setup AppCenter Analyitics/Crash reporting
            Microsoft.AppCenter.AppCenter.Start(
                "android=2fb059c1-aec4-430e-bd51-78218aad4608;" + 
                "uwp=98b3d133-0fff-4602-a039-bcc4731cab7a;" +
                "ios=d654e563-98e7-4352-a877-6339d81ffb0a",
                typeof(Analytics), typeof(Crashes));

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
            services.Register<IViewFor<ResourceEditorViewModel>>(() => new ResourceEditorView());

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

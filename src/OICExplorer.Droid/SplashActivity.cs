using Android.App;
using Android.OS;
using Android.Content;
using Android.Support.V7.App;
using System.Threading.Tasks;
using System.Threading;

namespace OICExplorer.Droid
{
    [Activity(Label = "OIC Explorer", MainLauncher = true, Icon = "@drawable/icon", Theme = "@style/AppTheme.Splash", NoHistory = true)]
    public class SplashActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            StartActivity(typeof(MainActivity));
        }
    }
}


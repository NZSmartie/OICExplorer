using Android.App;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ContentPage), typeof(OICExplorer.Droid.IconPageRenderer))]
namespace OICExplorer.Droid
{
    public class IconPageRenderer : PageRenderer
    {
        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();

            var tabPage = this.Element as ITabPage;
            var actionBar = ((Activity)this.Context)?.ActionBar;
            if (actionBar != null && !string.IsNullOrEmpty(tabPage?.TabIcon))
            {
                actionBar.SetIcon(new FontDrawable(this.Context, tabPage.TabIcon, Color.Black.ToAndroid(), 32));
            }
        }
    }
}
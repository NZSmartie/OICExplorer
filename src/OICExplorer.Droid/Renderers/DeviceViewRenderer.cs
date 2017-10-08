using System;
using Android.Support.V4.Widget;
using Android.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using OICExplorer;

[assembly: ExportRenderer(typeof(DeviceView), typeof(OICExplorer.Droid.DeviceViewRenderer))]
namespace OICExplorer.Droid
{
    public class DeviceViewRenderer : IVisualElementRenderer
    {
        private readonly MasterDetailRenderer _renderer;

        public DeviceViewRenderer()
        {
            _renderer = new MasterDetailRenderer();

            _renderer.ElementChanged += OnElementChanged;
        }

        public VisualElement Element => _renderer.Element;

        public VisualElementTracker Tracker => _renderer.Tracker;

        public ViewGroup ViewGroup => _renderer.ViewGroup;

        public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

        private void OnElementChanged(object sender, VisualElementChangedEventArgs e)
        {
            ElementChanged?.Invoke(this, e);
        }

        public void Dispose()
        {
            _renderer.ElementChanged -= OnElementChanged;
            _renderer.Dispose();
        }

        public SizeRequest GetDesiredSize(int widthConstraint, int heightConstraint)
            => _renderer.GetDesiredSize(widthConstraint, heightConstraint);

        public void SetElement(VisualElement element)
        {
            _renderer.SetElement(element);
            var activity = (DrawerLayout)_renderer;

            // The first child is the Detail view, the master view is the second child.
            // https://developer.android.com/reference/android/support/v4/widget/DrawerLayout.html
            // TODO: Do we want to care about drawer's on the other side of the layout?
            var masterView = activity.GetChildAt(1);

            _renderer.SetFitsSystemWindows(true);
            masterView.SetFitsSystemWindows(true);
        }

        public void UpdateLayout()
        {
            _renderer.UpdateLayout();
        }
    }
}
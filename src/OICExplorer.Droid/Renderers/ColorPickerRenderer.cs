using System;
using System.ComponentModel;
using System.Threading.Tasks;

using Android.App;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using OICExplorer.Controls;

[assembly: ExportRenderer(typeof(ColorPickerView), typeof(OICExplorer.Droid.ColorPickerRenderer))]
namespace OICExplorer.Droid
{
    public class ColorPickerRenderer : ViewRenderer<ColorPickerView, Com.Rarepebble.Colorpicker.ColorPickerView>
    {
        private class ColorObserverEventArgs : EventArgs
        {
            public Color Color { get; set; }
        }

        private class ColorObserver : Java.Lang.Object, Com.Rarepebble.Colorpicker.IColorObserver
        {
            public event EventHandler<ColorObserverEventArgs> ColorChanged;

            public void UpdateColor(Com.Rarepebble.Colorpicker.ObservableColor p0)
            {
                var color = new Android.Graphics.Color(p0.Color);
                ColorChanged?.Invoke(this, new ColorObserverEventArgs { Color = color.ToColor() });
            }
        }

        private ColorObserver _colorObserver = new ColorObserver();

        public ColorPickerRenderer()
        {
            _colorObserver.ColorChanged += OnColorChanged;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<ColorPickerView> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                SetNativeControl(new Com.Rarepebble.Colorpicker.ColorPickerView(Context));
                Control.ShowAlpha(false);
                Control.ShowHex(false);
                Control.AddColorObserver(_colorObserver);
            }

            if(e.NewElement != null)
                Control.Color = e.NewElement.Color.ToAndroid().ToArgb();
        }

        private void OnColorChanged(object sender, ColorObserverEventArgs e)
        {
            if (Element != null && Element.Color != e.Color)
                Element.Color = e.Color;
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if(e.PropertyName == nameof(Element.Color))
            {
                var color = Element.Color.ToAndroid().ToArgb();
                if(Control.Color != color)
                    Control.SetCurrentColor(Element.Color.ToAndroid().ToArgb());
            }
        }
    }
}
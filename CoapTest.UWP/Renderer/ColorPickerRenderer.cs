using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

using CoapTest.Controls;
using CoapTest.UWP.Controls;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(ColorPickerView), typeof(CoapTest.UWP.Renderer.ColorPickerViewRenderer))]
namespace CoapTest.UWP.Renderer
{
    class ColorPickerViewRenderer : ViewRenderer<ColorPickerView, ColorPickerControl>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<ColorPickerView> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                SetNativeControl(new ColorPickerControl());
                Control.PropertyChanged += OnControlPropertyChanged;
            }

            if (e.OldElement != null)
            {
                e.OldElement.PropertyChanged -= OnControlPropertyChanged;
            }

            if (e.NewElement != null)
            {
                e.NewElement.PropertyChanged += OnElementPropertyChanged;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Control.PropertyChanged -= OnControlPropertyChanged;
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == ColorPickerView.ColorProperty.PropertyName)
            {
                var color = Windows.UI.Color.FromArgb((byte)(Element.Color.A * 255), (byte)(Element.Color.R * 255), (byte)(Element.Color.G * 255), (byte)(Element.Color.B * 255));
                if (Control.Color != color)
                    Control.Color = color;
            }
        }

        private void OnControlPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ColorPickerControl.Color))
            {
                var color = Color.FromRgba(Control.Color.R, Control.Color.G, Control.Color.B, Control.Color.A);
                if (Element.Color != color)
                    Element.Color = color;  
            }
        }
    }
}

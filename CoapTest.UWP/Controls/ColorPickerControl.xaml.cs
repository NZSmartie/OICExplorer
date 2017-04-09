using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace CoapTest.UWP.Controls
{
    public sealed partial class ColorPickerControl : UserControl, INotifyPropertyChanged
    {
        private static readonly DependencyProperty HuePickerPointProperty = DependencyProperty.Register(nameof(HuePickerPoint), typeof(Point), typeof(ColorPickerControl), new PropertyMetadata(new Point(120.0d, 120.0d)));
        private static readonly DependencyProperty BrightnessPointYProperty = DependencyProperty.Register(nameof(BrightnessPointY), typeof(double), typeof(ColorPickerControl), new PropertyMetadata(0.0d));
        private static readonly DependencyProperty HueProperty = DependencyProperty.Register(nameof(Hue), typeof(Color), typeof(ColorPickerControl), new PropertyMetadata(Colors.White));
        private static readonly DependencyProperty BrightnessProperty = DependencyProperty.Register(nameof(Brightness), typeof(Color), typeof(ColorPickerControl), new PropertyMetadata(Colors.Transparent));

        public event PropertyChangedEventHandler PropertyChanged;

        private Point HuePickerPoint
        {
            get => (Point)GetValue(HuePickerPointProperty);
            set => SetValue(HuePickerPointProperty, value);
        }

        private double BrightnessPointY
        {
            get => (double)GetValue(BrightnessPointYProperty);
            set => SetValue(BrightnessPointYProperty, value);
        }

        private Color Hue
        {
            get => (Color)GetValue(HueProperty);
            set => SetValue(HueProperty, value);
        }

        private Color Brightness
        {
            get => (Color)GetValue(BrightnessProperty);
            set => SetValue(BrightnessProperty, value);
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(nameof(Color), typeof(Color), typeof(ColorPickerControl), new PropertyMetadata(Colors.White));

        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set
            {
                SetValue(ColorProperty, value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Color)));
            }
        }

        private double _hue = 0.0d;
        private double _saturation = 0.0d;
        private double _brightness = 0.0d;

        public ColorPickerControl()
        {
            DataContext = this;

            InitializeComponent();
        }

        private void PickColor(Point point, Rect bounds)
        {
            double midX = bounds.Width / 2, midY = bounds.Height / 2,
                   rad = Math.Atan2(point.Y - midY, point.X - midX),
                   normX = Math.Cos(rad) * midX,
                   normY = Math.Sin(rad) * midX,
                   minX, maxX, minY, maxY;
            if (normX > 0d)
            {
                minX = (normX * -1.0d) + midX;
                maxX = normX + midX;
            }
            else
            {
                minX = normX + midX;
                maxX = (normX * -1.0d) + midX;
            }

            if (normY> 0d)
            {
                minY = (normY * -1.0d) + midY;
                maxY = normY + midY;
            }
            else
            {
                minY = normY + midY;
                maxY = (normY * -1.0d) + midY;
            }

            var px = Math.Max(minX, point.X);
            px = Math.Min(maxX, px);
            var py = Math.Max(minY, point.Y);
            py = Math.Min(maxY, py);

            HuePickerPoint = new Point(Math.Round(px), Math.Round(py));

            _hue = ((rad / (2 * Math.PI)) - 0.75d) * -1.0d;
            _saturation = Math.Max(0, Math.Min(1, Math.Sqrt(Math.Pow(point.Y - midY, 2) + Math.Pow(point.X - midX, 2)) / midX));

            var hsv = new ColorMine.ColorSpaces.Hsv { H = _hue * 360d, S = _saturation, V = 1.0d };
            var color = hsv.ToRgb();

            Hue = new Color { A = 255, R = (byte)color.R, G = (byte)color.G, B = (byte)color.B };

            hsv.V = 1.0d - _brightness;
            color = hsv.ToRgb();

            Color = new Color { A = 255, R = (byte)color.R, G = (byte)color.G, B = (byte)color.B };
        }

        private void PickBrightness(double y, Rect bounds)
        {
            var newY = Math.Max(bounds.Top, Math.Min(bounds.Bottom, y));
            BrightnessPointY = newY;

            _brightness = Math.Max(0, Math.Min(1, (y - bounds.Top) / bounds.Height));
            Brightness = new Color { A = (byte)(_brightness * 255), R =0, G = 0, B = 0 };

            var hsv = new ColorMine.ColorSpaces.Hsv { H = _hue * 360d, S = _saturation, V = 1.0d - _brightness };
            var color = hsv.ToRgb();

            Color = new Color { A = 255, R = (byte)color.R, G = (byte)color.G, B = (byte)color.B };
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var bounds = new Rect(0, 0, pickerCanvas.ActualWidth, pickerCanvas.ActualHeight);
            PickColor(e.GetCurrentPoint(pickerCanvas).Position, bounds);

            pickerCanvas.CapturePointer(e.Pointer);

            PointerEventHandler moved = null;
            moved = (s, args) =>
            {
                PickColor(args.GetCurrentPoint(pickerCanvas).Position, bounds);
            };

            PointerEventHandler released = null;
            released = (s, args) =>
            {
                pickerCanvas.ReleasePointerCapture(args.Pointer);
                PickColor(args.GetCurrentPoint(pickerCanvas).Position, bounds);
                pickerCanvas.PointerMoved -= moved;
                pickerCanvas.PointerReleased -= released;
            };

            pickerCanvas.PointerMoved += moved;
            pickerCanvas.PointerReleased += released;
        }

        private void OnBrightnessPressed(object sender, PointerRoutedEventArgs e)
        {
            var bounds = new Rect(0, 0, 0, brightnessCanvas.ActualHeight - 12);
            PickBrightness(e.GetCurrentPoint(brightnessCanvas).Position.Y, bounds);


            brightnessCanvas.CapturePointer(e.Pointer);
            PointerEventHandler moved = null;
            moved = (s, args) =>
            {
                PickBrightness(e.GetCurrentPoint(brightnessCanvas).Position.Y, bounds);
            };

            PointerEventHandler released = null;
            released = (s, args) =>
            {
                brightnessCanvas.ReleasePointerCapture(args.Pointer);

                PickBrightness(e.GetCurrentPoint(brightnessCanvas).Position.Y, bounds);

                brightnessCanvas.PointerMoved -= moved;
                brightnessCanvas.PointerReleased -= released;
            };

            brightnessCanvas.PointerMoved += moved;
            brightnessCanvas.PointerReleased += released;
        }

        private void OnHuePressed(object sender, PointerRoutedEventArgs e)
        {

        }

        private void OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            
        }
    }
}

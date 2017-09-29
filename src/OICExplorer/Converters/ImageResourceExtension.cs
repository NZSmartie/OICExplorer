using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OICExplorer.Converters
{
    [ContentProperty("Source")]
    public class ImageResourceExtension : IMarkupExtension
    {
        public string Source { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Source == null)
                return null;

            ImageSource image = null;

            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    image = ImageSource.FromFile(Source);
                    break;
                default:
                    image = ImageSource.FromFile("Assets/" + Source);
                    break;
            }

            return image;
        }
    }
}

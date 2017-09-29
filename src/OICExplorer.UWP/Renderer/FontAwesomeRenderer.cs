using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(Button), typeof(OICExplorer.UWP.Renderer.FontAwesomeRenderer))]
namespace OICExplorer.UWP.Renderer
{
    class FontAwesomeRenderer : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            // Catchall if, any text that contains FontAwesome characters
            if(e.NewElement?.Text != null && e.NewElement.Text.Any(c => c >= 0xF000))
                e.NewElement.FontFamily = "Assets/fontawesome.ttf#FontAwesome";
        }
    }
}

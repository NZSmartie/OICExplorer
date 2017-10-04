using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace OICExplorer.Dialogs
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ColourPickerDialog : ContentPage
    {
        public ColourPickerDialog()
        {
            InitializeComponent();
        }

        private void OnClose(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }
    }
}

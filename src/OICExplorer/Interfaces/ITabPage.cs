using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OICExplorer
{
    public interface ITabPage
    {
        string TabIcon { get; }

        string SelectedTabIcon { get; }
    }
}

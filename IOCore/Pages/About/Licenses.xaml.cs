using System.IO;
using IOCore.Libs;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace IOCore.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Licenses : IOPage
    {
        public new static Licenses Inst { get; private set; }

        public Licenses()
        {
            InitializeComponent();
            Inst = this;
            DataContext = this;

            LicenseTextBlock.Text = File.ReadAllText(Path.Combine(Utils.GetAssetsFolderPath(), "license.txt"));
        }
    }
}
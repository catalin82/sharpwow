using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms.Integration;

namespace SharpWoW.Controls
{
    /// <summary>
    /// Interaction logic for ExpanderControl.xaml
    /// </summary>
    public partial class ExpanderControl : UserControl
    {
        public ExpanderControl()
        {
            InitializeComponent();
            
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public WindowsFormsHost Host { get { return winFormsHost; } }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            if (ExpandedChanged != null)
                ExpandedChanged(expander.IsExpanded);
        }

        public event Action<bool> ExpandedChanged;

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            if (expander.IsExpanded == false)
                expander.IsExpanded = true;
        }

        public void Collapse()
        {
            expander.IsExpanded = false;
        }

        public bool IsStatic { get { return staticToggleButton.IsChecked.Value; } }
    }
}

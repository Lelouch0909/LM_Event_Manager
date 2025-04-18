using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DriverSolution.customControl;

public partial class MenuButton : UserControl
{
    public MenuButton()
    {
        InitializeComponent();
    }

    public PathGeometry Icon
    {
        get { return (PathGeometry) GetValue(IconProperty);}
        set {SetValue(IconProperty, value);}
        
    }

    // Je declare
    public static readonly DependencyProperty IconProperty = DependencyProperty
        .Register("Icon",typeof(PathGeometry),typeof(MenuButton),new PropertyMetadata(0));
}
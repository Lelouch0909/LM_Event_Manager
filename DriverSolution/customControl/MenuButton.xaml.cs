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

    public String Text
    {
        get { return (String) GetValue(TextProperty); }
        set {SetValue(TextProperty, value);}
    }

    // Je declare
    public static readonly DependencyProperty IconProperty = DependencyProperty
        .Register("Icon",typeof(PathGeometry),typeof(MenuButton));

    public static readonly DependencyProperty TextProperty = DependencyProperty
        .Register("Text", typeof(String), typeof(MenuButton));
}
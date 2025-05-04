using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DriverSolution.customControl;

public partial class MenuButton : UserControl
{
    public event RoutedEventHandler Click;

    public Boolean IsActive
    {
        get => (Boolean)GetValue(isActiveProperty);
        set => SetValue(isActiveProperty, value);
    }
    
    public static readonly DependencyProperty isActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(Boolean), typeof(MenuButton), new PropertyMetadata(false));
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(MenuButton), new PropertyMetadata("text text"));

    public static readonly DependencyProperty IconKindProperty =
        DependencyProperty.Register(nameof(IconKind), typeof(string), typeof(MenuButton), new PropertyMetadata("Text"));

    
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    
    public string IconKind
    {
        get => (string)GetValue(IconKindProperty);
        set => SetValue(IconKindProperty, value);
    }
    
    
    public MenuButton()
    {
        InitializeComponent();
    }

 

    private void OnClick(object sender, RoutedEventArgs e)
    {
        Click?.Invoke(this, e);
        IsActive = true;
    }
}
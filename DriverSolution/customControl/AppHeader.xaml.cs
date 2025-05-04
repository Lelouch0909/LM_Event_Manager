using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Appwrite.Models;
using DriverSolution.models;
using DriverSolution.pages;

namespace DriverSolution.customControl;

public partial class AppHeader : UserControl
{
    
    public event Action<AppEvent> ActiveEventChanged;
    
    private List<AppEvent> _events;
    private AppEvent _activeEvent;
    public static readonly DependencyProperty ActiveEventProperty =
        DependencyProperty.Register(
            nameof(ActiveEvent), 
            typeof(AppEvent), 
            typeof(AppHeader),
            new PropertyMetadata(null, OnActiveEventChanged));

    public AppEvent ActiveEvent
    {
        get => (AppEvent)GetValue(ActiveEventProperty);
        set => SetValue(ActiveEventProperty, value);
    }

    private static void OnActiveEventChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = d as AppHeader;
        control?.UpdateCurrentEventText();
    }

    private void UpdateCurrentEventText()
    {
        CurrentEventText.Text = ActiveEvent?.Name ?? "Aucun événement sélectionné";
    }

    private void EventListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (EventsListBox.SelectedItem is AppEvent selectedEvent && selectedEvent != ActiveEvent)
        {
            ActiveEvent = selectedEvent;
            ActiveEventChanged?.Invoke(selectedEvent);
        }
    }

    public void Initialize(List<AppEvent> events, AppEvent activeEvent)
    {
        _events = events;
        ActiveEvent = activeEvent;
        EventsListBox.ItemsSource = _events.Where(e => e != activeEvent).ToList();
        UpdateCurrentEventText();
    }

    public Frame PageNewEvent { get; set; }

    public static readonly DependencyProperty CurrentUserProperty =
        DependencyProperty.Register(
            nameof(CurrentUser), 
            typeof(User), 
            typeof(AppHeader),
            new PropertyMetadata(null));

    public User CurrentUser
    {
        get => (User)GetValue(CurrentUserProperty);
        set => SetValue(CurrentUserProperty, value);
    }
    
 
    private void UpdateUI()
    {
        // Implémentez la mise à jour de l'UI ici
        // Par exemple, remplir une ComboBox avec les événements
        // et sélectionner l'événement actif
    }
    
    // Méthode appelée quand l'utilisateur change d'événement
    private void OnEventSelectionChanged(AppEvent selectedEvent)
    {
        _activeEvent = selectedEvent;
        ActiveEventChanged?.Invoke(_activeEvent);
    }
    public AppHeader()
    {
        InitializeComponent();
        this.DataContext = this;
    }
    public AppHeader(User currentUser) : this() // Appelle le constructeur sans param
    {
        CurrentUser = currentUser;
    }

    private void caretIcon_Open(object sender, MouseButtonEventArgs e)
    {
        Storyboard sb1 = (Storyboard)this.Resources["rotateAnimationOn"];

        sb1.Begin();
    }

    private void caretIcon_Closed(object sender, RoutedEventArgs e)
    {
        Storyboard sb2 = (Storyboard)this.Resources["rotateAnimationOff"];
        sb2.Begin();
    }

    private void OpenNewPage(object sender, RoutedEventArgs e)
    {
        // Récupère la fenêtre principale
        var mainWindow = Application.Current.MainWindow as MainWindow;
    
        if (mainWindow != null && mainWindow.MainFrame != null)
        {
            // Navigue vers la nouvelle page
            mainWindow.MainFrame.Navigate(new PageCreationEvenement(CurrentUser));
        
            //  cache le header 
            this.Visibility = Visibility.Collapsed;
        }
    }
}
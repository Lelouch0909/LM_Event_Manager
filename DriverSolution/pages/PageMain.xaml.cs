using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Appwrite.Models;
using DriverSolution.customControl;
using DriverSolution.models;

namespace DriverSolution.pages;

public partial class PageMain : Page
{
    private User _currentUser;
    private List<AppEvent> _userEvents;
    private AppEvent _activeEvent;

    private readonly AppwriteService _appwriteService;
    private PageAcceuil _mainPageAcceuil;


    public User CurrentUser
    {
        get => _currentUser;
        set
        {
            _currentUser = value;
            OnPropertyChanged(nameof(CurrentUser));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private ObservableCollection<MenuItemsModel> menuItems { get; set; } = new ObservableCollection<MenuItemsModel>();
    private List<MenuButton> menuButtons = new();


    public PageMain(User currentUser)
    {
        InitializeComponent();
        _appwriteService = AppwriteService.Instance;
        CurrentUser = currentUser;
        DataContext = this;

        // Créer une seule instance de PageAcceuil
        _mainPageAcceuil = new PageAcceuil();
        BodyContainer.Children.Add(_mainPageAcceuil);


        Loaded += PageMain_Loaded;

        menuItems.Add(
            new MenuItemsModel
            {
                Title = "Home",
                IconKind = "ViewDashboardOutline",
                ClickAction = () => NavigateToHome()
            }
        );
        menuItems.Add(
            new MenuItemsModel
            {
                Title = "Members",
                IconKind = "AccountGroup",
                ClickAction = () => NavigateTo(new PageMembers())
            }
        );
        menuItems.Add(
            new MenuItemsModel
            {
                Title = "Social",
                IconKind = "Forum",
                ClickAction = () => NavigateTo(new PageSocial())
            }
        );
        menuItems.Add(
            new MenuItemsModel
            {
                Title = "Stats",
                IconKind = "ChartBar",
                ClickAction = () => NavigateTo(new PageStatistics())
            }
        );

        foreach (MenuItemsModel menuItemsModel in menuItems)
        {
            var button = new MenuButton
            {
                Title = menuItemsModel.Title,
                IconKind = menuItemsModel.IconKind,
            };

            button.Click += (s, e) =>
            {
                foreach (var b in menuButtons)
                    b.IsActive = false;

                button.IsActive = true;
                menuItemsModel.ClickAction?.Invoke();
            };

            if (button.Title == "Home")
            {
                button.IsActive = true;
            }

            menuButtons.Add(button);
            MenuStackPanel.Children.Add(button);
        }
    }

    private void NavigateToHome()
    {
        BodyContainer.Children.Clear();
        BodyContainer.Children.Add(_mainPageAcceuil);

        // Force le rafraîchissement
        if (_activeEvent != null)
        {
            _mainPageAcceuil.SetActiveEvent(_activeEvent);
        }
    }



    private async void PageMain_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _currentUser = await _appwriteService.GetCurrentUserAsync();
            if (_currentUser == null) return;

            _userEvents = await _appwriteService.GetUserEventsAsync(_currentUser.Id);
            _activeEvent = _userEvents.FirstOrDefault();

            // Mise à jour synchrone
            appHeader.Initialize(_userEvents, _activeEvent);
            _mainPageAcceuil.SetActiveEvent(_activeEvent);
        
            appHeader.ActiveEventChanged += OnActiveEventChanged;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement: {ex.Message}");
        }
    }
    private void OnActiveEventChanged(AppEvent newActiveEvent)
    {
        _activeEvent = newActiveEvent;
        appHeader.ActiveEvent = _activeEvent; // Mise à jour du header

        if (BodyContainer.Children[0] is PageAcceuil pageAcceuil)
        {
            pageAcceuil.SetActiveEvent(_activeEvent);
        }
    }
    
    public AppEvent ActiveEvent 
    {
        get => _activeEvent;
        set
        {
            _activeEvent = value;
            OnPropertyChanged(nameof(ActiveEvent));
        }
    }


    private void NavigateTo(UserControl page)
    {
        BodyContainer.Children.Clear();
        BodyContainer.Children.Add(page);
        
        if (page is PageAcceuil acceuilPage && _activeEvent != null)
        {
            acceuilPage.SetActiveEvent(_activeEvent);
        }
    }

    private void AppHeader_Loaded(object sender, RoutedEventArgs e)
    {
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void pageAcceuil_Loaded(RoutedEventArgs e)
    {

    }

    
}
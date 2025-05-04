using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Appwrite.Models;
using DriverSolution.pages;
using DriverSolution.services;

namespace DriverSolution;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly AppwriteService _appwriteService = AppwriteService.Instance;

    private readonly INavigationService _navigationService;

    public User currentUser { get; set; }

    public MainWindow()
    {
        InitializeComponent();

        Loaded += MainWindow_Loaded;
        _navigationService = new NavigationService(MainContent);
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await CheckAuthAndNavigate();
    }

    public void NavigateTo(UserControl page)
    {
        _navigationService.NavigateTo(page);
    }

    public void NavigateBack()
    {
        _navigationService.NavigateBack();
    }

    private async Task CheckAuthAndNavigate()
    {
        var savedSession = SessionStorage.LoadSession();
       // MainFrame.Navigate(new PageMain(currentUser));

        if (!String.IsNullOrEmpty(savedSession))
        {
            _appwriteService.RestoreSession(savedSession);
            try
            {
                // Vérifie la session existante
                currentUser = await _appwriteService.GetCurrentUserAsync();

                if (currentUser != null)
                {
                    // Utilisateur connecté
                    MainFrame.Navigate(new PageMain(currentUser));
                }
                else
                {
                    // Non connecté
                    MainFrame.Navigate(new Pageinscription());
                }
            }
            catch
            {
                // En cas d'erreur, on considère comme non connecté
                MainFrame.Navigate(new Pageinscription());
            }
        }
        else
        {
            MainFrame.Navigate(new Pageinscription());

        }
    }

    private void WindowDrag(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            this.DragMove();
    }
}
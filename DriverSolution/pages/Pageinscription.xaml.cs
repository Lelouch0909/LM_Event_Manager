using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Appwrite.Models;

namespace DriverSolution.pages;

public partial class Pageinscription : Page
{
    public User currentUSer { get; set; }

    private AppwriteService _appwriteService = AppwriteService.Instance;

    public Pageinscription()
    {
        InitializeComponent();
    }

    private void SwitchToRegister_Click(object sender, MouseButtonEventArgs e)
    {
        tcAuth.SelectedIndex = 1; // Basculer vers l'onglet d'inscription
    }

    private void SwitchToLogin_Click(object sender, MouseButtonEventArgs e)
    {
        tcAuth.SelectedIndex = 0; // Basculer vers l'onglet de connexion
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(tbLoginEmail.Text) || pbLoginPassword.Password.Length < 8)
        {
            MessageBox.Show("Veuillez saisir un email valide et un mot de passe d'au moins 8 caractères");
            return;
        }

        string email = tbLoginEmail.Text;
        string password = pbLoginPassword.Password;

        try
        {
            var session = await _appwriteService.LoginAsync(email, password);
            currentUSer = await _appwriteService.GetCurrentUserAsync();

            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.currentUser = currentUSer;

            var userEvents = await _appwriteService.GetUserEventsAsync(currentUSer.Id);

            if (userEvents == null || !userEvents.Any())
            {
                mainWindow.MainFrame.Navigate(new PageCreationEvenement(currentUSer));
            }
            else
            {
                mainWindow.MainFrame.Navigate(new PageMain(currentUSer));
            }
        }
        catch (Exception ex)
        {
            Console.Write(ex);
            Console.Error.Write(ex.Message);
            MessageBox.Show($"Échec de la connexion : {ex.Message}");
        }
    }


    private async void Register_Click(object sender, RoutedEventArgs e)
    {
        // Validation des champs
        if (string.IsNullOrWhiteSpace(tbRegisterFullName.Text) ||
            string.IsNullOrWhiteSpace(tbRegisterEmail.Text) ||
            pbRegisterPassword.Password.Length < 8 ||
            pbRegisterPassword.Password != pbRegisterConfirmPassword.Password)
        {
            MessageBox.Show(
                "Veuillez remplir tous les champs correctement et vérifier que les mots de passe correspondent");
            return;
        }

        if (!cbTerms.IsChecked ?? false)
        {
            MessageBox.Show("Veuillez accepter les conditions générales");
            return;
        }

        string fullName = tbRegisterFullName.Text;
        string email = tbRegisterEmail.Text;
        string password = pbRegisterPassword.Password;

        try
        {
            currentUSer = await _appwriteService.SignUpAsync(email, password, fullName);
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow.MainFrame.Navigate(new PageCreationEvenement(currentUSer));
        }
        catch (Exception ex)
        {
            Console.Write(ex);
            Console.Error.Write(ex.Message);
            MessageBox.Show($"Erreur lors de l'inscription : {ex.Message}");
        }
    }

    private void GoogleLogin_Click(object sender, RoutedEventArgs e)
    {
        // Implémentez la connexion avec Google ici
        MessageBox.Show("Connexion avec Google");
    }

    private void ForgotPassword_Click(object sender, MouseButtonEventArgs e)
    {
        // Implémentez la réinitialisation du mot de passe ici
        MessageBox.Show("Lien de réinitialisation envoyé par email");
    }
}
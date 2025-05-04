using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DriverSolution.models;
using DriverSolution.pages;
using MaterialDesignThemes.Wpf;
using MessageBox = HandyControl.Controls.MessageBox;

namespace DriverSolution.customControl
{
    public partial class PageSocial : UserControl
    {
  
        public AppEvent CurrentEvent { get; private set; }

        public PageSocial()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += PageSocial_Loaded;
        }

        private void PageSocial_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Récupérer l'événement actif depuis PageMain
            if (TryFindParent<PageMain>(this) is PageMain pageMain)
            {
                CurrentEvent = pageMain.ActiveEvent;
                InitializeSocialCards();
            }
        }

        private void InitializeSocialCards()
        {
            var socialPlatforms = new List<SocialPlatform>
            {
                new SocialPlatform 
                { 
                    Name = "Twitter", 
                    Icon = PackIconKind.Twitter,
                    Color = Brushes.DodgerBlue,
                    ShareCommand = new RelayCommand(_ => ShareToTwitter())
                },
                new SocialPlatform 
                { 
                    Name = "Facebook", 
                    Icon = PackIconKind.Facebook,
                    Color = Brushes.Blue,
                    ShareCommand = new RelayCommand(_ => ShareToFacebook())
                },
                new SocialPlatform 
                { 
                    Name = "LinkedIn", 
                    Icon = PackIconKind.Linkedin,
                    Color = Brushes.CornflowerBlue,
                    ShareCommand = new RelayCommand(_ => ShareToLinkedIn())
                },
                new SocialPlatform 
                { 
                    Name = "Instagram", 
                    Icon = PackIconKind.Instagram,
                    Color = Brushes.Purple,
                    ShareCommand = new RelayCommand(_ => ShareToInstagram())
                }
            };

            SocialCards.ItemsSource = socialPlatforms;
        }

        private void ShareToTwitter()
        {
            if (CurrentEvent == null) return;
            string message = Uri.EscapeDataString($"Participez à {CurrentEvent.Name}! #Evenement");
            OpenUrl($"https://twitter.com/intent/tweet?text={message}");
        }

        private async void ShareToFacebook()
        {
            if (CurrentEvent == null) return;
            string message = Uri.EscapeDataString($"Je participe à {CurrentEvent.Name}!");
            string url = Uri.EscapeDataString(await AppwriteService.Instance.GetShareableLink(CurrentEvent.Id) ?? "https://example.com");
            OpenUrl($"https://www.facebook.com/sharer/sharer.php?u={url}&quote={message}");
        }

        private async void ShareToLinkedIn()
        {
            if (CurrentEvent == null) return;
            string url = Uri.EscapeDataString(await AppwriteService.Instance.GetShareableLink(CurrentEvent.Id) ?? "https://example.com");
            string title = Uri.EscapeDataString(CurrentEvent.Name);
            OpenUrl($"https://www.linkedin.com/shareArticle?mini=true&url={url}&text={title}");
        }

        private void OpenUrl(string url)
        {
            try
            {
                // Méthode moderne recommandée pour .NET Core/.NET 5+
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Impossible d'ouvrir le lien: {ex.Message}");
            }
        }

        private  void ShareToInstagram()
        {
            if (CurrentEvent == null) return;
            MessageBox.Show("Pour Instagram, utilisez l'app mobile pour partager cet événement");
        }

        // Helper pour trouver le parent dans l'arbre visuel
        private static T TryFindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null && !(child is T))
            {
                child = VisualTreeHelper.GetParent(child);
            }
            return child as T;
        }
    }

    public class SocialPlatform
    {
        public string Name { get; set; }
        public PackIconKind Icon { get; set; }
        public Brush Color { get; set; }
        public ICommand ShareCommand { get; set; }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        
        public RelayCommand(Action<object> execute)
        {
            _execute = execute;
        }

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => _execute(parameter);

        public event EventHandler CanExecuteChanged;
    }
}
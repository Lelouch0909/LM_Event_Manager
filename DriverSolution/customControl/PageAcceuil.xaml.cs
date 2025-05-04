using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DriverSolution.models;

namespace DriverSolution.customControl
{
    public partial class PageAcceuil : UserControl
    {
        private AppEvent _currentEvent;
        private List<Activity> _activities = new List<Activity>();
        public ICommand OpenActivityCommand { get; }

        public PageAcceuil()
        {
            InitializeComponent();
            OpenActivityCommand = new RelayCommand(OpenActivityDetails);

            Loaded += async (s, e) => await LoadActivitiesIfNeeded();

        }

        public async void SetActiveEvent(AppEvent activeEvent)
        {
            Console.WriteLine($"SetActiveEvent appelé avec: {activeEvent?.Name ?? "null"}");

            _currentEvent = activeEvent;

            if (activeEvent == null)
            {
                ResetUI();
                return;
            }

            // Nom de l'événement
            EventNameText.Text = activeEvent.Name;

            // Dates
            UpdateEventDates(activeEvent);

            // Lieu
            UpdateEventLocation(activeEvent);

            // Image de couverture
            UpdateCoverImage(activeEvent.ImageUrl);

            // Charger les activités
            await LoadActivities();
        }

        private async Task LoadActivitiesIfNeeded()
        {
            if (_currentEvent != null && (!_activities.Any() || _activities[0].EventId != _currentEvent.Id))
            {
                await LoadActivities();
            }
        }
        private async Task LoadActivities()
        {
            if (_currentEvent == null) return;

            try
            {
                var appwriteService = AppwriteService.Instance;
                _activities = await appwriteService.GetEventActivitiesAsync(_currentEvent.Id);
        
                var activityViewModels = _activities.Select(a => new ActivityViewModel(a)).ToList();
        
                Dispatcher.Invoke(() =>
                {
                    ActivitiesItemsControl.ItemsSource = activityViewModels;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des activités: {ex.Message}");
                MessageBox.Show("Impossible de charger les activités", "Erreur", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void OpenActivityDetails(object parameter)
        {
            if (parameter is ActivityViewModel vm)
            {
                var window = new ActivityDetailsPage(vm.Activity)
                {
                    Owner = Window.GetWindow(this)
                };
                window.ShowDialog();
            }
        }
   
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            // Quand la page est remise dans l'arbre visuel, on rafraîchit l'affichage
            if (this.Parent != null && _currentEvent != null)
            {
                SetActiveEvent(_currentEvent);
            }
        }
        private async void AddActivityButton_Click(object sender, RoutedEventArgs e)
        {
            // Implémentez la logique pour ajouter une nouvelle activité
            // Par exemple, ouvrir une fenêtre/modale de création
            var dialog = new NewActivityDialog(_currentEvent.Id);
            if (dialog.ShowDialog() == true)
            {
                await LoadActivities(); // Rafraîchir la liste
            }
        }
        private void ResetUI()
        {
            EventNameText.Text = "Aucun événement sélectionné";
            EventDatesText.Text = "Dates non disponibles";
            EventLocationText.Text = "Lieu non défini";
            CoverImage.Source = new BitmapImage(new Uri("../ressources/event_cover.jpg", UriKind.Relative));
        }

        private void UpdateEventDates(AppEvent activeEvent)
        {
            if (activeEvent.EndDateTime.HasValue)
            {
                EventDatesText.Text =
                    $"{activeEvent.StartDateTime:dd MMMM yyyy} - {activeEvent.EndDateTime.Value:dd MMMM yyyy}";
            }
            else
            {
                EventDatesText.Text = $"{activeEvent.StartDateTime:dd MMMM yyyy}";
            }
        }

        private void UpdateEventLocation(AppEvent activeEvent)
        {
            string locationText = string.Empty;

            switch (activeEvent.EventLocationType)
            {
                case AppEvent.LocationType.Physique:
                    locationText = $"{activeEvent.PlaceName}\n{activeEvent.Address}";
                    break;

                case AppEvent.LocationType.Online:
                    locationText = $"En ligne\n{activeEvent.Link}";
                    break;

                case AppEvent.LocationType.Hybride:
                    locationText =
                        $"{activeEvent.PlaceName}\n{activeEvent.Address}\n\nÉgalement en ligne: {activeEvent.Link}";
                    break;
            }

            EventLocationText.Text = locationText;
        }

        private async void UpdateCoverImage(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                SetDefaultImage();
                return;
            }

            try
            {
                // Utilisez un HttpClient avec timeout
                using (var client = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) })
                {
                    var response = await client.GetAsync(imageUrl);
                    response.EnsureSuccessStatusCode();

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        await Dispatcher.InvokeAsync(() =>
                        {
                            var bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.StreamSource = stream;
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                            bitmap.EndInit();

                            if (bitmap.PixelWidth > 1 && bitmap.PixelHeight > 1)
                            {
                                CoverImage.Source = bitmap;
                                Console.WriteLine("Image chargée et affichée avec succès");
                            }
                            else
                            {
                                SetDefaultImage();
                            }
                        }, System.Windows.Threading.DispatcherPriority.Background);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur de chargement: {ex.Message}");
                SetDefaultImage();
            }
        }

        private async Task LoadImageWithWebClient(string url)
        {
            try
            {
                using (var client = new System.Net.WebClient())
                {
                    var imageData = await client.DownloadDataTaskAsync(url);
                    var bitmap = new BitmapImage();

                    using (var stream = new System.IO.MemoryStream(imageData))
                    {
                        bitmap.BeginInit();
                        bitmap.StreamSource = stream;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze(); // Important pour le thread
                    }

                    Dispatcher.Invoke(() => { CoverImage.Source = bitmap; });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebClient error: {ex.Message}");
                SetDefaultImage();
            }
        }

        private void SetDefaultImage()
        {
            try
            {
                var uri = new Uri("pack://application:,,,/ressources/event_cover.jpg");
                CoverImage.Source = new BitmapImage(uri);
            }
            catch
            {
                CoverImage.Source = null;
            }
        }

        private async void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            string eventId = _currentEvent.Id;

            try
            {
                var appwriteService = AppwriteService.Instance;
                var shareableLink = await appwriteService.GetShareableLink(eventId);

                // Créer une popup pour afficher le lien
                var popup = new Window
                {
                    Title = "Partager l'événement",
                    Width = 400,
                    Height = 200,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize
                };

                var stackPanel = new StackPanel { Margin = new Thickness(20) };

                var textBlock = new TextBlock
                {
                    Text = "Lien de partage :",
                    Margin = new Thickness(0, 0, 0, 10)
                };

                var textBox = new TextBox
                {
                    Text = shareableLink,
                    IsReadOnly = true,
                    Margin = new Thickness(0, 0, 0, 20)
                };

                var copyButton = new Button
                {
                    Content = "Copier le lien",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Style = (Style)FindResource("MaterialDesignOutlinedButton"),
                    Padding = new Thickness(10, 5, 10, 5)
                };

                copyButton.Click += (s, args) =>
                {
                    Clipboard.SetText(shareableLink);
                    MessageBox.Show("Lien copié dans le presse-papiers !", "Succès",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                };

                stackPanel.Children.Add(textBlock);
                stackPanel.Children.Add(textBox);
                stackPanel.Children.Add(copyButton);

                popup.Content = stackPanel;
                popup.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la génération du lien : {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void AddSpeakerButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentEvent == null) return;

            var dialog = new AddSpeakerDialog();
            if (dialog.ShowDialog() == true && dialog.NewSpeaker != null)
            {
                try
                {
                    var appwriteService = AppwriteService.Instance;
                    await appwriteService.CreateSpeakerAsync(dialog.NewSpeaker, _currentEvent.Id);
            
                    MessageBox.Show("Intervenant ajouté avec succès", "Succès",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'ajout: {ex.Message}",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
   
        
        private void InitializeActivityViewModels(List<Activity> activities)
        {
            var activityViewModels = activities.Select(a => new ActivityViewModel(a)).ToList();
            ActivitiesItemsControl.ItemsSource = activityViewModels;
        }
     
        
        private void ActivityItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is ActivityViewModel vm)
            {
                var activity = vm.Activity;
        
                try
                {
                    // Débogage - afficher des informations sur l'activité
                    Console.WriteLine($"Ouverture de l'activité : {activity.Name} (ID: {activity.Id})");
            
                    // Créer la fenêtre de détails
                    var detailsPage = new ActivityDetailsPage(activity);
            
                    // S'assurer que le propriétaire est défini correctement
                    var parentWindow = Window.GetWindow(this);
                    if (parentWindow != null)
                    {
                        Console.WriteLine("Parent window found, setting owner");
                        detailsPage.Owner = parentWindow;
                
                        // Affichez la fenêtre comme modal
                        detailsPage.ShowDialog();
                    }
                    else
                    {
                        Console.WriteLine("Parent window NOT found");
                        // Si on ne trouve pas de parent, afficher sans propriétaire
                        detailsPage.ShowDialog();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de l'ouverture des détails: {ex.Message}");
                    MessageBox.Show($"Impossible d'ouvrir les détails de l'activité: {ex.Message}", 
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    public class ActivityViewModel
    {
        public Activity Activity { get; }

        public ActivityViewModel(Activity activity)
        {
            Activity = activity;
        }

        public string Name => Activity.Name;

        public string FormattedPeriod =>
            $"{Activity.StartDateTime:dd MMMM yyyy | HH:mm} - {Activity.EndDateTime:HH:mm}";

        public string FormattedSpeakers =>
            Activity.Speakers.Any() ? $"Avec : {string.Join(", ", Activity.Speakers)}" : "Aucun intervenant";
    }
}
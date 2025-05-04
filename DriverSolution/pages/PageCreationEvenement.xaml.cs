using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Appwrite.Models;
using DriverSolution.customControl;
using DriverSolution.models;

namespace DriverSolution.pages
{
    public partial class PageCreationEvenement : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private UserControl _currentView;
        private AppEvent _currentEvent = new AppEvent(); // Ajout de la déclaration
        private readonly User _currentUser;

        public UserControl CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        private string _currentStep = "Informations";
        public string CurrentStep
        {
            get => _currentStep;
            set
            {
                _currentStep = value;
                OnPropertyChanged(nameof(CurrentStep));
            }
        }

        public PageCreationEvenement(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            DataContext = this;
            ShowInformations(); // Commencez directement par les informations
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ShowInformations()
        {
            CurrentStep = "Informations";
            CurrentView = new FormInformations(
                parent: this,
                eventData: _currentEvent,
                onNext: updatedEvent => {
                    _currentEvent = updatedEvent;
                    ShowLocalisation();
                }
            );
        }

        public void ShowLocalisation()
        {
            CurrentStep = "Localisation";
            CurrentView = new FormLocalisation(
                parent: this,
                eventData: _currentEvent,
                onNext: updatedEvent => {
                    _currentEvent = updatedEvent;
                    ShowDates();
                },
                onBack: ShowInformations
            );
        }

        public void ShowDates()
        {
            CurrentStep = "Dates";
            CurrentView = new FormDates(
                parent: this,
                eventData: _currentEvent,
                onSubmit: async finalEvent => {
                    _currentEvent = finalEvent;
                    await SubmitEvent();
                },
                onBack: ShowLocalisation
            );
        }

     
        private async Task SubmitEvent()
        {
            try
            {
                var appwriteService = AppwriteService.Instance;
              
                await appwriteService.CreateEventAsync(_currentEvent);
        
                MessageBox.Show("Événement créé avec succès !");
                BackHome();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la création: {ex.Message}");
            }
        }

        public void BackHome(object sender = null, RoutedEventArgs e = null)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.MainFrame.Navigate(new PageMain(_currentUser));
            }
        }
        

        private void GoToInformations(object sender, MouseButtonEventArgs e) => ShowInformations();
        private void GoToLocalisation(object sender, MouseButtonEventArgs e) => ShowLocalisation();
        private void GoToDates(object sender, MouseButtonEventArgs e) => ShowDates();
    }
}
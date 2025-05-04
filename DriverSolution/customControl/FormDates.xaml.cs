using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DriverSolution.models;
using DriverSolution.pages;

namespace DriverSolution.customControl
{
    public partial class FormDates : UserControl
    {
        private readonly PageCreationEvenement _parent;
        private readonly AppEvent _currentEvent;
        private readonly Func<AppEvent, Task> _onSubmit;
        private readonly Action _onBack;

        public FormDates(PageCreationEvenement parent, 
                        AppEvent eventData, 
                        Func<AppEvent, Task> onSubmit,
                        Action onBack)
        {
            InitializeComponent();
            _parent = parent;
            _currentEvent = eventData ?? throw new ArgumentNullException(nameof(eventData));
            _onSubmit = onSubmit ?? throw new ArgumentNullException(nameof(onSubmit));
            _onBack = onBack ?? throw new ArgumentNullException(nameof(onBack));

            InitializeForm();
        }

        private void InitializeForm()
        {
            // Initialiser les contrôles avec les données existantes
            cbEventType.SelectedIndex = _currentEvent.Recurrence == AppEvent.RecurrenceType.Unique ? 0 : 1;
            dpStartDate.SelectedDate = _currentEvent.StartDateTime.Date;
            tbStartTime.Text = _currentEvent.StartDateTime.ToString("HH:mm");
            
            if (_currentEvent.EndDateTime.HasValue)
            {
                dpEndDate.SelectedDate = _currentEvent.EndDateTime.Value.Date;
            }
        }

        private void EventType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isUniqueEvent = cbEventType.SelectedIndex == 0;
            cardEndDate.Visibility = isUniqueEvent ? Visibility.Visible : Visibility.Collapsed;
            cardFirstSession.Visibility = isUniqueEvent ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _onBack?.Invoke();
        }

        private async void Next_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateForm())
                    return;

                UpdateEventFromForm();
                await _onSubmit?.Invoke(_currentEvent);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateForm()
        {
            // Validation de la date de début
            if (dpStartDate.SelectedDate == null)
            {
                MessageBox.Show("Veuillez sélectionner une date de début", "Champ requis", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Validation de l'heure de début
            if (!TimeSpan.TryParse(tbStartTime.Text, out _))
            {
                MessageBox.Show("Veuillez entrer une heure valide (format HH:mm)", "Format invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Validation spécifique aux événements uniques
            if (cbEventType.SelectedIndex == 0 && dpEndDate.SelectedDate.HasValue)
            {
                if (dpEndDate.SelectedDate < dpStartDate.SelectedDate)
                {
                    MessageBox.Show("La date de fin ne peut pas être antérieure à la date de début", "Incohérence de dates", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            return true;
        }

        private void UpdateEventFromForm()
        {
            // Type de récurrence
            _currentEvent.Recurrence = cbEventType.SelectedIndex == 0 
                ? AppEvent.RecurrenceType.Unique 
                : AppEvent.RecurrenceType.A_Seances;

            // Date et heure de début
            var startDate = dpStartDate.SelectedDate ?? DateTime.Today;
            var startTime = TimeSpan.Parse(tbStartTime.Text);
            _currentEvent.StartDateTime = startDate.Add(startTime);

            // Date de fin (pour événements uniques)
            if (cbEventType.SelectedIndex == 0)
            {
                _currentEvent.EndDateTime = dpEndDate.SelectedDate?.Add(startTime);
            }
            else
            {
                _currentEvent.EndDateTime = null;
            }
        }
    }
    
}
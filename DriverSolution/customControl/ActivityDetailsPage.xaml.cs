using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using DriverSolution.models;
using System.Linq;
using MaterialDesignThemes.Wpf;

namespace DriverSolution.customControl
{
    public partial class ActivityDetailsPage : Window
    {
        private  DispatcherTimer _timer;
        private Activity _currentActivity;
        private List<Speaker> _speakers;
        private Grid ProgressBarContainer => (Grid)FindName("ProgressBarContainer");
        public List<TimeSlotViewModel> TimeSlots { get; set; }
        private List<Speaker> _speakersList = new List<Speaker>();
        public List<Speaker> SpeakersList
        {
            get => _speakersList;
            set
            {
                _speakersList = value ?? new List<Speaker>();
                OnPropertyChanged(nameof(SpeakersList));
            }
        }
        public ActivityDetailsPage(Activity activity)
        {
            InitializeComponent();
    
            if (activity == null)
            {
                MessageBox.Show("L'activité fournie est invalide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            _currentActivity = activity;
            SpeakersList = new List<Speaker>(); // Initialisation de la liste
            TimeSlots = new List<TimeSlotViewModel>(); // Initialisation des créneaux

            InitializeTimer();
            LoadActivityData();
            InitializeTimeSlots();
            LoadSpeakers();
        }
        private async void LoadSpeakers()
        {
            try
            {
                if (_currentActivity?.Speakers == null) return;
        
                var appwriteService = AppwriteService.Instance;
                var allSpeakers = await appwriteService.GetAvailableSpeakersAsync(_currentActivity.EventId);
        
                SpeakersList = allSpeakers?
                    .Where(s => _currentActivity.Speakers.Contains(s.Id))
                    .ToList() ?? new List<Speaker>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des intervenants: {ex.Message}", 
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                SpeakersList = new List<Speaker>();
            }
        }
        private void InitializeTimer()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (sender, e) => UpdateTimeData();
            _timer.Start();
        }

        private void LoadActivityData()
        {
            try
            {
                ActivityTitle.Text = _currentActivity?.Name ?? "Titre non disponible";
                DescriptionText.Text = _currentActivity?.Description ?? "Aucune description";
                LocationText.Text = _currentActivity?.Location ?? "Localisation non spécifiée";
        
               
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des données: {ex.Message}", 
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void UpdateTimeData()
        {
            UpdateCountdown();
            UpdateTimeBars();
            UpdateTimeSlotsStatus();
        }

        private void UpdateCountdown()
        {
            var now = DateTime.Now;
            var timeLeft = _currentActivity.StartDateTime - now;
            
            if (now < _currentActivity.StartDateTime)
            {
                // Activité à venir
                CountdownText.Text = $"{timeLeft.Days}j {timeLeft.Hours}h {timeLeft.Minutes}m";
                TimelineStatusText.Text = "À VENIR";
                TimelineStatusIcon.Kind = PackIconKind.ClockAlertOutline;
            }
            else if (now >= _currentActivity.StartDateTime && now <= _currentActivity.EndDateTime)
            {
                // Activité en cours
                CountdownText.Text = "EN COURS";
                TimelineStatusText.Text = "EN COURS";
                TimelineStatusIcon.Kind = PackIconKind.ClockCheckOutline;
            }
            else
            {
                // Activité terminée
                CountdownText.Text = "TERMINÉ";
                TimelineStatusText.Text = "TERMINÉ";
                TimelineStatusIcon.Kind = PackIconKind.ClockOutline;
                _timer.Stop();
            }
        }

        private void UpdateTimeBars()
        {
            var now = DateTime.Now;
            var totalDuration = (_currentActivity.EndDateTime - _currentActivity.StartDateTime).TotalSeconds;
            var containerWidth = ProgressBarContainer.ActualWidth;
    
            if (containerWidth <= 0) return;

            if (now < _currentActivity.StartDateTime)
            {
                PastTimeBar.Width = 0;
                FutureTimeBar.Width = containerWidth;
            }
            else if (now > _currentActivity.EndDateTime)
            {
                PastTimeBar.Width = containerWidth;
                FutureTimeBar.Width = 0;
            }
            else
            {
                var elapsed = (now - _currentActivity.StartDateTime).TotalSeconds;
                var percentage = Math.Min(1, Math.Max(0, elapsed / totalDuration));
        
                PastTimeBar.Width = containerWidth * percentage;
                FutureTimeBar.Width = containerWidth * (1 - percentage);
            }
        }
        private void InitializeTimeSlots()
        {
            TimeSlots = new List<TimeSlotViewModel>();
            var currentTime = _currentActivity.StartDateTime;
            var endTime = _currentActivity.EndDateTime;

            while (currentTime < endTime)
            {
                var slotDuration = TimeSpan.FromMinutes(30); // Créneaux de 30 minutes
                var nextTime = currentTime.Add(slotDuration);
                if (nextTime > endTime) nextTime = endTime;

                TimeSlots.Add(new TimeSlotViewModel
                {
                    StartTime = currentTime,
                    EndTime = nextTime,
                    TimeLabel = $"{currentTime:HH:mm} - {nextTime:HH:mm}",
                    Status = "À VENIR",
                    TimeStatusColor = "#4CAF50" // Vert
                });

                currentTime = nextTime;
            }

            UpdateTimeSlotsStatus();
        }

        private void UpdateTimeSlotsStatus()
        {
            var now = DateTime.Now;
            foreach (var slot in TimeSlots)
            {
                if (now > slot.EndTime)
                {
                    slot.Status = "TERMINÉ";
                    slot.TimeStatusColor = "#FF5252"; // Rouge
                }
                else if (now >= slot.StartTime && now <= slot.EndTime)
                {
                    slot.Status = "EN COURS";
                    slot.TimeStatusColor = "#FFC107"; // Orange
                }
                else
                {
                    slot.Status = "À VENIR";
                    slot.TimeStatusColor = "#4CAF50"; // Vert
                }
            }

            OnPropertyChanged(nameof(TimeSlots));
        }

        private async void CancelActivityButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Voulez-vous vraiment annuler cette activité ?",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var appwriteService = AppwriteService.Instance;
                    await appwriteService.UpdateActivityStatusAsync(_currentActivity.Id, ActivityStatus.Annule);
                    
                    MessageBox.Show("Activité annulée avec succès", "Succès",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'annulation: {ex.Message}",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OnlineLink_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentActivity.OnlineLink))
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = _currentActivity.OnlineLink,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Impossible d'ouvrir le lien: {ex.Message}");
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }

    public class TimeSlotViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string TimeLabel { get; set; }

        private string _status;
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        private string _timeStatusColor;
        public string TimeStatusColor
        {
            get => _timeStatusColor;
            set
            {
                _timeStatusColor = value;
                OnPropertyChanged(nameof(TimeStatusColor));
            }
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}
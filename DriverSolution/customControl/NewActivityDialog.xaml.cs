namespace DriverSolution.customControl;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DriverSolution.models;

public partial class NewActivityDialog : Window
{
    private readonly string _eventId;
    private List<SpeakerCheckItem> _availableSpeakers = new List<SpeakerCheckItem>();

    public NewActivityDialog(string eventId)
    {
        InitializeComponent();
        _eventId = eventId;
        Loaded += NewActivityDialog_Loaded;

        // Définir les dates par défaut (maintenant + 1h)
        StartDatePicker.SelectedDate = DateTime.Now;
        StartTimePicker.SelectedTime = DateTime.Now;
        EndDatePicker.SelectedDate = DateTime.Now;
        EndTimePicker.SelectedTime = DateTime.Now.AddHours(1);
    }

    private async void NewActivityDialog_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadAvailableSpeakers();
    }

    private async Task LoadAvailableSpeakers()
    {
        try
        {
            var appwriteService = AppwriteService.Instance;
            // Implémentez cette méthode dans votre AppwriteService
            var speakers = await appwriteService.GetAvailableSpeakersAsync(eventId: _eventId);

            _availableSpeakers = speakers.Select(s => new SpeakerCheckItem
            {
                Id = s.Id,
                Name = s.Name,
                IsSelected = false
            }).ToList();

            SpeakersListView.ItemsSource = _availableSpeakers;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors du chargement des intervenants: {ex.Message}",
                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
 private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateInputs()) return;

        try
        {
            var startDateTime = CombineDateAndTime(StartDatePicker.SelectedDate, StartTimePicker.SelectedTime);
            var endDateTime = CombineDateAndTime(EndDatePicker.SelectedDate, EndTimePicker.SelectedTime);

            var newActivity = new Activity
            {
                EventId = _eventId,
                Name = NameTextBox.Text,
                Description = DescriptionTextBox.Text,
                StartDateTime = startDateTime,
                EndDateTime = endDateTime,
                Location = LocationTextBox.Text,
                OnlineLink = OnlineLinkTextBox.Text,
                Speakers = _availableSpeakers.Where(s => s.IsSelected).Select(s => s.Name).ToList(),
                Status = ActivityStatus.Planifie
            };

            var appwriteService = AppwriteService.Instance;
            await appwriteService.CreateActivityAsync(newActivity);

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erreur lors de la création: {ex.Message}",
                "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private bool ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            MessageBox.Show("Le nom de l'activité est requis", "Validation",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (StartDatePicker.SelectedDate == null || StartTimePicker.SelectedTime == null ||
            EndDatePicker.SelectedDate == null || EndTimePicker.SelectedTime == null)
        {
            MessageBox.Show("Les dates de début et fin sont requises", "Validation",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        var startDateTime = CombineDateAndTime(StartDatePicker.SelectedDate, StartTimePicker.SelectedTime);
        var endDateTime = CombineDateAndTime(EndDatePicker.SelectedDate, EndTimePicker.SelectedTime);

        if (endDateTime <= startDateTime)
        {
            MessageBox.Show("La date de fin doit être après la date de début", "Validation",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    private DateTime CombineDateAndTime(DateTime? date, DateTime? time)
    {
        if (!date.HasValue || !time.HasValue)
            throw new ArgumentException("Date et heure requises");

        return new DateTime(
            date.Value.Year, date.Value.Month, date.Value.Day,
            time.Value.Hour, time.Value.Minute, time.Value.Second);
    }
}

public class SpeakerCheckItem
{
    public string Id { get; set; }
    public string Name { get; set; }
    public bool IsSelected { get; set; }
}
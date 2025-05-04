using System;
using System.Windows;
using System.Windows.Controls;
using DriverSolution.models;
using DriverSolution.pages;

namespace DriverSolution.customControl
{
    public partial class FormLocalisation : UserControl
    {
        private readonly PageCreationEvenement _parent;
        private readonly AppEvent _currentEvent;
        private readonly Action<AppEvent> _onNext;
        private readonly Action _onBack;

        public FormLocalisation(PageCreationEvenement parent, 
                              AppEvent eventData, 
                              Action<AppEvent> onNext,
                              Action onBack)
        {
            InitializeComponent();
            _parent = parent;
            _currentEvent = eventData ?? throw new ArgumentNullException(nameof(eventData));
            _onNext = onNext ?? throw new ArgumentNullException(nameof(onNext));
            _onBack = onBack ?? throw new ArgumentNullException(nameof(onBack));

            InitializeForm();
        }

        private void InitializeForm()
        {
            // Initialiser les contrôles avec les données existantes
            if (_currentEvent.EventLocationType == AppEvent.LocationType.Online)
            {
                comboLocationType.SelectedIndex = 1;
                txtOnlineLink.Text = _currentEvent.Link;
                txtInstructions.Text = _currentEvent.ConnectionInstructions;
            }
            else if (_currentEvent.EventLocationType == AppEvent.LocationType.Physique)
            {
                comboLocationType.SelectedIndex = 0;
                txtPlaceName.Text = _currentEvent.PlaceName;
                txtAddress.Text = _currentEvent.Address;
            }
            else
            {
                comboLocationType.SelectedIndex = 2; // Hybride
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            _onBack?.Invoke();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateForm())
                    return;

                UpdateEventFromForm();
                _onNext?.Invoke(_currentEvent);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateForm()
        {
            var selectedLocationType = comboLocationType.SelectedIndex switch
            {
                0 => AppEvent.LocationType.Physique,
                1 => AppEvent.LocationType.Online,
                _ => AppEvent.LocationType.Hybride
            };

            if (selectedLocationType != AppEvent.LocationType.Online)
            {
                if (string.IsNullOrWhiteSpace(txtPlaceName.Text))
                {
                    MessageBox.Show("Le nom du lieu est requis pour les événements physiques", 
                                  "Champ requis", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }
            else
            {
                if (!Uri.TryCreate(txtOnlineLink.Text, UriKind.Absolute, out _))
                {
                    MessageBox.Show("Veuillez entrer un lien de visioconférence valide", 
                                  "Lien invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            return true;
        }

        private void UpdateEventFromForm()
        {
            _currentEvent.EventLocationType = comboLocationType.SelectedIndex switch
            {
                0 => AppEvent.LocationType.Physique,
                1 => AppEvent.LocationType.Online,
                _ => AppEvent.LocationType.Hybride
            };

            if (_currentEvent.EventLocationType != AppEvent.LocationType.Online)
            {
                _currentEvent.PlaceName = txtPlaceName.Text;
                _currentEvent.Address = txtAddress.Text;
                _currentEvent.Link = null;
                _currentEvent.ConnectionInstructions = null;
            }
            else
            {
                _currentEvent.Link = txtOnlineLink.Text;
                _currentEvent.ConnectionInstructions = txtInstructions.Text;
                _currentEvent.PlaceName = null;
                _currentEvent.Address = null;
            }
        }
    }
}
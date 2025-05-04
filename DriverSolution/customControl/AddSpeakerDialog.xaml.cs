using System;
using System.Windows;
using DriverSolution.models;

namespace DriverSolution.customControl
{
    public partial class AddSpeakerDialog : Window
    {
        public Speaker NewSpeaker { get; private set; }

        public AddSpeakerDialog()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                MessageBox.Show("Veuillez remplir tous les champs", "Validation",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                NewSpeaker = new Speaker
                {
                    Name = NameTextBox.Text,
                    Mail = EmailTextBox.Text
                };

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la création: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
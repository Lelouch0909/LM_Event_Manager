using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DriverSolution.models;
using DriverSolution.pages;
using Microsoft.Win32;

namespace DriverSolution.customControl
{
    public partial class FormInformations : UserControl
    {
        private AppEvent _currentEvent;

        public AppEvent CurrentEvent
        {
            get => _currentEvent;
            set
            {
                _currentEvent = value;
                UpdateFormFields();
            }
        }

        private readonly Action<AppEvent> _onNext;
        private readonly PageCreationEvenement _parent;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 Mo

        public FormInformations(PageCreationEvenement parent, AppEvent eventData, Action<AppEvent> onNext)
        {
            InitializeComponent();
            _parent = parent;
            _onNext = onNext ?? throw new ArgumentNullException(nameof(onNext));
            CurrentEvent = eventData ?? new AppEvent();

            InitializeForm();
        }

        private void InitializeForm()
        {
            comboEventType.SelectedIndex = 0;

            if (CurrentEvent != null)
            {
                txtEventName.Text = CurrentEvent.Name;
                txtDescription.Text = CurrentEvent.Description;

                if (!string.IsNullOrEmpty(CurrentEvent.Type))
                {
                    var item = comboEventType.Items.Cast<ComboBoxItem>()
                        .FirstOrDefault(i => i.Content.ToString() == CurrentEvent.Type);
                    if (item != null)
                    {
                        comboEventType.SelectedItem = item;
                    }
                }
            }
        }

        private void UpdateFormFields()
        {
            if (CurrentEvent == null) return;

            txtEventName.Text = CurrentEvent.Name;
            txtDescription.Text = CurrentEvent.Description;
        }

        private async void Next_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateForm())
                    return;

                await UpdateEventFromForm();

                _onNext(CurrentEvent);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                MessageBox.Show($"Erreur: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateEventFromForm()
        {
            var appwriteService = AppwriteService.Instance;
            var currentUser = await appwriteService.GetCurrentUserAsync();

            // Upload de l'image si elle existe
            if (!string.IsNullOrEmpty(CurrentEvent.ImageUrl))
            {
                CurrentEvent.ImageUrl = await appwriteService.UploadEventImageAsync(CurrentEvent.ImageUrl);
            }
            else
            {
                throw new Exception("vous  devez ajouter un visuel de votre evenement !");
            }


            CurrentEvent.Name = txtEventName.Text.Trim();
            CurrentEvent.Type = (comboEventType.SelectedItem as ComboBoxItem)?.Content.ToString();
            CurrentEvent.Description = txtDescription.Text.Trim();
            CurrentEvent.IdUser = currentUser?.Id;
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtEventName.Text))
            {
                MessageBox.Show("Le nom de l'événement est obligatoire", "Champ requis", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private async void OnImageChanged(string newImagePath)
        {
            try
            {
                var service = new AppwriteService();
                CurrentEvent.ImageUrl = await service.UploadEventImageAsync(newImagePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur image: {ex.Message}");
            }
        }

        private void ImageDropBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog();
        }

        private void OpenFileDialog()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Images|*.jpg;*.jpeg;*.png;*.gif",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                HandleSelectedFile(openFileDialog.FileName);
            }
        }

        private void Border_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files?.Length == 0)
                return;

            HandleSelectedFile(files[0]);
        }

        private void HandleSelectedFile(string filePath)
        {
            if (!IsFileValid(filePath))
            {
                MessageBox.Show("Le fichier doit être une image (.jpg, .jpeg, .png, .gif) de moins de 10 Mo.",
                    "Format invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var bitmap = new BitmapImage(new Uri(filePath));

                ImageDropBorder.Background = new ImageBrush(bitmap)
                {
                    Stretch = Stretch.UniformToFill,
                    Opacity = 0.3
                };

                CurrentEvent.ImageUrl = filePath;

                MessageBox.Show("Image chargée avec succès !", "Succès", MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement de l'image: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Border_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files?.Length > 0 && IsFileValid(files[0]))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }

            e.Handled = true;
        }

        private void Border_DragLeave(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }


        private bool IsFileValid(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            var extension = Path.GetExtension(filePath)?.ToLower();
            if (!_allowedExtensions.Contains(extension))
                return false;

            var fileInfo = new FileInfo(filePath);
            return fileInfo.Exists && fileInfo.Length <= MaxFileSizeBytes;
        }
    }
}
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Media;
using DriverSolution.models;
using System;
using System.Windows;
using DriverSolution.pages;
using System.Threading.Tasks;
using System.ComponentModel;

namespace DriverSolution.customControl
{
    public partial class PageStatistics : UserControl, INotifyPropertyChanged
    {
        private AppEvent _currentEvent;
        private long _memberCount;
        private long _activityCount;
        private bool _isLoading;

        public AppEvent CurrentEvent 
        { 
            get => _currentEvent;
            private set
            {
                _currentEvent = value;
                OnPropertyChanged(nameof(CurrentEvent));
            }
        }

        public long MemberCount 
        { 
            get => _memberCount;
            set
            {
                _memberCount = value;
                OnPropertyChanged(nameof(MemberCount));
            }
        }

        public long ActivityCount 
        { 
            get => _activityCount;
            set
            {
                _activityCount = value;
                OnPropertyChanged(nameof(ActivityCount));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public SeriesCollection MemberGrowthSeries { get; set; }
        public List<string> GrowthDates { get; set; }
        public SeriesCollection GenderSeries { get; set; }
        public List<string> Genders { get; set; }

        public PageStatistics()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += PageStatistics_Loaded;
        }

        private void PageStatistics_Loaded(object sender, RoutedEventArgs e)
        {
            if (TryFindParent<PageMain>(this) is PageMain pageMain)
            {
                CurrentEvent = pageMain.ActiveEvent;
                LoadStatistics();
            }
        }

        private async void LoadStatistics()
        {
            if (CurrentEvent == null) return;

            IsLoading = true;
            try
            {
                await LoadStatsFromAppwrite();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des statistiques: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadStatsFromAppwrite()
        {
            var appwriteService = AppwriteService.Instance;

            // 1. Statistiques de base
            MemberCount = await appwriteService.GetEventMemberCount(CurrentEvent.Id);
            ActivityCount = await appwriteService.GetActivitiesCount(CurrentEvent.Id);

            // 2. Évolution des membres
            var growthData = await appwriteService.GetMemberGrowthOverTime(CurrentEvent.Id);
            MemberGrowthSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Membres",
                    Values = new ChartValues<int>(growthData.Counts),
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 10,
                    Stroke = Brushes.DodgerBlue,
                    Fill = Brushes.Transparent
                }
            };
            GrowthDates = growthData.Dates;

            // 3. Répartition par genre
            var genderStats = await appwriteService.GetGenderDistribution(CurrentEvent.Id);
            GenderSeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Hommes",
                    Values = new ChartValues<int> { genderStats.MaleCount },
                    Fill = Brushes.DodgerBlue
                },
                new ColumnSeries
                {
                    Title = "Femmes",
                    Values = new ChartValues<int> { genderStats.FemaleCount },
                    Fill = Brushes.DeepPink
                }
            };
            Genders = new List<string> { "Hommes", "Femmes" };

            // Rafraîchir les bindings
            OnPropertyChanged(nameof(MemberCount));
            OnPropertyChanged(nameof(ActivityCount));
            OnPropertyChanged(nameof(MemberGrowthSeries));
            OnPropertyChanged(nameof(GrowthDates));
            OnPropertyChanged(nameof(GenderSeries));
            OnPropertyChanged(nameof(Genders));
        }

        private static T TryFindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null && !(child is T))
            {
                child = VisualTreeHelper.GetParent(child);
            }
            return child as T;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
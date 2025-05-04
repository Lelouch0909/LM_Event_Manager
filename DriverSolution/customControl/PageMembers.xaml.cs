using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DriverSolution.models;
using DriverSolution.pages;

namespace DriverSolution.customControl;

public partial class PageMembers : UserControl
{
    private readonly AppwriteService _appwriteService;
    private string _currentEventId;

    public PageMembers()
    {
        InitializeComponent();
        _appwriteService = AppwriteService.Instance;
        Loaded += PageMembers_Loaded;
    }


    private void PageMembers_Loaded(object sender, RoutedEventArgs e)
    {
        if (TryFindParent<PageMain>(this) is PageMain pageMain)
        {
            _currentEventId = pageMain.ActiveEvent?.Id;
            LoadMembers();
        }
    }
    private async void LoadMembers()
    {
        if (string.IsNullOrEmpty(_currentEventId))
        {
            MembersList.ItemsSource = new List<Member>();
            return;
        }

        var members = await _appwriteService.GetEventMembersAsync(_currentEventId);
        MembersList.ItemsSource = members;
    }

    private static T TryFindParent<T>(DependencyObject child) where T : DependencyObject
    {
        while (child != null && !(child is T))
        {
            child = VisualTreeHelper.GetParent(child);
        }
        return child as T;
    }

}
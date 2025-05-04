using System.Windows.Controls;

namespace DriverSolution.services;
public class NavigationService : INavigationService
{
    private readonly Stack<UserControl> _navigationStack = new Stack<UserControl>();
    private readonly ContentControl _contentHost;

    public NavigationService(ContentControl contentHost)
    {
        _contentHost = contentHost;
    }

    public void NavigateTo(UserControl page)
    {
        if (_contentHost.Content != null)
        {
            _navigationStack.Push((UserControl)_contentHost.Content);
        }
        _contentHost.Content = page;
    }

    public void NavigateBack()
    {
        if (_navigationStack.Count > 0)
        {
            _contentHost.Content = _navigationStack.Pop();
        }
    }
}
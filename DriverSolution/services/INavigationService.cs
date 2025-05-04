using System.Windows.Controls;

namespace DriverSolution.services;

public interface INavigationService
{
        void NavigateTo(UserControl page);
        void NavigateBack();
    
}
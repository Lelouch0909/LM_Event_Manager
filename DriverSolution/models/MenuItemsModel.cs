using System.Windows.Input;

namespace DriverSolution.models;

public class MenuItemsModel
{
    public String Title { get; set; }
    public String IconKind  { get; set; }
    
    public Action ClickAction { get; set; }
}
using CommunityToolkit.Maui.Views;

namespace MauiSample.Pages.Popups;

public class LoadingPopup : Popup
{
	public LoadingPopup()
	{
		this.Color = Microsoft.Maui.Graphics.Colors.Transparent;
		this.CanBeDismissedByTappingOutsideOfPopup = false;

		Content = new Grid
		{
			Children = {
				new ActivityIndicator {
					IsRunning = true, IsEnabled = true,
					HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center
				}
			}
		};
	}

	static public async Task ShowWhile(Page page, Task task)
	{
		var p = new LoadingPopup();
		page.ShowPopup(p);
		await task;
		p.Close();
	}
    static public async Task<T> ShowWhile<T>(Page page, Task<T> task)
    {
        var p = new LoadingPopup();
        page.ShowPopup(p);
        var res = await task;
        p.Close();
		return res;
    }

}
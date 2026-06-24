
using System.Text;
using ProjectManagementSystem.Client;
using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Navigation;
using ProjectManagementSystem.Client.Session;
using ProjectManagementSystem.Client.Screens;

Console.Title = ClientDefaults.ApplicationTitle;
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var session       = new SessionContext();
var api           = new ApiClient(ClientDefaults.ApiBaseUrl);
var screenFactory = new ScreenFactory(api, session);
var router        = new ScreenRouter(screenFactory, session);
var start         = new StartScreen(api, session);

while (true)
{
    await start.ShowAsync();           

    if (session.IsLoggedIn)
        await router.RouteAsync();     

}


// See https://aka.ms/new-console-template for more information
using System.Text;
using ProjectManagementSystem.Client;
using ProjectManagementSystem.Client.Api;
using ProjectManagementSystem.Client.Session;
using ProjectManagementSystem.Client.Screens;

// -- Bootstrap -----------------------------------------------------------------
Console.Title = "Project & Resource Management Tool";
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

const string apiBaseUrl = "https://localhost:7171/";

var session = new SessionContext();
var api     = new ApiClient(apiBaseUrl);
var router  = new ScreenRouter(api, session);
var start   = new StartScreen(api, session);

// -- Main Loop -----------------------------------------------------------------
while (true)
{
    await start.ShowAsync();           // blocks until login succeeds

    if (session.IsLoggedIn)
        await router.RouteAsync();     // role-based menu

    // After logout, loop back to start screen
}


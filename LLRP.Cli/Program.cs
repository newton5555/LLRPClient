using System;
using Terminal.Gui.App;

namespace LLRP.Cli;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            using IApplication app = Application.Create();
            app.Init();
            
            var win = new AppWindow(app);
            app.Run(win);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}

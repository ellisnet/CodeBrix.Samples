using CodeBrix.Platform.UI.Hosting;
using PicoScope.Brix.ScopeData;
using PicoScope.Brix.ScopeData.Ps2000;
using PicoScope.Brix.ScopeData.Simulation;
using System;

namespace PicoScope.Brix;

internal class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App.InitializeLogging();

        //This head decides which scope implementations the application can use.
        //  Registration order does not matter: FindBest() prefers real hardware
        //  and falls back to the simulator when nothing is plugged in.
        ScopeDeviceFinder.Register(new Ps2000ScopeDataDevice());
        ScopeDeviceFinder.Register(new SimulatedScopeDataDevice());

        var host = CodeBrixPlatformHostBuilder.Create()
            .App(() => new App())
            .UseLinuxFrameBuffer()
            .UseDirectSkiaCanvasMode() //Experimental - should be safe to leave enabled
            .Build();

        host.Run();
    }
}

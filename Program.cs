using WashingMachine.UI;
using WashingMachine.Services;
using WashingMachine.Models;

namespace WashingMachine
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            WashingConfiguration configurations = new();
            WashingService service = new WashingService(configurations);
            Dashboard dashboard = new Dashboard(service);
            await dashboard.Execute();
        }
    }
}

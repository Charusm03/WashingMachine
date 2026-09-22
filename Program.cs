using WashingMachine.UI;
using WashingMachine.Services;
using WashingMachine.Models;

namespace WashingMachine
{
    internal class Program
    {
        static void Main(string[] args)
        {
            WashingConfiguration configurations = new();
            WashingService service = new WashingService(configurations);
            Dashboard dashboard = new Dashboard(service);
            dashboard.Execute();
        }
    }
}

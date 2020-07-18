using System.Threading.Tasks;

namespace DarlingBotNet
{
    internal class SystemStart
    {
        public static Task Main(string[] args)
        {
            return SystemSingleTone.RunAsync(args);
        }
    }
}
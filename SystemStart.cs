using System.Reflection;
using System.Threading.Tasks;

namespace DarlingBotNet
{
    class SystemStart
    {
        public static Task Main(string[] args) => SystemSingleTone.RunAsync(args);
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DevelopTools
{
    public class GamesLoader
    {
        public static DirectoryInfo GetDllPath()
        {
            return new DirectoryInfo(Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path)));
        }
    }
}

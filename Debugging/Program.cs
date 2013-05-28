using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace Debugging
{
    class Program
    {
        static void Main(string[] args)
        {
            // var asd = GetDataToRender();
        }


        //static List<T4TS.TypeScriptModule> GetDataToRender() {
        //    DTE dte = null;
        //    dte = (DTE)GetService(typeof(DTE));
        //    // Get the DTE service from the host
        //    var serviceProvider = Host as IServiceProvider;
        //    if (serviceProvider != null)
        //        dte = serviceProvider.GetService(typeof(SDTE)) as DTE;

        //    // Fail if we couldn't get the DTE. This can happen when trying to run in TextTransform.exe
        //    if (dte == null)
        //        throw new Exception("Can only execute through the Visual Studio host");

        //    var project = GetProjectContainingT4File(dte);
        
        //    if (project == null)
        //        throw new Exception("Could not find the VS project containing the T4TS file.");

        //    // Read settings from T4TS.tt.settings.tt
        //    var settings = new T4TS.Settings
        //    {
        //        DefaultModule = DefaultModule,
        //        DefaultOptional = DefaultOptional,
        //        DefaultCamelCaseMemberNames = DefaultCamelCaseMemberNames,
        //        DefaultInterfaceNamePrefix = DefaultInterfaceNamePrefix
        //    };

        //    var generator = new T4TS.CodeTraverser(new Project(), settings);

        //    return generator.GetAllInterfaces().ToList();
        //}
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace T4TS
{
    public class TypeScriptInterfaceAttributeValues
    {
        public string Module { get; set; }
        public string Name { get; set; }
        public string NamePrefix { get; set; }
        
        /// <summary>
        /// If true, typescript interfaces will be created also for all the baseclasses of the class. 
        /// The interfaces will be generated even if the baseclasses aren't decorated with the [TypeScriptInterface] -Attribute,
        /// and even if the class is external to the project. Defaults to false.
        /// </summary>
        public bool CreateTypeScriptInterfacesAlsoForBaseClasses { get; set; }
    }
}

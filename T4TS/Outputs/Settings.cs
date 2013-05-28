using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace T4TS
{
    public class Settings
    {
        /// <summary>
        /// The default module of the generated interface, if not specified by the TypeScriptInterfaceAttribute
        /// </summary>
        public string DefaultModule { get; set; }

        /// <summary>
        /// The default value for Optional, if not specified by the TypeScriptMemberAttribute
        /// </summary>
        public bool DefaultOptional { get; set; }

        /// <summary>
        /// The default value for the CamelCase flag for an interface member name, if not specified by the TypeScriptMemberAttribute
        /// </summary>
        public bool DefaultCamelCaseMemberNames { get; set; }

        /// <summary>
        /// The default string to prefix interface names with. For instance, you might want to prefix the names with an "I" to get conventional interface names.
        /// </summary>
        public string DefaultInterfaceNamePrefix { get; set; }

        /// <summary>
        /// If true, typescript interfaces will be created also for all the baseclasses of the class. 
        /// The interfaces will be generated even if the baseclasses aren't decorated with the [TypeScriptInterface] -Attribute,
        /// and even if the class is external to the project.
        /// </summary>
        public bool DefaultCreateTypeScriptInterfacesAlsoForBaseClasses { get; set; }
    }
}

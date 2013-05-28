using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace T4TS
{
    public class TypeScriptInterface
    {
        public string Name { get; set; }
        public string FullName { get; set; }

        public List<TypeScriptInterfaceMember> Members { get; set; }
        public TypescriptType IndexedType { get; set; }
        public TypeScriptInterface Parent { get; set; }
        public TypeScriptModule Module { get; set; }

        /// <summary>
        ///     If the TypeScriptInterface is a created directly from a class that is decorated as [TypeScriptInterface], then it is 
        ///     a DirectlyMarkedInterface. This means that the resulting TypeScriptInterface will be an "export"-interface. 
        ///     Note that a TypeScriptInterface may be both Directly and Indirectly marked.
        /// </summary>
        public bool IsDirectlyMarkedInterface { get; set; }

        /// <summary>
        ///     If the TypeScriptInterface is a base-class for class that is decorated as [TypeScriptInterface], then it is 
        ///     an IndirectlyMarkedInterface. This means that the resulting TypeScriptInterface will not be an "export"-interface, 
        ///     but instead, an interface used solely for module-scoped inheritance (if the TypeScriptInterface is not also 
        ///     DirectlyMarkedInterface).
        ///     Note that a TypeScriptInterface may be both Directly and Indirectly marked.
        /// </summary>
        public bool IsIndirectlyMarkedInterface { get; set; }

        /// <summary>
        ///     Should the resulting TypeScript-interface be presented as "export" (ie. "module-scoped") or not.
        /// </summary>
        public bool ShouldBeExportInterface { 
            get {
                return IsDirectlyMarkedInterface;
            } 
        }
        
        /// <summary>
        ///     A TypeScriptInterface may extend a BaseClassInterface (se above), only if it's marked as CanExtendAllBaseClasses == true.
        ///     With this we prevent unwanted extensions on classes not marked as CreateTypeScriptInterfacesAlsoForBaseClasses == true.
        /// </summary>
        public bool CanExtendAllBaseClasses { 
            get {
                return IsDirectlyMarkedInterface || IsIndirectlyMarkedInterface;
            } 
        }

        /// <summary>
        ///     The DocComment/XML-documentation (eg. the text you are reading!) of the related dataClass. Used to reveal class-comments
        ///     in the TypeScript-interfaces as well.
        /// </summary>
        public string DocComment { get; set; }

        public TypeScriptInterface()
        {
            Members = new List<TypeScriptInterfaceMember>();
        }
    }
}

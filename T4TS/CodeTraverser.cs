using EnvDTE;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace T4TS
{
    public class CodeTraverser
    {
        public Project Project { get; private set; }
        public Settings Settings { get; private set; }

        private static readonly string InterfaceAttributeFullName = "T4TS.TypeScriptInterfaceAttribute";
        private static readonly string MemberAttributeFullName = "T4TS.TypeScriptMemberAttribute";

        public CodeTraverser(Project project, Settings settings)
        {
            if (project == null)
                throw new ArgumentNullException("project");

            if (settings == null)
                throw new ArgumentNullException("settings");

            this.Project = project;
            this.Settings = settings;
        }

        public TypeContext BuildContext()
        {
            var typeContext = new TypeContext();

            new ProjectTraverser(this.Project, (ns) =>
            {
                new NamespaceTraverser(ns, (codeClass) =>
                {
                    CodeAttribute attribute;
                    if (!TryGetAttribute(codeClass.Attributes, InterfaceAttributeFullName, out attribute))
                        return;

                    BuildCustomType(codeClass, attribute, typeContext);
                });
            });

            return typeContext;
        }

        private void BuildCustomType(
            CodeClass codeClass,
            CodeAttribute originalTypeScriptInterfaceAttribute,
            TypeContext typeContext
        ) {
            foreach(CodeClass baseClassCodeClass in codeClass.Bases)
            {
                BuildCustomType(baseClassCodeClass, originalTypeScriptInterfaceAttribute, typeContext);
            }
            var typeScriptInterfaceAttributeValues = GetInterfaceValues(codeClass, originalTypeScriptInterfaceAttribute);
            // Make sure an overridden name from originalTypeScriptInterfaceAttribute never overrides the name of "baseClass-interface", ie. a codeClass without a TypeScriptInterfaceAttribute of it's own.
            // typeScriptInterfaceAttributeValues.Name = codeClass.Name;
            // typeScriptInterfaceAttributeValues.NamePrefix = Settings.DefaultInterfaceNamePrefix ?? string.Empty;

            var customType = new CustomType(GetInterfaceName(typeScriptInterfaceAttributeValues), typeScriptInterfaceAttributeValues.Module);
            typeContext.AddCustomType(codeClass.FullName, customType);
        }

        public IEnumerable<TypeScriptModule> GetAllInterfaces()
        {
            var typeContext = BuildContext();
            var byModuleName = new Dictionary<string, TypeScriptModule>();
            var tsMap = new Dictionary<CodeClass, TypeScriptInterface>();

            new ProjectTraverser(this.Project, (ns) =>
            {
                new NamespaceTraverser(ns, (codeClass) =>
                {
                    if (codeClass.Attributes == null || codeClass.Attributes.Count == 0)
                        return;

                    CodeAttribute attribute;
                    if (!TryGetAttribute(codeClass.Attributes, InterfaceAttributeFullName, out attribute))
                        return;

                    var values = GetInterfaceValues(codeClass, attribute);

                    TypeScriptModule module;
                    if (!byModuleName.TryGetValue(values.Module, out module))
                    {
                        module = new TypeScriptModule { QualifiedName = values.Module };
                        byModuleName.Add(values.Module, module);
                    }

                    BuildInterface(codeClass, typeContext, attribute, tsMap, module);
                });
            });

            var tsInterfaces = tsMap.Values.ToList();
            tsMap.Keys.ToList().ForEach(codeClass =>
            {
                var parent = tsInterfaces.LastOrDefault(intf => codeClass.IsDerivedFrom[intf.FullName] && intf.FullName != codeClass.FullName);
                var tsInterfaceLackingParent = tsMap[codeClass];
                if (
                    parent != null 
                    && (
                        // Parent-classes marked specifically TypeScriptInterface -attribute can always be used as parent.
                        !parent.IsIndirectlyMarkedInterface 
                        // Only interfaces marked with CanExtendAllBaseClasses can extend BaseClassInterface.
                        || tsInterfaceLackingParent.CanExtendAllBaseClasses
                    )
                )
                {
                    tsInterfaceLackingParent.Parent = parent;
                }
            });

            return byModuleName.Values
                .OrderBy(m => m.QualifiedName)
                .ToList();
        }
        
        private string GetInterfaceName(TypeScriptInterfaceAttributeValues attributeValues)
        {
            if (!string.IsNullOrEmpty(attributeValues.NamePrefix))
                return attributeValues.NamePrefix + attributeValues.Name;

            return attributeValues.Name;
        }

        /// <summary>
        /// A recursively used method which builds interfaces out of CodeClasses *and* their base-class -CodeClasses 
        /// (if instructed with [TypeScriptInterface(CreateTypeScriptInterfacesAlsoForBaseClasses = true)]).
        /// </summary>
        /// <param name="codeClass">The CodeClass for which to create a TypeScriptInterface.</param>
        /// <param name="typeContext"></param>
        /// <param name="originalTypeScriptInterfaceAttribute"></param>
        /// <param name="tsMap">The tsMap in which the created TypeScriptInterface will be stored.</param>
        /// <param name="module">The module in which the created TypeScriptInterface will be stored.</param>
        /// <param name="indirectlyMarkedInterface">A marker to indicate that a CodeClass with [TypeScriptInterface(CreateTypeScriptInterfacesAlsoForBaseClasses = true)] has been hit somewhere in previous recursion.</param>
        private void BuildInterface(
            CodeClass codeClass, 
            TypeContext typeContext, 
            CodeAttribute originalTypeScriptInterfaceAttribute, 
            Dictionary<CodeClass, TypeScriptInterface> tsMap, 
            TypeScriptModule module,
            bool indirectlyMarkedInterface = false
        ) 
        {
            var interfaceAlreadyExists = tsMap.ContainsKey(codeClass) || tsMap.Keys.Any(x => x.FullName == codeClass.FullName); 
            // There's no need to display the properties of System.Object, ie. the grand-base-class of all classes.
            var codeClassIsIrrelevant = codeClass.FullName == typeof(Object).FullName;
            if(interfaceAlreadyExists || codeClassIsIrrelevant)
            {
                return;
            }

            CodeAttribute specificTypeScriptInterfaceAttribute;
            var directlyMarkedInterface = TryGetAttribute(
                codeClass.Attributes, 
                InterfaceAttributeFullName, 
                out specificTypeScriptInterfaceAttribute
            );

            TypeScriptInterfaceAttributeValues typeScriptInterfaceAttributeValues;
            if (directlyMarkedInterface)
            {
                typeScriptInterfaceAttributeValues = GetInterfaceValues(codeClass, specificTypeScriptInterfaceAttribute);
                // This will set all the baseClasses of this class to be indirectlyMarked.
                indirectlyMarkedInterface = 
                    indirectlyMarkedInterface 
                    || typeScriptInterfaceAttributeValues.CreateTypeScriptInterfacesAlsoForBaseClasses;
            }
            else
            {
                // If no specific attribute was available, use the originalTypeScriptInterfaceAttribute as source for typeScriptInterfaceAttributeValues.
                typeScriptInterfaceAttributeValues = GetInterfaceValues(codeClass, originalTypeScriptInterfaceAttribute);
                // Make sure an overridden name from originalTypeScriptInterfaceAttribute never overrides the name of "baseClass-interface", ie. a codeClass without a TypeScriptInterfaceAttribute of it's own.
                typeScriptInterfaceAttributeValues.Name = codeClass.Name;
                typeScriptInterfaceAttributeValues.NamePrefix = Settings.DefaultInterfaceNamePrefix ?? string.Empty;
            }
            
            // Only indirectlyMarkedInterfaces build also their base-class -interfaces.
            if(indirectlyMarkedInterface) 
            {
                // First, create interfaces for the baseCodeClasses.
                foreach(CodeClass baseClassCodeClass in codeClass.Bases)
                {
                    BuildInterface(
                        baseClassCodeClass, 
                        typeContext, 
                        originalTypeScriptInterfaceAttribute, 
                        tsMap, 
                        module, 
                        true
                    );
                }
            }
            
            // Second, create interfaces for the CodeClass itself.
            var tsInterface = BuildInterface(
                codeClass, 
                typeScriptInterfaceAttributeValues, 
                typeContext, 
                directlyMarkedInterface,
                indirectlyMarkedInterface
            );

            tsMap.Add(codeClass, tsInterface);
            tsInterface.Module = module;
            module.Interfaces.Add(tsInterface);
        }

        private TypeScriptInterface BuildInterface(
            CodeClass codeClass,
            TypeScriptInterfaceAttributeValues attributeValues,
            TypeContext typeContext,
            bool directlyMarkedInterface,
            bool indirectlyMarkedInterface
        )
        {
            var tsInterface = new TypeScriptInterface
            {
                FullName = codeClass.FullName,
                Name = GetInterfaceName(attributeValues),
                IsIndirectlyMarkedInterface = indirectlyMarkedInterface,
                IsDirectlyMarkedInterface = directlyMarkedInterface
            };
            try {
                tsInterface.DocComment = codeClass.DocComment;
            }
            catch(Exception exception) {
                tsInterface.DocComment = "Error resolving CodeClass.DocComment.";
            }

            TypescriptType indexedType;
            if (TryGetIndexedType(codeClass, typeContext, out indexedType))
                tsInterface.IndexedType = indexedType;

            new ClassTraverser(codeClass, (property) =>
            {
                TypeScriptInterfaceMember member;
                if (TryGetMember(property, typeContext, out member))
                    tsInterface.Members.Add(member);
            });
            return tsInterface;
        }

        private bool TryGetAttribute(CodeElements attributes, string attributeFullName, out CodeAttribute attribute)
        {
            foreach (CodeAttribute attr in attributes)
            {
                if (attr.FullName == attributeFullName)
                {
                    attribute = attr;
                    return true;
                }
            }

            attribute = null;
            return false;
        }

        private bool TryGetIndexedType(CodeClass codeClass, TypeContext typeContext, out TypescriptType indexedType)
        {
            indexedType = null;
            if (codeClass.Bases == null || codeClass.Bases.Count == 0)
                return false;

            foreach (CodeElement baseClass in codeClass.Bases)
            {
                if (typeContext.IsGenericEnumerable(baseClass.FullName))
                {
                    string fullName = typeContext.UnwrapGenericType(baseClass.FullName);
                    indexedType = typeContext.GetTypeScriptType(fullName);
                    return true;
                }
            }

            return false;
        }

        private TypeScriptInterfaceAttributeValues GetInterfaceValues(CodeClass codeClass, CodeAttribute interfaceAttribute)
        {
            var values = GetAttributeValues(interfaceAttribute);

            return new TypeScriptInterfaceAttributeValues
            {
                Name = values.ContainsKey("Name") ? values["Name"] : codeClass.Name,
                Module = values.ContainsKey("Module") ? values["Module"] : Settings.DefaultModule ?? "T4TS",
                NamePrefix = values.ContainsKey("NamePrefix") ? values["NamePrefix"] : Settings.DefaultInterfaceNamePrefix ?? string.Empty,
                CreateTypeScriptInterfacesAlsoForBaseClasses = 
                    values.ContainsKey("CreateTypeScriptInterfacesAlsoForBaseClasses") ?
                    bool.Parse(values["CreateTypeScriptInterfacesAlsoForBaseClasses"]) :
                    Settings.DefaultCreateTypeScriptInterfacesAlsoForBaseClasses
            };
        }

        private bool TryGetMember(CodeProperty property, TypeContext typeContext, out TypeScriptInterfaceMember member)
        {
            member = null;
            if (property.Access != vsCMAccess.vsCMAccessPublic)
                return false;

            var getter = property.Getter;
            if (getter == null)
                return false;

            var values = GetMemberValues(property, typeContext);
            member = new TypeScriptInterfaceMember
            {
                Name = values.Name ?? property.Name,
                FullName = property.FullName,
                Optional = values.Optional,
                Type = (string.IsNullOrWhiteSpace(values.Type))
                    ? typeContext.GetTypeScriptType(getter.Type)
                    : new CustomType(values.Type)
            };

            if (values.CamelCase && values.Name == null)
                member.Name = member.Name.Substring(0, 1).ToLowerInvariant() + member.Name.Substring(1);

            return true;
        }

        private TypeScriptMemberAttributeValues GetMemberValues(CodeProperty property, TypeContext typeContext)
        {
            bool? attributeOptional = null;
            bool? attributeCamelCase = null;
            string attributeName = null;
            string attributeType = null;

            CodeAttribute attribute;
            if (TryGetAttribute(property.Attributes, MemberAttributeFullName, out attribute))
            {
                var values = GetAttributeValues(attribute);
                if (values.ContainsKey("Optional"))
                    attributeOptional = values["Optional"] == "true";

                if (values.ContainsKey("CamelCase"))
                    attributeCamelCase = values["CamelCase"] == "true";

                values.TryGetValue("Name", out attributeName);
                values.TryGetValue("Type", out attributeType);
            }

            return new TypeScriptMemberAttributeValues
            {
                Optional = attributeOptional.HasValue ? attributeOptional.Value : Settings.DefaultOptional,
                Name = attributeName,
                Type = attributeType,
                CamelCase = attributeCamelCase ?? Settings.DefaultCamelCaseMemberNames
            };
        }

        private Dictionary<string, string> GetAttributeValues(CodeAttribute codeAttribute)
        {
            var values = new Dictionary<string, string>();
            foreach (CodeElement child in codeAttribute.Children)
            {
                var property = (EnvDTE80.CodeAttributeArgument)child;
                if (property == null || property.Value == null)
                    continue;
                
                // remove quotes if the property is a string
                string val = property.Value ?? string.Empty;
                if (val.StartsWith("\"") && val.EndsWith("\""))
                    val = val.Substring(1, val.Length - 2);

                values.Add(property.Name, val);
            }

            return values;
        }
    }
}

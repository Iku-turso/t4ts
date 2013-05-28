using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace T4TS
{
    public class InterfaceOutputAppender : OutputAppender<TypeScriptInterface>
    {
        private bool InGlobalModule { get; set; }

        public InterfaceOutputAppender(StringBuilder output, int baseIndentation, bool inGlobalModule)
            : base(output, baseIndentation)
        {
            this.InGlobalModule = inGlobalModule;
        }

        public override void AppendOutput(TypeScriptInterface tsInterface)
        {
            BeginInterface(tsInterface);

            AppendMembers(tsInterface);
            
            if (tsInterface.IndexedType != null)
                AppendIndexer(tsInterface);

            EndInterface();
        }

        private void AppendMembers(TypeScriptInterface tsInterface)
        {
            var appender = new MemberOutputAppender(Output, BaseIndentation + 4);
            foreach (var member in tsInterface.Members)
                appender.AppendOutput(member);
        }

        private void BeginInterface(TypeScriptInterface tsInterface)
        {
            string docComment;
            try {
                var summary = XDocument.Parse(tsInterface.DocComment).Descendants("summary").FirstOrDefault();
                docComment = summary != null ? summary.ToString() : string.Empty;
            }
            catch (Exception) {
                // Revert to unformatted comments in case of incoherent xml.
                docComment = tsInterface.DocComment;
            }
            
            if(docComment != string.Empty)
            {
                AppendIndentedLine("/**");
                AppendIndentedLine("Generated from " + tsInterface.FullName);
                AppendMultipleLines(docComment);
                AppendIndentedLine("*/");
            }
            else {
                AppendIndentedLine("/** Generated from " + tsInterface.FullName + " */");
            }

            // If the interface is just a BaseClassInterface, no need to make it public (ie. "export" -interface) beyond the current module.
            if (InGlobalModule || !tsInterface.ShouldBeExportInterface)
                AppendIndented("interface " + tsInterface.Name);
            else
                AppendIndented("export interface " + tsInterface.Name);

            if (tsInterface.Parent != null)
                // When extending BaseClassInterfaces, there is no need to state module name, as BaseClassInterfaces are always local to module.
                Output.Append(" extends " + (tsInterface.Parent.Module.IsGlobal || tsInterface.Parent.IsIndirectlyMarkedInterface ? "" : tsInterface.Parent.Module.QualifiedName + ".") + tsInterface.Parent.Name);

            Output.AppendLine(" {");
        }

        private void EndInterface()
        {
            AppendIndentedLine("}");
            AppendIndentedLine("");
        }

        private void AppendIndexer(TypeScriptInterface tsInterface)
        {
            AppendIndendation();
            Output.AppendFormat("    [index: number]: {0};", tsInterface.IndexedType);
            Output.AppendLine();
        }
    }
}

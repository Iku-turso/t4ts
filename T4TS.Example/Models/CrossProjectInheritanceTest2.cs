using System;

namespace T4TS.Example.Models
{
    [TypeScriptInterface(CreateTypeScriptInterfacesAlsoForBaseClasses = true)]
    public class CrossProjectInheritanceTest2 : TypeScriptInterfaceAttribute
    {
        public string SomeString4 { get; set; }
        public Attribute FooAttribute { get; set; }
    }
}
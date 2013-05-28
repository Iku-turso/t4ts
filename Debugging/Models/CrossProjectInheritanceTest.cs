namespace T4TS.Example.Models
{
    [TypeScriptInterface(CreateTypeScriptInterfacesAlsoForBaseClasses = true)]
    public class CrossProjectInheritanceTest : TypeScriptInterfaceAttribute
    {
        public string SomeString4 { get; set; }
        public Foobar Recursive4 { get; set; }
    }
}
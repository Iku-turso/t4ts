namespace T4TS.Example.Models
{
    /// <summary>
    /// A class which lacks the [TypeScriptInterface] -attribute. A "non-export" TypeScript interface will still be however generated, 
    /// because CrossProjectInheritanceTest inherits from this class, and is decorated as
    /// [TypeScriptInterface(CreateTypeScriptInterfacesAlsoForBaseClasses = true)].
    /// </summary>
    public class TestClass : System.Object
    {
        public string Property { get; set; }
    }
}
T4TS
====

T4 template to generate TypeScript interface definitions.

Usage
----
 1. Install the NuGet package (https://nuget.org/packages/T4TS).

 2. Decoreate any C# class that you want to generate an interface for, with the `TypeScriptInterfaceAttribute` (in the T4TS namespace).

 3. Run the T4TS.tt-file (right-click and select *Run custom tool*).

 4. The generated file T4TS.d.ts will now contain the interfaces that can be used in your TypeScript files.

Examples
----
C# classes:

    [TypeScriptInterface]
    public class MyModel
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public ReferencedModel Ref { get; set; }
    }

    [TypeScriptInterface]
    public class ReferencedModel
    {
        public double Fraction { get; set; }
        public int[] Digits { get; set; }
    }

Resulting T4TS.d.ts:

    module T4TS {
        export interface MyModel {
            Number: number;
            Name: string;
            Ref: ReferencedModel;
        }
        export interface ReferencedModel {
            Fraction: number;
            Digits: number[];
        }
    }

This interface can now be used in your TypeScript files:

    /// <reference path="T4TS.d.ts" />
    class Test {
        constructor () {
            // Make an AJAX post and get some data from the server.
            // In the callback, you can specify that the data is of a certain type:
            $.post('./example', {}, (data: T4TS.MyModel) => {
                // Intellisense support for the properties:
                alert(data.Number.toString());
                alert(data.Ref.Digits[0].toString());
            });
        }
    }

Specifications
----
 * Only public properties are considered

 * Right now, `System.DateTime` is considered a `string` in the type translation. The main reason is that the [JSON serialization in .NET MVC](http://stackoverflow.com/questions/726334/asp-net-mvc-jsonresult-date-format) will typically serialize a `DateTime` as `"\/Date(ticks)\/"`

 * The type translation works like this, from C# => TypeScript, for each property:
   * Built-in numeric type (`int`, `double`, `float`, etc.) **=>** `number`
   * `string` **=>** `string`
   * A class marked with `[TypeScriptInterface]` **=>** lookup the generated TypeScript name
   * Otherwise **=>** `any`
   * For `Collection<T>`, `List<T>`, `IList<T>` and `T[]` **=>** lookup type for `T` as above, and return `T[]`.

Customize the generated interfaces
----
The attribute `TypeScriptInterfaceAttribute` is set on C# classes, and has the following properties:
* **Name:** Specifies the name of the interface (default is the class name).
* **Module:** Specifies the module of the interface (default _T4TS_).

The attribute `TypeScriptMemberAttribute` can be set on the properties of a C# class, and has the following properties:
* **Name:** Specifies the name of the member (default is the property name).
* **Optional:** Specifies whether this member should be optional, ie. `member?: type` instead of `member: type`.
* **Type:** Specifies the type of the member (default is to do type translation of the property).

Default settings
----
There are a couple of default settings that can be specified in the `T4TS.tt.settings.t4` file.

* **DefaultModule:** The default module name of an interfaces (if not specified by `TypeScriptInterfaceAttribute`).
* **DefaultOptional:** The default value for the Optional flag for `TypeScriptMemberAttribute`.

Wiki
----
See the [Wiki](https://github.com/cskeppstedt/t4ts/wiki) for more information.

Known problems
----
**TextTransformation fails:**

If the TextTransformation fails (for any reason), the `T4TS.cs` will not be generated correctly. This file contains `TypeScriptInterfaceAttribute` and `TypeScriptMemberAttribute`, so if they go amiss, there will be compile errors. This in turn will make the .tt file unable to be transformed. The workaround so far, is to open `T4TS.cs` and paste the code [from the source](https://github.com/cskeppstedt/t4ts/blob/master/build/T4TS.cs). I'm looking for a way to avoid this.

License
----
[Apache License, Version 2.0](http://www.apache.org/licenses/LICENSE-2.0)

Thanks to [T4MVC](http://t4mvc.codeplex.com/), which has been the inspiration. I also learned how to use the .tt-stuff from reading the source, and using part of the source code from T4MVC.
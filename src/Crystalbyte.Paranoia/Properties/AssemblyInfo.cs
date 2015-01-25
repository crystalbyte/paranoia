#region Using directives

using System.Runtime.CompilerServices;

#region Using directives

using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Windows;

#endregion

#endregion

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("Paranoia")]
[assembly: AssemblyDescription("Paranoia is a secure e-mail application.")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("Crystalbyte")]
[assembly: AssemblyProduct("Crystalbyte Paranoia")]
[assembly: AssemblyCopyright("Copyright ©  2013")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.

[assembly: ComVisible(false)]

//In order to begin building localizable applications, set 
//<UICulture>CultureYouAreCodingWith</UICulture> in your .csproj file
//inside a <PropertyGroup>.  For example, if you are using US english
//in your source files, set the <UICulture> to en-US.  Then uncomment
//the NeutralResourceLanguage attribute below.  Update the "en-US" in
//the line below to match the UICulture setting in the project file.

[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.MainAssembly)]
[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
    //(used if a resource is not found in the page, 
    // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
    //(used if a resource is not found in the page, 
    // app, or any theme specific resource dictionaries)
    )]


// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
// Public Key: 00240000048000009400000006020000002400005253413100040000010001003bf228fa0ea21f753fe1b4aba4135796086fd6bfb7d05aad8c9c6d5725eeccafca100ece803c2a80547db48ba286e9d7886f1f73920d9ab011e2c9aff1376b4b1bbd81d55fa601e8c70bf130756c120bec9a47e8bfa6e5f2ed3006f671fc15399cf136d1c6d9a0aa5a0feb360e9f21d4cbe2727dca51c3186ccda6ca398f01d6
[assembly: InternalsVisibleTo("Crystalbyte.Paranoia.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001003bf228fa0ea21f753fe1b4aba4135796086fd6bfb7d05aad8c9c6d5725eeccafca100ece803c2a80547db48ba286e9d7886f1f73920d9ab011e2c9aff1376b4b1bbd81d55fa601e8c70bf130756c120bec9a47e8bfa6e5f2ed3006f671fc15399cf136d1c6d9a0aa5a0feb360e9f21d4cbe2727dca51c3186ccda6ca398f01d6")]
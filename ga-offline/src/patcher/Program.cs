using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

// patcher <Assembly-CSharp.dll in> <GAOffline.dll> <Assembly-CSharp.dll out>
// Routes WWWManager.Begin through GAOffline.LocalServer:
//   Begin(url, form): "var w = LocalServer.BeginForm(this, url, form); if (w != null) return w;" then the original body
//   Begin(url):       "url = LocalServer.MapUrl(url);" then the original body
// and calls GAOffline.Trace.Install() at the start of AbstractManagers.Awake.
class P {
  static int Main(string[] a) {
    var res = new DefaultAssemblyResolver();
    res.AddSearchDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(a[0])));
    res.AddSearchDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(a[1])));
    var asm = AssemblyDefinition.ReadAssembly(a[0], new ReaderParameters { AssemblyResolver = res });
    var off = AssemblyDefinition.ReadAssembly(a[1], new ReaderParameters { AssemblyResolver = res });
    var mod = asm.MainModule;
    var server = off.MainModule.GetType("GAOffline.LocalServer");
    var beginForm = mod.ImportReference(server.Methods.Single(m => m.Name == "BeginForm"));
    var mapUrl = mod.ImportReference(server.Methods.Single(m => m.Name == "MapUrl"));

    var www = mod.GetType("WWWManager");
    var withForm = www.Methods.Single(m => m.Name == "Begin" && m.Parameters.Count == 2 && m.Parameters[1].ParameterType.Name == "WWWForm");
    var plain = www.Methods.Single(m => m.Name == "Begin" && m.Parameters.Count == 1);

    var il = withForm.Body.GetILProcessor();
    var first = withForm.Body.Instructions[0];
    var pop = il.Create(OpCodes.Pop);
    foreach (var i in new[] {
        il.Create(OpCodes.Ldarg_0), il.Create(OpCodes.Ldarg_1), il.Create(OpCodes.Ldarg_2), il.Create(OpCodes.Call, beginForm),
        il.Create(OpCodes.Dup), il.Create(OpCodes.Brfalse, pop), il.Create(OpCodes.Ret), pop })
      il.InsertBefore(first, i);

    il = plain.Body.GetILProcessor();
    first = plain.Body.Instructions[0];
    foreach (var i in new[] { il.Create(OpCodes.Ldarg_1), il.Create(OpCodes.Call, mapUrl), il.Create(OpCodes.Starg, plain.Parameters[0]) })
      il.InsertBefore(first, i);

    // boot diagnostics: "Trace.Install();" first thing in AbstractManagers.Awake
    var install = mod.ImportReference(off.MainModule.GetType("GAOffline.Trace").Methods.Single(m => m.Name == "Install"));
    var awake = mod.GetType("SynCore.AbstractManagers").Methods.Single(m => m.Name == "Awake");
    il = awake.Body.GetILProcessor();
    il.InsertBefore(awake.Body.Instructions[0], il.Create(OpCodes.Call, install));

    asm.Write(a[2]);
    Console.WriteLine("patched WWWManager.Begin(url, form), Begin(url) and AbstractManagers.Awake");
    return 0;
  }
}

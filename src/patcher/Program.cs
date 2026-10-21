using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

class P {
  static int Main(string[] a) {
    string mode = a[0];
    if (mode == "info") {
      var m = ModuleDefinition.ReadModule(a[1]);
      Console.WriteLine("runtime " + m.RuntimeVersion + " arch " + m.Architecture);
      foreach (var r in m.AssemblyReferences) Console.WriteLine("ref " + r.FullName);
      return 0;
    }
    if (mode == "patch") {
      // a[1]=firstpass in, a[2]=SparxOffline.dll, a[3]=out
      var res = new DefaultAssemblyResolver();
      res.AddSearchDirectory(System.IO.Path.GetDirectoryName(a[1]));
      res.AddSearchDirectory(System.IO.Path.GetDirectoryName(a[2]));
      var fp = AssemblyDefinition.ReadAssembly(a[1], new ReaderParameters { AssemblyResolver = res });
      var off = AssemblyDefinition.ReadAssembly(a[2], new ReaderParameters { AssemblyResolver = res });
      var mod = fp.MainModule;
      var offRef = new AssemblyNameReference(off.Name.Name, off.Name.Version);
      mod.AssemblyReferences.Add(offRef);
      Redirect(mod, offRef, "EB.Sparx.EndPointFactory", "LocalEndPoint", "EB.Sparx.EndPointOptions");
      Redirect(mod, offRef, "EB.IAP.Internal.ProviderFactory", "LocalProvider", "EB.IAP.Config");
      if (a.Length > 4 && a[4] == "obb-in-apk") ObbInApk(mod);
      fp.Write(a[3]);
      return 0;
    }
    return 1;
  }

  // The OBB packs ship inside the APK (listed in assets/packs.txt), so
  // ObbManager.Check() just reports them mounted: "IsMounted = true; return null;".
  static void ObbInApk(ModuleDefinition mod) {
    var type = mod.GetType("EB.Sparx.ObbManager");
    var check = type.Methods.Single(x => x.Name == "Check");
    var setter = type.Properties.Single(x => x.Name == "IsMounted").SetMethod;
    var body = check.Body;
    body.Instructions.Clear();
    body.ExceptionHandlers.Clear();
    body.Variables.Clear();
    body.InitLocals = false;
    var il = body.GetILProcessor();
    il.Emit(OpCodes.Ldarg_0);
    il.Emit(OpCodes.Ldc_I4_1);
    il.Emit(OpCodes.Call, setter);
    il.Emit(OpCodes.Ldnull);
    il.Emit(OpCodes.Ret);
  }

  // Replaces Factory.Create(args) with "return new EB.Sparx.<target>(args);".
  static void Redirect(ModuleDefinition mod, AssemblyNameReference offRef, string factoryName, string target, string lastParam) {
    var create = mod.GetType(factoryName).Methods.Single(x => x.Name == "Create");
    var type = new TypeReference("EB.Sparx", target, mod, offRef);
    var ctor = new MethodReference(".ctor", mod.TypeSystem.Void, type) { HasThis = true };
    foreach (var p in create.Parameters) ctor.Parameters.Add(new ParameterDefinition(p.ParameterType));
    var body = create.Body;
    body.Instructions.Clear();
    body.ExceptionHandlers.Clear();
    body.Variables.Clear();
    body.InitLocals = false;
    var il = body.GetILProcessor();
    for (int i = 0; i < create.Parameters.Count; i++) il.Emit(OpCodes.Ldarg, create.Parameters[i]);
    il.Emit(OpCodes.Newobj, ctor);
    il.Emit(OpCodes.Ret);
  }
}

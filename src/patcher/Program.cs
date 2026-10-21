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
      if (a.Contains("obb-in-apk")) ObbInApk(mod);
      if (a.Contains("no-dead-sdks")) NoDeadSdks(mod);
      if (a.Contains("diag-log")) DiagLog(mod);
      if (a.Contains("diag-trace")) DiagTrace(mod);
      fp.Write(a[3]);
      return 0;
    }
    if (mode == "empty-enum") {
      // Test builds only: empty-enum <in.dll> <Type> <Method> <out.dll> makes an IEnumerator method yield nothing.
      var res2 = new DefaultAssemblyResolver();
      res2.AddSearchDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(a[1])));
      var asm = AssemblyDefinition.ReadAssembly(a[1], new ReaderParameters { AssemblyResolver = res2 });
      var mod = asm.MainModule;
      var m = mod.GetType(a[2]).Methods.Single(x => x.Name == a[3]);
      var list = new TypeReference("System.Collections", "ArrayList", mod, mod.TypeSystem.CoreLibrary);
      var body = m.Body;
      body.Instructions.Clear(); body.ExceptionHandlers.Clear(); body.Variables.Clear(); body.InitLocals = false;
      var il = body.GetILProcessor();
      il.Emit(OpCodes.Newobj, new MethodReference(".ctor", mod.TypeSystem.Void, list) { HasThis = true });
      il.Emit(OpCodes.Callvirt, new MethodReference("GetEnumerator", m.ReturnType, list) { HasThis = true });
      il.Emit(OpCodes.Ret);
      asm.Write(a[4]);
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

  // The APK-signature check (native libapkprotect), the Chartboost ad SDK and the MobileAppTracker
  // install tracker only talk to dead servers. On current Android the game crashed right after they
  // started, so they are switched off: Protect reports a fixed signature (the local server ignores it),
  // Chartboost keeps its Instance but never starts a session, MobileAppTracker never initialises.
  static void NoDeadSdks(ModuleDefinition mod) {
    var protect = mod.GetType("EB.Sparx.Protect");
    ConstantCallback(mod, protect.Methods.Single(x => x.Name == "CalculateSha1"), 0);
    ConstantCallback(mod, protect.Methods.Single(x => x.Name == "CalculateHmac"), 1);

    var cb = mod.GetType("ChartboostPlugin");
    var start = cb.Methods.Single(x => x.Name == "Start");
    var il = start.Body.GetILProcessor();
    var first = start.Body.Instructions[0];
    il.InsertBefore(first, il.Create(OpCodes.Ldarg_0));
    il.InsertBefore(first, il.Create(OpCodes.Ldnull));
    il.InsertBefore(first, il.Create(OpCodes.Stfld, cb.Fields.Single(x => x.Name == "appId")));
    foreach (var m in cb.Methods.Where(x => x.Name == "CacheInterstitial" || x.Name == "ShowInterstitial" || x.Name == "ShowMoreApps" || x.Name == "CacheMoreApps"))
      Return(m);
    Return(mod.GetType("MobileAppTrackerPlugin").Methods.Single(x => x.Name == "Start"));
  }

  // Test builds only: EB.Debug.Log/LogWarning/LogError also go to logcat (normally kept in an in-memory ring buffer).
  static void DiagLog(ModuleDefinition mod) {
    var dbg = mod.GetType("EB.Debug");
    var format = dbg.Methods.Single(x => x.Name == "Format");
    var ue = mod.AssemblyReferences.Single(x => x.Name == "UnityEngine");
    var unityDebug = new TypeReference("UnityEngine", "Debug", mod, ue);
    foreach (var name in new[] { "Log", "LogWarning", "LogError" }) {
      var m = dbg.Methods.Single(x => x.Name == name);
      var log = new MethodReference(name, mod.TypeSystem.Void, unityDebug);
      log.Parameters.Add(new ParameterDefinition(mod.TypeSystem.Object));
      var il = m.Body.GetILProcessor();
      var first = m.Body.Instructions[0];
      il.InsertBefore(first, il.Create(OpCodes.Ldarg_0));
      il.InsertBefore(first, il.Create(OpCodes.Ldarg_1));
      il.InsertBefore(first, il.Create(OpCodes.Call, format));
      il.InsertBefore(first, il.Create(OpCodes.Call, log));
    }
  }

  // Test builds only: logs entry into every method of the content loader (EB.Loader, EB.Assets and their coroutines).
  static void DiagTrace(ModuleDefinition mod) {
    var ue = mod.AssemblyReferences.Single(x => x.Name == "UnityEngine");
    var log = new MethodReference("Log", mod.TypeSystem.Void, new TypeReference("UnityEngine", "Debug", mod, ue));
    log.Parameters.Add(new ParameterDefinition(mod.TypeSystem.Object));
    var concat = new MethodReference("Concat", mod.TypeSystem.String, new TypeReference("System", "String", mod, mod.TypeSystem.CoreLibrary));
    concat.Parameters.Add(new ParameterDefinition(mod.TypeSystem.String));
    concat.Parameters.Add(new ParameterDefinition(mod.TypeSystem.String));
    var types = new[] { "EB.Loader", "EB.Assets" }.Select(n => mod.GetType(n)).ToList();
    foreach (var t in types.ToList()) types.AddRange(t.NestedTypes);
    foreach (var t in types)
      foreach (var m in t.Methods.Where(x => x.HasBody && x.Body.Instructions.Count > 0)) {
        var il = m.Body.GetILProcessor();
        var first = m.Body.Instructions[0];
        var strArg = m.Parameters.FirstOrDefault(x => x.ParameterType.FullName == "System.String");
        il.InsertBefore(first, il.Create(OpCodes.Ldstr, "TRACE " + t.Name + "." + m.Name + " "));
        if (strArg != null) il.InsertBefore(first, il.Create(OpCodes.Ldarg, strArg)); else il.InsertBefore(first, il.Create(OpCodes.Ldstr, ""));
        il.InsertBefore(first, il.Create(OpCodes.Call, concat));
        il.InsertBefore(first, il.Create(OpCodes.Call, log));
      }
  }

  static void Return(MethodDefinition m) {
    var body = m.Body;
    body.Instructions.Clear(); body.ExceptionHandlers.Clear(); body.Variables.Clear(); body.InitLocals = false;
    body.GetILProcessor().Emit(OpCodes.Ret);
  }

  // IEnumerator Method(..., Action<string> cb) => { cb("offline"); return new ArrayList().GetEnumerator(); }
  static void ConstantCallback(ModuleDefinition mod, MethodDefinition m, int cbArg) {
    var cbType = (GenericInstanceType)m.Parameters[cbArg].ParameterType;
    var invoke = new MethodReference("Invoke", mod.TypeSystem.Void, cbType) { HasThis = true };
    invoke.Parameters.Add(new ParameterDefinition(cbType.ElementType.GenericParameters[0]));
    var list = new TypeReference("System.Collections", "ArrayList", mod, mod.TypeSystem.CoreLibrary);
    var listCtor = new MethodReference(".ctor", mod.TypeSystem.Void, list) { HasThis = true };
    var getEnum = new MethodReference("GetEnumerator", m.ReturnType, list) { HasThis = true };
    var body = m.Body;
    body.Instructions.Clear(); body.ExceptionHandlers.Clear(); body.Variables.Clear(); body.InitLocals = false;
    var il = body.GetILProcessor();
    il.Emit(OpCodes.Ldarg, m.Parameters[cbArg]);
    il.Emit(OpCodes.Ldstr, "offline");
    il.Emit(OpCodes.Callvirt, invoke);
    il.Emit(OpCodes.Newobj, listCtor);
    il.Emit(OpCodes.Callvirt, getEnum);
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

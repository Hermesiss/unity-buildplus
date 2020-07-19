using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Debug = UnityEngine.Debug;

namespace Trismegistus.BuildPlus {
	[AttributeUsage(AttributeTargets.Method)]
	public class BuildPathAttribute : Attribute {
		public static IEnumerable<PathParameter> CallAllMethods() {
			var pathParams = new List<PathParameter>();
			var methods = AppDomain.CurrentDomain.GetAssemblies() // Returns all currently loaded assemblies
				.SelectMany(x => x.GetTypes()) // returns all types defined in this assemblies
				.Where(x => x.IsClass &&
				            x.GetCustomAttributes(typeof(BuildPathProviderAttribute), false).FirstOrDefault() !=
				            null) // only yields classes
				.SelectMany(x => x.GetMethods()).Where(x => x.IsStatic) // returns all methods defined in those classes
				.Where(x => x.GetCustomAttributes(typeof(BuildPathAttribute), false).FirstOrDefault() != null);

			foreach (var method in methods) // iterate through all found methods
			{
				var resp = method.Invoke(null, null); // invoke the method
				if (resp is PathParameter pathParameter) {
					pathParams.Add(pathParameter);
				}
				else {
					Debug.LogError($"Wrong type! myst be {nameof(PathParameter)} but used {resp.GetType()}");
				}
			}

			return pathParams;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class BuildPathProviderAttribute : Attribute { }
}
using System.IO;
using UnityEditor;
using UnityEngine;
	
namespace Trismegistus.BuildPlus {
	public static class StandardBuildParameters {
		[BuildPath]
		public static PathParameter ProjectName() {
			return new PathParameter("p_name", "Project", Application.productName,
				"ProductName");
		}

		[BuildPath]
		public static PathParameter Version() {
			var v = BuildPlusEditor.build.CurrentVersion;
			var revision = !string.IsNullOrEmpty(v.revision) ? $"_{v.revision}" : "";
			return new PathParameter("ver", "Project", $"{v.major}.{v.minor}.{v.build}{revision}",
				"Last version");
		}

		[BuildPath]
		public static PathParameter ProjectPath() {
			return new PathParameter("p_path", "Project", new DirectoryInfo(Application.dataPath).Parent?.FullName,
				"Path to project folder");
		}

		[BuildPath]
		public static PathParameter Platform() {
			return new PathParameter("platform", "Project", EditorUserBuildSettings.activeBuildTarget.ToString(),
				"Current build target");
		}
	}
}
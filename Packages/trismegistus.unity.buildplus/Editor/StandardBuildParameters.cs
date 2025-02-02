using System.IO;
using UnityEditor;
using UnityEngine;

namespace Trismegistus.BuildPlus {
	[BuildPathProvider]
	public static class StandardBuildParameters {
		[BuildPath]
		public static PathParameter ProjectName() {
			return new PathParameter("p_name", "Project", Application.productName.ReplaceUnsupportedSymbols(),
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
		
		[BuildPath]
		public static PathParameter BuildType() {
			return new PathParameter("build_type", "Project", EditorUserBuildSettings.development ? "Development" : "Release",
				"Build type");
		}
		
		[BuildPath]
		public static PathParameter BuildTarget() {
			return new PathParameter("build_target", "Project", EditorUserBuildSettings.activeBuildTarget.ToString(),
				"Build target");
		}
	}
}
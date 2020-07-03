// Build+ Unity Extension
// Copyright (c) 2012 Luminary Productions, Inc.
// Please direct any bugs/comments/suggestions to http://luminaryproductions.net

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using BuildPlus;
using Version = BuildPlus.Version;
using Note = BuildPlus.Version.Note;

public sealed class BuildPlusEditor : EditorWindow {
	static readonly string kXmlPath = "Assets/_BuildPlus.xml";
	static readonly string kAssetPath = "Assets/_BuildPlus.asset";

	Build build;
	Dictionary<Version, bool> expanded = new Dictionary<Version, bool>();
	Vector2 scrollPos;

	[MenuItem("File/Build+... %&b", false, 1000)]
	static void ShowWindow() {
		GetWindow<BuildPlusEditor>(true, "Build+", true);
	}

	bool IsDirty() {
		// This is the quick and easy test
		if (!File.Exists(kXmlPath)) return true;

		var onDiskXml = File.ReadAllText(kXmlPath);

		if (build.ToXML() == onDiskXml)
			return false;

		// Unity doesn't serialize DateTime properly, so if the editor code is reloaded and this window is up,
		// then those values are reset, so let's check for that special case
		if (build.CurrentVersion.date == DateTime.MinValue)
			return false;

		Build original = Build.FromXML(onDiskXml);

		if (original.versions.Count != build.versions.Count)
			return true;

		if (original.CurrentVersion.ToString() != build.CurrentVersion.ToString())
			return true;

		// Exhaustive walkthrough
		for (int i = 0; i < build.versions.Count; i++) {
			if (original.versions[i].major != build.versions[i].major)
				return true;

			if (original.versions[i].minor != build.versions[i].minor)
				return true;

			if (original.versions[i].build != build.versions[i].build)
				return true;

			if (original.versions[i].revision != build.versions[i].revision)
				return true;

			if (original.versions[i].date != build.versions[i].date)
				return true;

			if (original.versions[i].notes.Count != build.versions[i].notes.Count)
				return true;

			for (int j = 0; j < build.versions[i].notes.Count; j++) {
				if (original.versions[i].notes[j].category != build.versions[i].notes[j].category)
					return true;

				if (original.versions[i].notes[j].description != build.versions[i].notes[j].description)
					return true;
			}
		}

		return true;
	}

	void OnEnable() {
		if (build == null || !IsDirty()) {
			if (File.Exists(kXmlPath))
				build = Build.FromXML(File.ReadAllText(kXmlPath));
			else {
				if (LoadBuildAsset(out var b)) {
					build = b;
				}
				else {
					build = new Build();

					var v = new Version {
						build = 1,
						date = DateTime.Now
					};
					build.versions.Add(v);
				}
			}
		}
		
		CategoryColors = new Dictionary<Note.Category, Color> {
			{Note.Category.Hidden, Color.gray},
			{Note.Category.Features, new Color(0.47f, 1f, 0.45f)},
			{Note.Category.Improvements, new Color(0.53f, 0.42f, 0.89f)},
			{Note.Category.Fixes, new Color(0.4f, 0.87f, 1f)},
			{Note.Category.Changes, new Color(1f, 0.59f, 0.29f)},
			{Note.Category.KnownIssues, new Color(1f, 0.34f, 0.39f)},
			{Note.Category.General, GUI.contentColor}
		};

		expanded.Clear();
	}

	private bool LoadBuildAsset(out Build b) {
		if (File.Exists(kAssetPath)) {
			var bso = AssetDatabase.LoadAssetAtPath(kAssetPath, typeof(BuildPlusSO)) as BuildPlusSO;
			if (bso != null) {
				b = bso.build;
				foreach (var bVersion in b.versions) {
					bVersion.FillDate();
				}

				return true;
			}
		}

		b = null;
		return false;
	}

	void Save() {
		File.WriteAllText(kXmlPath, build.ToXML());
		BuildPlusSO bso = null;
		if (File.Exists(kAssetPath))
			bso = AssetDatabase.LoadAssetAtPath(kAssetPath, typeof(BuildPlusSO)) as BuildPlusSO;
		else {
			bso = ScriptableObject.CreateInstance<BuildPlusSO>();
			AssetDatabase.CreateAsset(bso, kAssetPath);
		}

		bso.build = Build.FromXML(build.ToXML()); // This makes a copy, which is needed when writing to disk
		bso.build.releaseNotes = GetReleaseNotes();
		EditorUtility.SetDirty(bso);
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		var v = bso.build.versions.First();
		var bundleVersion = $"{v.major}.{v.minor}.{v.build}";
		if (build.editorSettings.saveToPlayerSettings) {
			
			PlayerSettings.bundleVersion = bundleVersion;
		}

		if (build.editorSettings.saveToPackageJson) {
			if (File.Exists(build.editorSettings.packageJsonPath)) {
				var manifest = File.ReadAllText(build.editorSettings.packageJsonPath);
				var manifest2 = Regex.Replace(manifest, "\"version\": \"[0-9]*.[0-9]*.[0-9]*\"", $"\"version\": \"{bundleVersion}\"");
				File.WriteAllText(build.editorSettings.packageJsonPath, manifest2);
			}
		}
	}

	string Indent(int count) {
		return string.Empty.PadLeft(count);
	}

	string GetReleaseNotes() {
		StringBuilder sb = new StringBuilder();
		sb.Append("RELEASE NOTES");
		sb.AppendLine();
		sb.AppendLine();
		foreach (Version v in build.versions) {
			sb.AppendFormat("v{0}", v.ToString());
			sb.AppendLine();
			Note.Category current = Note.Category.Hidden;
			foreach (Note n in v.notes) {
				if (n.category == Note.Category.Hidden)
					continue;

				if (current != n.category) {
					current = n.category;
					sb.AppendFormat("{0}{1}", Indent(2), ObjectNames.NicifyVariableName(current.ToString()).ToUpper());
					sb.AppendLine();
				}

				sb.AppendFormat("{0}* {1}", Indent(4), n.description);
				sb.AppendLine();
			}

			sb.AppendLine();
		}

		return sb.ToString();
	}

	void ExportReleaseNotes() {
		string path = EditorUtility.SaveFilePanel("Export Release Notes", "Assets", "ReleaseNotes", "txt");
		if (!string.IsNullOrEmpty(path)) {
			File.WriteAllText(path, GetReleaseNotes());
		}
	}

	void DoBuild() {
		// Update latest version's date/time to reflect build
		build.CurrentVersion.date = DateTime.Now;
		Save();

		//string buildLocation = EditorUserBuildSettings.GetBuildLocation(EditorUserBuildSettings.activeBuildTarget);

		var versionPostfix = $"{build.CurrentVersion.major}.{build.CurrentVersion.minor}.{build.CurrentVersion.build}";

		var buildLocation = Path.Combine("Builds",
			Application.productName + "_" + versionPostfix + "_" +
			EditorUserBuildSettings.activeBuildTarget.ToString("G"),
			Application.productName + ".exe");

		if (!string.IsNullOrEmpty(buildLocation)) {
//			string[] dlls = Directory.GetFiles("Assets/Plugins", "*.dll");
//			foreach (string dll in dlls)
//				File.Copy(dll, Path.GetDirectoryName(buildLocation) + "/" + Path.GetFileName(dll), true);

			BuildPipeline.BuildPlayer(
				EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
				buildLocation,
				EditorUserBuildSettings.activeBuildTarget,
				BuildOptions.ShowBuiltPlayer);

			var folder = new FileInfo(buildLocation).DirectoryName;

			File.WriteAllText(Path.Combine(folder, "ReleaseNotes.txt"), GetReleaseNotes());
		}
	}

	void OnGUI() {
		float buttonWidth = 100f;
		float padding = 10f;

		GUILayout.BeginArea(new Rect(padding, padding, position.width - padding * 2f, position.height - padding * 2f));

		EditorGUILayout.BeginHorizontal();
		GUILayout.Label("Version");
		if (GUILayout.Button("+", EditorStyles.miniButton)) {
			var v = new Version {
				major = build.CurrentVersion.major,
				minor = build.CurrentVersion.minor,
				build = build.CurrentVersion.build + 1,
				date = DateTime.Now
			};

			var latest = build.versions.First()?.notes.Where(x => x.category == Note.Category.KnownIssues).Select(x => (Note)x.Clone());

			if (latest != null) v.notes = latest.ToList();

			build.versions.Add(v);
			build.versions = build.versions.OrderByDescending(ver => ver.major)
				.ThenByDescending(ver => ver.minor)
				.ThenByDescending(ver => ver.build)
				.ThenByDescending(ver => ver.date).ToList();
		}

		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
		foreach (Version v in build.versions) {
			if (!expanded.ContainsKey(v))
				expanded[v] = v == build.CurrentVersion;

			EditorGUILayout.BeginHorizontal();
			expanded[v] = EditorGUILayout.Foldout(expanded[v], v.ToString());
			if (expanded[v] && build.versions.Count > 0 && GUILayout.Button("Delete", EditorStyles.miniButton)) {
				if (EditorUtility.DisplayDialog("Remove Version?", "Are you sure you want to delete this version?",
					"Yes", "No")) {
					build.versions.Remove(v);
					GUIUtility.ExitGUI();
				}
			}

			EditorGUILayout.EndHorizontal();

			if (expanded[v]) {
				EditorGUI.indentLevel++;
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField("Version");
					v.major = EditorGUILayout.IntField(v.major);
					GUILayout.Label(".");
					v.minor = EditorGUILayout.IntField(v.minor);
					GUILayout.Label(".");
					v.build = EditorGUILayout.IntField(v.build);
					v.revision = EditorGUILayout.TextField(v.revision);
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Date / Time");
				int dateValue;
				dateValue = EditorGUILayout.IntField(v.date.Year);

				if (GUI.changed) {
					v.date = v.date.AddYears(dateValue - v.date.Year);
					GUI.changed = false;
				}

				dateValue = EditorGUILayout.IntField(v.date.Month);
				if (GUI.changed) {
					v.date = v.date.AddMonths(dateValue - v.date.Month);
					GUI.changed = false;
				}

				dateValue = EditorGUILayout.IntField(v.date.Day);
				if (GUI.changed) {
					v.date = v.date.AddDays(dateValue - v.date.Day);
					GUI.changed = false;
				}

				dateValue = EditorGUILayout.IntField(v.date.Hour);
				if (GUI.changed) {
					v.date = v.date.AddHours(dateValue - v.date.Hour);
					GUI.changed = false;
				}

				dateValue = EditorGUILayout.IntField(v.date.Minute);
				if (GUI.changed) {
					v.date = v.date.AddMinutes(dateValue - v.date.Minute);
					GUI.changed = false;
				}

				if (GUILayout.Button("Now", EditorStyles.miniButton))
					v.date = DateTime.Now;
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Notes", GUILayout.Width(100));
				
				EditorGUILayout.LabelField("Add new:", GUILayout.Width(75));
				
				foreach (var s in Enum.GetNames(typeof(Note.Category))) {
					var category = (Note.Category) Enum.Parse(typeof(Note.Category), s, true);
					var color = GetColorByCategory(category) ;

					var oldColor = GUI.contentColor;
					GUI.color = color;
					if (GUILayout.Button(s, EditorStyles.miniButton)) {
						var note = new Note() {category = category};
						v.notes.Add(note);	
						v.notes = v.notes.OrderBy(n => n.category).ToList();
					}

					GUI.color = oldColor;
				}
				
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				EditorGUI.indentLevel++;
				foreach (Note n in v.notes) {
					EditorGUILayout.BeginHorizontal();
					GUI.changed = false;
					var color = GetColorByCategory(n.category);

					var oldColor = GUI.contentColor;
					GUI.color = color;

					n.category = (Note.Category) EditorGUILayout.EnumPopup(n.category, GUILayout.Width(100f));
					GUI.color = oldColor;
					if (GUI.changed) {
						v.notes = v.notes.OrderBy(note => note.category).ToList();
						GUIUtility.keyboardControl = -1;
						GUI.changed = false;
						GUIUtility.ExitGUI();
					}

					
					n.description = EditorGUILayout.TextArea(n.description, GUILayout.MaxWidth(400f));
					if (GUILayout.Button("-", EditorStyles.miniButton, GUILayout.Width(20f))) {
						v.notes.Remove(n);
						GUIUtility.ExitGUI();
					}
					
					EditorGUILayout.EndHorizontal();
				}

				EditorGUI.indentLevel--;

				EditorGUI.indentLevel--;
			}
		}

		EditorGUILayout.EndScrollView();

		GUILayout.FlexibleSpace();

		//Settings

		build.editorSettings.saveToPlayerSettings =
			GUILayout.Toggle(build.editorSettings.saveToPlayerSettings, "Save to PlayerSettings");

		// save to package.json
		EditorGUILayout.BeginHorizontal();
		{
			build.editorSettings.saveToPackageJson =
				GUILayout.Toggle(build.editorSettings.saveToPackageJson, "Save to package.json");

			var guiEnabledOld = GUI.enabled;
			GUI.enabled = build.editorSettings.saveToPackageJson;

			GUIStyle myStyle = new GUIStyle(GUI.skin.textField);
			myStyle.alignment = TextAnchor.MiddleRight;
			build.editorSettings.packageJsonPath =
				GUILayout.TextField(build.editorSettings.packageJsonPath, myStyle, GUILayout.MaxWidth(300));
			if (GUILayout.Button("Browse", EditorStyles.miniButton, GUILayout.Width(buttonWidth))) {
				var projpath = new DirectoryInfo(Application.dataPath).FullName;
				var path = new DirectoryInfo(EditorUtility.OpenFilePanel("Choose package.json",
					build.editorSettings.packageJsonPath, "json")).FullName;

				string GetRelativePath(string pathFrom, string pathTo) {
					Uri uri = new Uri(pathFrom);
					string relativePath = Uri.UnescapeDataString(uri.MakeRelativeUri(new Uri(pathTo)).ToString());
					relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
					if (!relativePath.Contains(Path.DirectorySeparatorChar.ToString()))
						relativePath = "." + Path.DirectorySeparatorChar + relativePath;
					return relativePath;
				}

				build.editorSettings.packageJsonPath = GetRelativePath(projpath, path);
			}

			GUI.enabled = guiEnabledOld;
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		{
			if (GUILayout.Button("Export ReleaseNotes.txt", GUILayout.Width(buttonWidth * 2f))) {
				ExportReleaseNotes();
			}

			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Save", GUILayout.Width(buttonWidth))) {
				Save();
			}

			if (GUILayout.Button("Build", GUILayout.Width(buttonWidth))) {
				DoBuild();
				GUIUtility.ExitGUI();
			}
		}
		EditorGUILayout.EndHorizontal();
		GUILayout.EndArea();
	}

	private static Dictionary<Note.Category, Color> CategoryColors;

	private static Color GetColorByCategory(Note.Category category) => CategoryColors[category];
}
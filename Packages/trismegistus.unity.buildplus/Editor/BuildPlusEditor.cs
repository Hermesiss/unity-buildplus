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
using Trismegistus.SmartFormat;
using Note = Trismegistus.BuildPlus.Version.Note;

namespace Trismegistus.BuildPlus {
	public sealed class BuildPlusEditor : EditorWindow {
		static readonly string kXmlPath = "Assets/_BuildPlus.xml";
		static readonly string kAssetPath = "Assets/_BuildPlus.asset";

		internal static Build build;
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

			_categoryColors = new Dictionary<Note.Category, Color> {
				{Note.Category.Hidden, Color.gray},
				{Note.Category.Features, new Color(0.47f, 1f, 0.45f)},
				{Note.Category.Improvements, new Color(0.53f, 0.42f, 0.89f)},
				{Note.Category.Fixes, new Color(0.4f, 0.87f, 1f)},
				{Note.Category.Changes, new Color(1f, 0.59f, 0.29f)},
				{Note.Category.KnownIssues, new Color(1f, 0.34f, 0.39f)},
				{Note.Category.General, GUI.contentColor},
				{Note.Category.Removed, new Color(1f, 0.08f, 0.22f)},
				{Note.Category.Deprecated, new Color(1f, 0.57f, 0.32f)}
			};

			expanded.Clear();
			UpdateDict();
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
			UpdateDict();
			File.WriteAllText(kXmlPath, build.ToXML());
			BuildPlusSO bso = null;
			if (File.Exists(kAssetPath))
				bso = AssetDatabase.LoadAssetAtPath(kAssetPath, typeof(BuildPlusSO)) as BuildPlusSO;
			else {
				bso = CreateInstance<BuildPlusSO>();
				AssetDatabase.CreateAsset(bso, kAssetPath);
			}

			bso.build = Build.FromXML(build.ToXML()); // This makes a copy, which is needed when writing to disk
			bso.build.releaseNotes = GetReleaseNotes();
			EditorUtility.SetDirty(bso);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			var v = bso.build.versions.First(x => !x.unreleased);
			var bundleVersion = $"{v.major}.{v.minor}.{v.build}";
			if (build.editorSettings.saveToPlayerSettings) {
				PlayerSettings.bundleVersion = bundleVersion;
			}

			if (build.editorSettings.saveToPackageJson && File.Exists(build.editorSettings.packageJsonPath)) {
				var manifest = File.ReadAllText(build.editorSettings.packageJsonPath);
				var manifest2 = Regex.Replace(manifest, "\"version\": \"[0-9]*.[0-9]*.[0-9]*\"",
					$"\"version\": \"{bundleVersion}\"");
				File.WriteAllText(build.editorSettings.packageJsonPath, manifest2);
			}

			if (build.editorSettings.saveToChangelogMd) {
				var b = new ChangelogBuilder(build);
				b.Build();
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
						sb.AppendFormat("{0}{1}", Indent(2),
							ObjectNames.NicifyVariableName(current.ToString()).ToUpper());
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

		private void DoBuild() {
			// Update latest version's date/time to reflect build
			build.CurrentVersion.date = DateTime.Now;
			Save();

			//string buildLocation = EditorUserBuildSettings.GetBuildLocation(EditorUserBuildSettings.activeBuildTarget);

			var versionPostfix =
				$"{build.CurrentVersion.major}.{build.CurrentVersion.minor}.{build.CurrentVersion.build}";

			var buildLocation = GetBuildPath();
			Debug.Log(buildLocation);
			/*Path.Combine("Builds",
			Application.productName + "_" + versionPostfix + "_" +
			EditorUserBuildSettings.activeBuildTarget.ToString("G"),
			Application.productName + ".exe");*/

			if (!string.IsNullOrEmpty(buildLocation)) {
				//			string[] dlls = Directory.GetFiles("Assets/Plugins", "*.dll");
				//			foreach (string dll in dlls)
				//				File.Copy(dll, Path.GetDirectoryName(buildLocation) + "/" + Path.GetFileName(dll), true);

				var options = new BuildPlayerOptions {
					scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path)
						.ToArray(),
					locationPathName = buildLocation,
					target = EditorUserBuildSettings.activeBuildTarget,
					options = BuildOptions.ShowBuiltPlayer
				};

				BuildPipeline.BuildPlayer(options);

				var folder = new FileInfo(buildLocation).DirectoryName;

				File.WriteAllText(Path.Combine(folder, "ReleaseNotes.txt"), GetReleaseNotes());
			}
		}

		private void AddVersion(bool unreleased = false) {
			Version lastVer = null;
			if (build.versions.Count > 0) {
				if (unreleased) {
					lastVer = build.versions[0];
				}
				else {
					lastVer = build.CurrentVersion;
					foreach (var version in build.versions.Where(x => x.unreleased)) {
						version.build++;
					}
				}
			}

			if (lastVer == null) {
				lastVer = new Version();
			}

			var v = new Version {
				major = lastVer.major,
				minor = lastVer.minor,
				build = lastVer.build + 1,
				date = DateTime.Now,
				unreleased = unreleased
			};

			var latestIssues = build.CurrentVersion?.notes.Where(x => x.category == Note.Category.KnownIssues)
				.Select(x => (Note) x.Clone());

			if (latestIssues != null) v.notes = latestIssues.ToList();

			build.versions.Add(v);
			build.versions = build.versions.OrderByDescending(ver => ver.major)
				.ThenByDescending(ver => ver.minor)
				.ThenByDescending(ver => ver.build)
				.ThenByDescending(ver => ver.date).ToList();
		}

		void OnGUI() {
			float buttonWidth = 100f;
			float padding = 10f;

			GUILayout.BeginArea(new Rect(padding, padding, position.width - padding * 2f,
				position.height - padding * 2f));
			{
				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Label("Version");


					if (GUILayout.Button("Add release", EditorStyles.miniButton)) {
						AddVersion();
					}


					if (GUILayout.Button("Add unreleased", EditorStyles.miniButton)) {
						AddVersion(true);
					}


					GUILayout.FlexibleSpace();
				}
				EditorGUILayout.EndHorizontal();

				scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
				{
					foreach (var v in build.versions) {
						if (!expanded.ContainsKey(v))
							expanded[v] = v == build.CurrentVersion;

						var oldestUnreleased = build.versions.LastOrDefault(x => x.unreleased) == v;
						var latestReleased = build.versions.FirstOrDefault(x => !x.unreleased) == v;

						EditorGUILayout.BeginHorizontal();
						{
							expanded[v] = EditorGUILayout.Foldout(expanded[v], v.ToString());
							if (expanded[v]) {
								v.yanked = GUILayout.Toggle(v.yanked, "Yanked", GUILayout.Width(60));

								if (v.unreleased) {
									if (oldestUnreleased) {
										if (GUILayout.Button("Release", EditorStyles.miniButton,
											GUILayout.Width(100))) {
											v.unreleased = false;
										}
									}
								}
								else {
									if (latestReleased) {
										if (GUILayout.Button("Unrelease", EditorStyles.miniButton,
											GUILayout.Width(100))) {
											v.unreleased = true;
										}
									}
								}

								if (GUILayout.Button("Delete", EditorStyles.miniButton, GUILayout.Width(100))) {
									if (EditorUtility.DisplayDialog("Remove Version?",
										"Are you sure you want to delete this version?",
										"Yes", "No")) {
										RemoveVersion(v);


										GUIUtility.ExitGUI();
									}
								}
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
							{
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
							}
							EditorGUILayout.EndHorizontal();

							EditorGUILayout.BeginHorizontal();
							{
								EditorGUILayout.LabelField("Notes", GUILayout.Width(100));

								EditorGUILayout.LabelField("Add new:", GUILayout.Width(75));

								foreach (var s in Enum.GetNames(typeof(Note.Category))) {
									var category = (Note.Category) Enum.Parse(typeof(Note.Category), s, true);
									var color = GetColorByCategory(category);

									var oldColor = GUI.contentColor;
									GUI.color = color;
									if (GUILayout.Button(s, EditorStyles.miniButton)) {
										var note = new Note {category = category};
										v.notes.Add(note);
										v.notes = v.notes.OrderBy(n => n.category).ToList();
									}

									GUI.color = oldColor;
								}

								GUILayout.FlexibleSpace();
							}
							EditorGUILayout.EndHorizontal();

							EditorGUI.indentLevel++;
							foreach (Note n in v.notes) {
								EditorGUILayout.BeginHorizontal();
								{
									GUI.changed = false;
									var color = GetColorByCategory(n.category);

									var oldColor = GUI.contentColor;
									GUI.color = color;

									n.category =
										(Note.Category) EditorGUILayout.EnumPopup(n.category, GUILayout.Width(100f));
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

									if (v.unreleased) {
										if (GUILayout.Button("To current", EditorStyles.miniButton,
											GUILayout.Width(80f))) {
											build.CurrentVersion.notes.Add(n);
											v.notes.Remove(n);
											GUIUtility.ExitGUI();
										}
									}
								}
								EditorGUILayout.EndHorizontal();
							}

							EditorGUI.indentLevel--;

							EditorGUI.indentLevel--;
						}
					}
				}
				EditorGUILayout.EndScrollView();

				GUILayout.FlexibleSpace();

				//Settings

				build.editorSettings.saveToPlayerSettings =
					GUILayout.Toggle(build.editorSettings.saveToPlayerSettings, "Save to PlayerSettings");

				GUIStyle myStyle = new GUIStyle(GUI.skin.textField);
				myStyle.alignment = TextAnchor.MiddleRight;

				// save to CHANGELOG.md
				EditorGUILayout.BeginHorizontal();
				{
					build.editorSettings.saveToChangelogMd =
						GUILayout.Toggle(build.editorSettings.saveToChangelogMd, "Save to CHANGELOG.md");
				}

				EditorGUILayout.EndHorizontal();

				// save to package.json
				EditorGUILayout.BeginHorizontal();
				{
					build.editorSettings.saveToPackageJson =
						GUILayout.Toggle(build.editorSettings.saveToPackageJson, "Save to package.json");

					var guiEnabledOld = GUI.enabled;
					GUI.enabled = build.editorSettings.saveToPackageJson;


					build.editorSettings.packageJsonPath =
						GUILayout.TextField(build.editorSettings.packageJsonPath, myStyle, GUILayout.MaxWidth(300));
					if (GUILayout.Button("Browse", EditorStyles.miniButton, GUILayout.Width(buttonWidth))) {
						var projpath = new DirectoryInfo(Application.dataPath).FullName;
						var packageJsonPath = EditorUtility.OpenFilePanel("Choose package.json",
							build.editorSettings.packageJsonPath, "json");
						if (!string.IsNullOrEmpty(packageJsonPath)) {
							var path = new DirectoryInfo(packageJsonPath).FullName;

							string GetRelativePath(string pathFrom, string pathTo) {
								Uri uri = new Uri(pathFrom);
								string relativePath =
									Uri.UnescapeDataString(uri.MakeRelativeUri(new Uri(pathTo)).ToString());
								relativePath =
									relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
								if (!relativePath.Contains(Path.DirectorySeparatorChar.ToString()))
									relativePath = "." + Path.DirectorySeparatorChar + relativePath;
								return relativePath;
							}

							build.editorSettings.packageJsonPath = GetRelativePath(projpath, path);
						}
					}

					GUI.enabled = guiEnabledOld;
				}
				EditorGUILayout.EndHorizontal();


				EditorGUILayout.BeginVertical();
				{
					EditorGUILayout.BeginHorizontal();
					{
						build.editorSettings.buildPathScheme =
							GUILayout.TextField(build.editorSettings.buildPathScheme,
								new GUIStyle(GUI.skin.textField) {stretchWidth = true});
						if (GUILayout.Button("Refresh", EditorStyles.miniButton, GUILayout.Width(buttonWidth))) {
							UpdateDict();
						}

						if (GUILayout.Button("Add parameter", EditorStyles.miniButton, GUILayout.Width(buttonWidth))) {
							var menu = new GenericMenu();
							foreach (var pathParameter in _buildPathDict.Select(x => x.Value)) {
								AddMenu(menu, $"{pathParameter.menuPath}/{pathParameter.description}",
									pathParameter.key);
							}

							menu.ShowAsContext();
						}

						void AddMenu(GenericMenu menu, string menuPath, string option) {
							menu.AddItem(new GUIContent(menuPath), false,
								data => { build.editorSettings.buildPathScheme += $"{{{option}}}"; },
								this);
						}
					}
					EditorGUILayout.EndHorizontal();

					GUILayout.Label(GetBuildPath());
				}
				EditorGUILayout.EndVertical();

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
			}

			GUILayout.EndArea();
		}

		private void RemoveVersion(Version version) {
			build.versions.Remove(version);
			var current = build.CurrentVersion;
			var buildAdd = 1;
			foreach (var v in build.versions.Where(x => x.unreleased).Reverse()) {
				v.major = current.major;
				v.minor = current.minor;
				v.build = current.build + buildAdd++;
			}
		}

		private string GetBuildPath(bool fullPath = true) {
			try {
				var dict = _buildPathDict.ToDictionary(pair => pair.Key, pair => pair.Value.parameter);
				var path = Smart.Format(build.editorSettings.buildPathScheme.Replace("\\", "/"),
					dict
				);
				if (fullPath)
					switch (EditorUserBuildSettings.activeBuildTarget) {
						case BuildTarget.StandaloneWindows:
						case BuildTarget.StandaloneWindows64:
							path += $"/{Application.productName.ReplaceUnsupportedSymbols()}.exe";
							break;
						case BuildTarget.Android:
							path += ".apk";
							break;
						case BuildTarget.iOS:
							break;
						default:
							throw new NotImplementedException(
								$"Building for platform {EditorUserBuildSettings.activeBuildTarget} is not yet supported, " +
								"feel free to write an Issue or contribute: https://github.com/Hermesiss/unity-buildplus");
					}
				
				return Path.GetFullPath(path.ReplaceMultiple('_'));
			}
			catch (Exception e) {
				return $"ERROR: {e}";
			}
		}

		private static Dictionary<Note.Category, Color> _categoryColors;

		private static Dictionary<string, PathParameter> _buildPathDict = new Dictionary<string, PathParameter>();

		private static void UpdateDict() {
			var pathParams = BuildPathAttribute.CallAllMethods();
			_buildPathDict = new Dictionary<string, PathParameter>();
			foreach (var pathParameter in pathParams) {
				_buildPathDict.Add(pathParameter.key, pathParameter);
			}
		}

		private static Color GetColorByCategory(Note.Category category) => _categoryColors[category];
	}
}
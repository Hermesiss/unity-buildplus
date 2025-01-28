using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Trismegistus.BuildPlus {
	public class ChangelogBuilder {
		public enum ChangeType {
			Added,
			Changed,
			Deprecated,
			Removed,
			Fixed,
			Security
		}

		private readonly Build _build;
		
		private readonly bool _compact;

		private readonly string _filePath;

		private readonly Dictionary<ChangeType, Version.Note.Category[]> _typeNoteLink =
			new Dictionary<ChangeType, Version.Note.Category[]> {
				{
					ChangeType.Added, new[] {Version.Note.Category.Features}
				}, {
					ChangeType.Changed, new[] {
						Version.Note.Category.Changes,
						Version.Note.Category.Improvements,
						Version.Note.Category.General
					}
				}, {
					ChangeType.Removed, new[] {
						Version.Note.Category.Removed
					}
				}, {
					ChangeType.Fixed, new[] {
						Version.Note.Category.Fixes
					}
				}, {
					ChangeType.Security, new[] {
						Version.Note.Category.KnownIssues
					}
				}, {
					ChangeType.Deprecated, new[] {
						Version.Note.Category.Deprecated
					}
				}
			};

		public ChangelogBuilder(Build build, bool compact) {
			_build = build;
			_compact = compact;
			_filePath = Path.Combine(new DirectoryInfo(Application.dataPath).Parent.FullName, "CHANGELOG.md");
		}

		public void Build() {
			var b = new StringBuilder();
			b.AppendLine(@"
# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).");
			var unreleased = _build.versions.Where(x => x.unreleased)
				.SelectMany(x => x.notes).ToList();

			b.AppendLine("\n## Unreleased\n");

			if (unreleased.Any()) {
				b.AppendLine(FormatNotes(unreleased));
			}

			var released = _build.versions.Where(x => !x.unreleased);


			foreach (var version in released) {
				var yanked = version.yanked ? " [YANKED]" : "";
				b.AppendLine(
					$"\n## {version.major}.{version.minor}.{version.build} - {version.date:yyyy-MM-dd}{yanked}\n");

				b.Append(FormatNotes(version.notes));
			}

			var result = Regex.Replace(b.ToString(), @"^\s+$[\r\n]*", "\n", RegexOptions.Multiline);
			File.WriteAllText(_filePath, result);
		}

		private string FormatNotes(List<Version.Note> notes) {
			var sb = new StringBuilder();
			foreach (var changeType in Enum.GetNames(typeof(ChangeType))
				.Select(x => (ChangeType) Enum.Parse(typeof(ChangeType), x))) {
				sb.Append(FormatNotes(notes, changeType));
			}

			return sb.ToString();
		}

		private string FormatNotes(List<Version.Note> notes, ChangeType changeType) {
			var sb = new StringBuilder();


			var filtered = notes.Where(x => _typeNoteLink[changeType].Contains(x.category)).ToArray();
			if (filtered.Length == 0) return "";
			
			if (!_compact)
			{
				sb.AppendLine($"\n### {changeType}\n");
			}
			
			

			foreach (var b in filtered)
			{
				sb.Append("- ");
				if (_compact)
				{
					sb.Append($"{changeType}: ");
				}
				
				sb.AppendLine(b.description);
			}

			return sb.ToString();
		}
	}
}
// Build+ Unity Extension
// Copyright (c) 2012 Luminary Productions, Inc.
// Please direct any bugs/comments/suggestions to http://luminaryproductions.net

using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Trismegistus.BuildPlus
{
	[Serializable]
	public class Version : XmlSerializableData<Version>
	{
		public const string KDateFormat = "yyyy.MM.dd.HHmm";
		
		[Serializable]
		public class Note : ICloneable
		{
			public enum Category
			{
				Hidden,
				Features,
				Improvements,
				Fixes,
				Changes,
				KnownIssues,
				General,
				Removed,
				Deprecated
			}
			
			public Category category;
			public string description;
			
			
			public Note(Category category1, string s) {
				category = category1;
				description = s;
			}

			public Note() {
				category = Category.General;
				description = string.Empty;
			}

			public object Clone() {
				return new Note(category, description);
			}
		}
		
		public int major;
		public int minor;
		public int build;
		public string revision = string.Empty;
		public DateTime date;
		public string dateString; // workaround for Unity not serializing DateTime to ScriptableObjects
		/// <summary>
		/// Try if release is fixing serious bug or security issue
		/// <see cref="https://github.com/olivierlacan/keep-a-changelog/blob/0417b6b4e824f459de3ad57c8ba7d4ea0967329c/README.md#what-about-yanked-releases"/>
		/// </summary>
		public bool yanked;

		public bool unreleased;
			
		
		public List<Note> notes = new List<Note>();
		
		public override string ToString()
		{
			if (date != DateTime.MinValue)
				dateString = date.ToString(KDateFormat);
			var yankedStr = yanked ? "[YANKED] " : "";
			var unreleasedStr = unreleased ? "***" : "";
			return $"{unreleasedStr}{major}.{minor}.{build}{revision} {yankedStr}({dateString})";
		}
		
		public override void PreSerialize()
		{
			base.PreSerialize();
			FillDate();
		}

		public void FillDate() {
			date = DateTime.ParseExact(dateString, KDateFormat, null);
			//dateString = date.ToString(Version.kDateFormat);
		}
	}
}
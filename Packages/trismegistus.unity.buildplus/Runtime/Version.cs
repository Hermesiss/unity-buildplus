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
				General
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
		
		public List<Note> notes = new List<Note>();
		
		public override string ToString()
		{
			if (date != DateTime.MinValue)
				dateString = date.ToString(KDateFormat);
			return string.Format("{0}.{1}.{2}{3} ({4})", major, minor, build, revision, dateString);
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
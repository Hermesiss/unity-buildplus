// Build+ Unity Extension
// Copyright (c) 2012 Luminary Productions, Inc.
// Please direct any bugs/comments/suggestions to http://luminaryproductions.net

using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace BuildPlus
{
	[Serializable]
	public class Version : XmlSerializableData<Version>
	{
		static public readonly string kDateFormat = "yyyy.MM.dd.HHmm";
		
		[Serializable]
		public class Note
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
			
			public Category category = Category.General;
			public string description = string.Empty;
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
				dateString = date.ToString(kDateFormat);
			return string.Format("{0}.{1}.{2}{3} ({4})", major, minor, build, revision, dateString);
		}
		
		public override void PreSerialize()
		{
			base.PreSerialize();
			dateString = date.ToString(Version.kDateFormat);
		}
	}
}
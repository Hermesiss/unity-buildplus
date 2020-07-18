using System;

namespace Trismegistus.BuildPlus {
	[Serializable]
	public class PathParameter {
		/// <summary>
		/// This will be inside your build path scheme 
		/// </summary>
		public readonly string key;

		/// <summary>
		/// One or many-level menu path with / as delimiter 
		/// </summary>
		public readonly string menuPath;

		/// <summary>
		/// Calculated value for <see cref="key"/>
		/// </summary>
		public readonly string parameter;

		/// <summary>
		/// Description text that will appear in menu
		/// </summary>
		public readonly string description;

		public override string ToString() {
			return $"{menuPath}/{parameter}: {description}";
		}

		public PathParameter(string key, string menuPath, string parameter, string description) {
			this.menuPath = menuPath;
			this.parameter = parameter;
			this.description = description;
			this.key = key;
		}
	}
}
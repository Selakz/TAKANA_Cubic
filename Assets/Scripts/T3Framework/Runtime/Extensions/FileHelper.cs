using System;
using System.IO;

namespace T3Framework.Runtime.Extensions
{
	public static class FileHelper
	{
		public static string GetAbsolutePathFromRelative(string absoluteBasePath, string relativePath)
		{
			if (absoluteBasePath == null) return null;
			else if (relativePath == null) return absoluteBasePath;

			string baseDirectory = Path.GetDirectoryName(absoluteBasePath);
			if (baseDirectory == null)
				throw new ArgumentException("The absoluteBasePath is not a valid file path.", nameof(absoluteBasePath));

			Uri baseUri = new Uri(baseDirectory + Path.DirectorySeparatorChar);
			Uri relativeUri = new Uri(relativePath, UriKind.Relative);
			Uri targetUri = new Uri(baseUri, relativeUri);
			return targetUri.LocalPath;
		}
	}
}
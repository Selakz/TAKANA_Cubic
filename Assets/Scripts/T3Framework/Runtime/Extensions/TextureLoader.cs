using System.IO;
using UnityEngine;

namespace T3Framework.Runtime.Extensions
{
	public static class TextureLoader
	{
		public static Texture2D LoadTexture2D(string path)
		{
			byte[] file;
			try
			{
				file = File.ReadAllBytes(path);
			}
			catch
			{
				return null;
			}

			Texture2D texture = new Texture2D(1, 1);
			bool success = texture.LoadImage(file, true);
			if (success)
			{
				texture.wrapMode = TextureWrapMode.Clamp;
				texture.name = path;
				texture.mipMapBias = -4;
				return texture;
			}
			else
			{
				Object.Destroy(texture);
				return null;
			}
		}
	}
}
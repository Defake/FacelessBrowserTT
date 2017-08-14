using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;

namespace FacelessBrowserTT.App_Code
{
	/// <summary>
	/// Saves an image to a server directory as a file
	/// </summary>
	public class FileImageSaver : IImageSaver
	{
		public void Save(Image img, string fullPath)
		{
			var directory = Path.GetDirectoryName(fullPath);

			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			if (File.Exists(fullPath))
				File.Delete(fullPath);

			img.Save(fullPath);
		}

	}
}
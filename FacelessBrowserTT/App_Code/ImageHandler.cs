using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using ImageModification;

namespace FacelessBrowserTT.App_Code
{
	public class ImageHandler
	{
		private Image _image;

		private ImageHandler(Image image)
		{
			_image = image;
		}

		public static ImageHandler CreateImageFromUrl(string url)
		{
			ImageHandler ih = null;
			try
			{
				var request = WebRequest.Create(url);
				using (var response = request.GetResponse())
				using (var stream = response.GetResponseStream())
					if (stream != null)
						ih = new ImageHandler(Image.FromStream(stream));
			}
			catch (Exception)
			{
				Debug.WriteLine($"Couldn't load {url}");
			}

			return ih;
		}

		public static async Task<ImageHandler> CreateImageFromUrlAsync(string url)
		{
			ImageHandler ih = null;

			try
			{
				var request = WebRequest.Create(url);
				using (var response = await request.GetResponseAsync())
				using (var stream = response.GetResponseStream())
					if (stream != null)
						ih = new ImageHandler(Image.FromStream(stream));
			}
			catch (Exception)
			{
				Debug.WriteLine($"Couldn't load {url}");
			}

			return ih;
		}

		public static ImageHandler CreateImageFromPath(string path)
		{
			using (var source = Image.FromFile(path))
				return new ImageHandler(new Bitmap(source));
		}

		public bool Save(IImageSaver imageSaver, string path)
		{
			try
			{
				imageSaver.Save(_image, path);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public ImageHandler Modify(IImageModificator imageModificator)
		{
			_image = imageModificator.Modify(_image);
			return this;
		}
	}
}
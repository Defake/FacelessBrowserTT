using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using FaceDetection;
using ImageModification;
using FacelessBrowserTT.App_Code;

namespace FacelessBrowserTT.Controllers
{
    public class HomeController : Controller
    {
		private static readonly HttpClient Client = new HttpClient();
		private const string DownloadFolder = "/Temp";

		// GET: Home
		public ActionResult Index()
		{
			return View();
		}

		public async Task<string> GetPage(string address)
		{
			var page = Client.GetStringAsync(address).Result;
			return await PreparePage(page, address);
		}

	    private async Task<string> PreparePage(string page, string address)
	    {
			var pageEditor = new PageEditor(page, address);
			pageEditor.FixRelativeUrls();

			List<string> imagesPaths;
			// Replace external images with internal downloaded images and get list of downloaded images paths
			try
		    {
				imagesPaths = await pageEditor.ReplaceExternalImagesAsync(Server.MapPath("~" + DownloadFolder), DownloadFolder);
			}
		    catch (Exception e)
		    {
			    Debug.WriteLine("LOADING IMAGES ERR: " + e);
			    throw new Exception(e.Message);
		    }

			// Modify all downloaded images to blur faces
			var faceBlurModificator = new FaceBlurModificator(new FaceDetector());
			var faceBluringTasks = imagesPaths.Select(imagePath => Task.Run(() =>
			{
				ImageHandler.CreateImageFromPath(imagePath)
					.Modify(faceBlurModificator)
					.Save(new FileImageSaver(), imagePath);
			}));

			Task.WhenAll(faceBluringTasks).Wait();

		    return pageEditor.Page;
	    }

	}
}
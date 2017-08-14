using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FacelessBrowserTT.App_Code
{
	public class PageEditor
	{
		public string Page { get; private set; }

		private readonly string _address;
		private int _filesCounter;

		public PageEditor(string page, string address)
		{
			Page = page;
			_filesCounter = 0;

			// Address is always like "http://site.com/" with a slash at the end
			_address = Regex.Match(address, @"^(https?:\/\/)([a-zA-Z1-90\\.]+)").Value + "/";
		}

		/// <summary>
		/// Replaces all relative url paths to the absolute
		/// </summary>
		public void FixRelativeUrls()
		{
			// "(<[^<>]*)(href|src|srcset|url)(\\(|=)([\"']?)([^'\"\\s<>]+)([\"']?)([^<>]*>)" // my regex for all urls 
			//     "(\\s)(href|src|srcset|url)(\\(|=)([\"']?)([^'\"\\s<>]+)([\"']?)(\\s|>|\\/>)" // 2nd ver without tags (better)
			// "([\"'])([^'\"\\s]+\\.)(?i)(png|jpg|gif|jpeg|svg)(?-i)([\"'])" // regex for img urls

			ReplaceUrlsWithPattern(
				"(href|src|srcset|url|content)(?:(?:(\\(|=)([\"']?))|(,\\s*))([^'\"\\s,<>\\(\\)]+)(\\)|[\"']|,)", 4);

			// If we didn't replace some urls like "//yastatic.net/social/current/sprites/ico-16.png"
			ReplaceUrlsWithPattern(
				"(?:(\\(|[\"'])|(,\\s*))(\\/\\/[^'\"\\s,<>\\(\\)]+)(\\)|[\"']|,)", 2);
		}

		/// <summary>
		/// Downloads all images and replaces images' src attrubute 
		/// to local path to downloaded images
		/// </summary>
		/// <param name="downloadPath">Absolute path of images download folder</param>
		/// <param name="htmlPath">Relative path of download folder to use it in html WITHOUT A SLASH AT THE END</param>
		/// <returns>Path to all saved images</returns>
		public async Task<List<string>> ReplaceExternalImagesAsync(string downloadPath, string htmlPath)
		{
			var imgUrlMatches = Regex.Matches(Page, "([\"'])([^'\"\\s]+\\.)(?i)(png|jpg|gif|jpeg)(?-i)([\"'])");

			// Start loading all images
			var imageLoadingTasksDict =
				imgUrlMatches.OfType<Match>()
					.Select(match => match.Groups[2].Value + match.Groups[3].Value)
					.Distinct()
					.ToDictionary(url => url, ImageHandler.CreateImageFromUrlAsync);

			var savingHandlers = new Dictionary<string, Task<string>>();
			foreach (var taskPair in imageLoadingTasksDict)
			{
				var imgHandler = await taskPair.Value;
				if (imgHandler != null)
				{
					var url = taskPair.Key;
					var localFileName = CreateLocalFileNameFromUrl(url);
					savingHandlers.Add(url, Task.Run(() => imgHandler.Save(new FileImageSaver(), Path.Combine(downloadPath, localFileName)) ? localFileName : null));
				}
			}
			
			var internalImagesPaths = new List<string>();
			foreach (var handlerPair in savingHandlers)
			{
				var relativeImagePath = handlerPair.Value.Result;
				if (relativeImagePath != null)
				{
					var url = handlerPair.Key;
					Page = Page.Replace(url, htmlPath + '/' + relativeImagePath);
					internalImagesPaths.Add(Path.Combine(downloadPath, relativeImagePath));
				}
			}
			
			return internalImagesPaths;
		}

		/// <summary>
		/// Transforms resource's url to a proper file name
		/// </summary>
		/// <param name="url">Absolute url of a resurce</param>
		/// <returns>Proper file name for a local system</returns>
		private string CreateLocalFileNameFromUrl(string url)
		{
			var extension = new Regex(@"(\.\w+)$").Match(url).Groups[1].Value;

			var name = _filesCounter.ToString() + extension;
			_filesCounter++;
			return name;
		}

		/// <summary>
		/// Checks if a provided url is relative and makes it absolute.
		/// If the provided url is already absolute, just returns it
		/// </summary>
		/// <param name="url">Absolute or relative url of some resource</param>
		/// <returns>Absolute url</returns>
		private string ConvertToAbsoluteUrl(string url)
		{
			Uri result;
			return Uri.TryCreate(new Uri(_address), url, out result) ? result.AbsoluteUri : url;
		}

		private void ReplaceUrlsWithPattern(string pattern, int index)
		{
			Page = Regex.Replace(Page, pattern,
						match => match.Groups
							.OfType<Group>()
							.Skip(1)
							.Select((g, i) => i == index ? ConvertToAbsoluteUrl(g.Value) : g.Value)
							.Aggregate(string.Concat));
		}
	}
}
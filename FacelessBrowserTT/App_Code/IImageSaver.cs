using System.Drawing;

namespace FacelessBrowserTT.App_Code
{
	public interface IImageSaver
	{
		void Save(Image img, string path);
	}
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperfastBlur;

namespace ImageModification
{
	public class BlurModificator : IImageModificator
	{
		private readonly Rectangle _blurRect;
		private readonly int _radius;

		public BlurModificator(Rectangle blurRect, int radius)
		{
			_blurRect = blurRect;
			_radius = radius;
		}

		public Image Modify(Image img)
		{
			var bitmap = new Bitmap(img);
			using (var blur = new GaussianBlur(bitmap))
				bitmap = blur.Process(_radius, _blurRect);

			return bitmap;
		}



	}
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SuperfastBlur
{
	/// <summary>
	/// Code source: https://github.com/mdymel/superfastblur
	/// I just added ability to choose bluring area with a Rectange object
	/// </summary>
	public class GaussianBlur : IDisposable
	{
		private int[] _red;
		private int[] _green;
		private int[] _blue;

		private int _width;
		private int _height;

		private readonly ParallelOptions _pOptions = new ParallelOptions { MaxDegreeOfParallelism = 16 };

		public GaussianBlur(Bitmap image)
		{
			var rct = new Rectangle(0, 0, image.Width, image.Height);
			var source = new int[rct.Width * rct.Height];
			var bits = image.LockBits(rct, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
			Marshal.Copy(bits.Scan0, source, 0, source.Length);
			image.UnlockBits(bits);

			_width = image.Width;
			_height = image.Height;

			_red = new int[_width * _height];
			_green = new int[_width * _height];
			_blue = new int[_width * _height];

			Parallel.For(0, source.Length, _pOptions, i =>
			{
				_red[i] = (source[i] & 0xff0000) >> 16;
				_green[i] = (source[i] & 0x00ff00) >> 8;
				_blue[i] = (source[i] & 0x0000ff);
			});
		}

		public Bitmap Process(int radial, Rectangle rect)
		{
			if ((radial + 1) * 2 >= _height || (radial + 1) * 2 >= _width)
				throw new ArgumentException("Blur radius may not be greater than half of any image dimension");

			var newRed = new int[_width * _height];
			var newGreen = new int[_width * _height];
			var newBlue = new int[_width * _height];
			var dest = new int[_width * _height];

			Parallel.Invoke(
				() => gaussBlur_4(_red, newRed, radial, rect),
				() => gaussBlur_4(_green, newGreen, radial, rect),
				() => gaussBlur_4(_blue, newBlue, radial, rect));

			Parallel.For(0, dest.Length, _pOptions, i =>
			{
				if (newRed[i] > 255) newRed[i] = 255;
				if (newGreen[i] > 255) newGreen[i] = 255;
				if (newBlue[i] > 255) newBlue[i] = 255;

				if (newRed[i] < 0) newRed[i] = 0;
				if (newGreen[i] < 0) newGreen[i] = 0;
				if (newBlue[i] < 0) newBlue[i] = 0;

				dest[i] = (int)(0xff000000u | (uint)(newRed[i] << 16) | (uint)(newGreen[i] << 8) | (uint)newBlue[i]);
			});

			var image = new Bitmap(_width, _height);
			var rct = new Rectangle(0, 0, image.Width, image.Height);
			var bits2 = image.LockBits(rct, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
			Marshal.Copy(dest, 0, bits2.Scan0, dest.Length);
			image.UnlockBits(bits2);
			return image;
		}

		private void gaussBlur_4(int[] source, int[] dest, int r, Rectangle rect)
		{
			var bxs = boxesForGauss(r, 3);
			boxBlur_4(source, dest, _width, _height, (bxs[0] - 1) / 2, rect);
			boxBlur_4(dest, source, _width, _height, (bxs[1] - 1) / 2, rect);
			boxBlur_4(source, dest, _width, _height, (bxs[2] - 1) / 2, rect);
		}

		private int[] boxesForGauss(int sigma, int n)
		{
			var wIdeal = Math.Sqrt((12 * sigma * sigma / n) + 1);
			var wl = (int)Math.Floor(wIdeal);
			if (wl % 2 == 0) wl--;
			var wu = wl + 2;

			var mIdeal = (double)(12 * sigma * sigma - n * wl * wl - 4 * n * wl - 3 * n) / (-4 * wl - 4);
			var m = Math.Round(mIdeal);

			var sizes = new List<int>();
			for (var i = 0; i < n; i++) sizes.Add(i < m ? wl : wu);
			return sizes.ToArray();
		}

		private void boxBlur_4(int[] source, int[] dest, int w, int h, int r, Rectangle rect)
		{
			for (var i = 0; i < source.Length; i++) dest[i] = source[i];
			boxBlurH_4(dest, source, w, h, r);
			boxBlurT_4(source, dest, w, h, r, rect);
		}

		private void boxBlurH_4(int[] source, int[] dest, int w, int h, int r)
		{
			var iar = (double)1 / (r + r + 1);
			Parallel.For(0, h, _pOptions, i =>
			{
				// i - cur line
				var ti = i * w; // ti - cur pixel
				var li = ti; // li - first pixel of the pixel line
				var ri = ti + r; // center pixel of blur radius?
				var fv = source[ti]; // first pixel of the line
				var lv = source[ti + w - 1]; // last pixel of the line
				var val = (r + 1) * fv; // wtf?? val of blured pixel?

				// Cycle to gather all pixel colors inside radius
				for (var j = 0; j < r; j++)
					val += source[ti + j];

				for (var j = 0; j <= r; j++)
				{
					val += source[ri++] - fv;
					dest[ti++] = (int)Math.Round(val * iar);
				}
				for (var j = r + 1; j < w - r; j++)
				{
					val += source[ri++] - dest[li++];
					dest[ti++] = (int)Math.Round(val * iar);
				}
				for (var j = w - r; j < w; j++)
				{
					val += lv - source[li++];
					dest[ti++] = (int)Math.Round(val * iar);
				}



			});
		}

		private void boxBlurT_4(int[] source, int[] dest, int imgWidth, int imgHeight, int r, Rectangle target)
		{
			var bluredHeight = Math.Min(target.Height, imgHeight);
			var iar = (double)1 / (r + r + 1);
			Parallel.For(target.X, Math.Min(imgWidth, target.X + target.Width), _pOptions, i =>
			{
				var ti = i + target.Y * imgWidth;
				var li = ti;
				var ri = ti + r * imgWidth;
				var fv = source[ti];
				var lv = source[ti + imgWidth * (bluredHeight - 1)];
				var val = (r + 1) * fv;

				for (var j = 0; j < r; j++) val += source[ti + j * imgWidth];

				for (var j = 0; j <= r; j++)
				{
					val += source[ri] - fv;
					dest[ti] = (int)Math.Round(val * iar);
					ri += imgWidth;
					ti += imgWidth;
				}

				for (var j = r + 1; j < bluredHeight - r; j++)
				{
					val += source[ri] - source[li];
					dest[ti] = (int)Math.Round(val * iar);
					li += imgWidth;
					ri += imgWidth;
					ti += imgWidth;
				}

				for (var j = bluredHeight - r; j < bluredHeight; j++)
				{
					val += lv - source[li];
					dest[ti] = (int)Math.Round(val * iar);
					li += imgWidth;
					ti += imgWidth;
				}
			});
		}

		public void Dispose()
		{
			_red = null;
			_green = null;
			_blue = null;
			_width = 0;
			_height = 0;
		}
	}
}

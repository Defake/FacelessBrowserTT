using System;
using System.Drawing;
using FaceDetection;
using SuperfastBlur;

namespace ImageModification
{
	public class FaceBlurModificator : IImageModificator
	{
		private readonly FaceDetector _faceDetector;

		public FaceBlurModificator(FaceDetector faceDetector)
		{
			_faceDetector = faceDetector;
		}

		public Image Modify(Image img)
		{
			var bitmap = new Bitmap(img);

			var faces = _faceDetector.GetFaces(bitmap);			
			foreach (var face in faces)
			{
				bitmap = new BlurModificator(face, Math.Min(12, Math.Max(2, 1 + (int) ((face.Width + face.Height) / 2f / 35f)))).Modify(img) as Bitmap;
			}

			return bitmap;
		}
	}
}

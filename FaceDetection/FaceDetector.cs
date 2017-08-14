using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Emgu.CV;
using Emgu.CV.Structure;

namespace FaceDetection
{
    public class FaceDetector
    {
	    private const string CascadeFront = "haarcascade_frontalface_default.xml";
	    private const string CascadeProfile = "haarcascade_profileface.xml";

		// great workaround T_T
	    private static string GetCascadePath(string cascadeName) 
			=> Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\FaceDetection\cascades\", cascadeName);

	    public List<Rectangle> GetFaces(Bitmap img)
	    {
			using (var bgrImage = new Image<Bgr, byte>(img))
			using (var grayImage = bgrImage.Convert<Gray, byte>())
			{
				var imgSize = new Size(img.Width, img.Height);

				var frontFaceClassifier = new CascadeClassifier(GetCascadePath(CascadeFront));
				var profileFaceClassifier = new CascadeClassifier(GetCascadePath(CascadeProfile));

				var rects = new List<Rectangle>();
				try
				{
					rects.AddRange(frontFaceClassifier.DetectMultiScale(grayImage, 1.09, 1, new Size(15, 15), imgSize));
					rects.AddRange(profileFaceClassifier.DetectMultiScale(grayImage, 1.09, 1, new Size(30, 30), imgSize));
				}
				catch (Exception e)
				{
					Debug.WriteLine(e);
				}

				return rects;
			}
		}

	    public List<Rectangle> GetFaces(Image img) 
			=> GetFaces(new Bitmap(img));
    }
}

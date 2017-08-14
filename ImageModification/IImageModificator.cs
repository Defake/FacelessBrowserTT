using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageModification
{
	public interface IImageModificator
	{
		Image Modify(Image img);
	}
}

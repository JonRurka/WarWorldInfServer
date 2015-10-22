using System;
using System.Collections.Generic;
using System.Drawing;

namespace Libnoise
{
	/// <summary>
	/// All SerializationStructs that require type information
	/// when serializing.
	/// </summary>
	public class SerializationStructs
	{

		// To Client
		public struct ImageFileData{
			public string file;
			public List<Color> image;
			
			public ImageFileData(string file, List<Color> image){
				this.file = file;
				this.image = image;
			}
		}
	}
}


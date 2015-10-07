using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.FileProviders;

namespace MakingSense.AspNet.Documentation
{
	public class HtmlDocumentHandler : BaseDocumentHandler
	{
		protected override string[] AcceptedExtensions => new[] { ".html", ".htm" };

		public HtmlDocumentHandler(IFileProvider fileProvider, string subpath)
			: base(fileProvider, subpath)
		{ }

		public override DocumentContent Open()
		{
			return new DocumentContent(FileInfo.CreateReadStream(), FileInfo.Length);
		}
	}
}

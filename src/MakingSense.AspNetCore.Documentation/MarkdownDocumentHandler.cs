using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.FileProviders;

namespace MakingSense.AspNetCore.Documentation
{
	public class MarkdownDocumentHandler : BaseDocumentHandler
	{
		protected override string[] AcceptedExtensions => new[] { ".md", ".markdown" };

		public MarkdownDocumentHandler(IFileProvider fileProvider, string subpath)
			: base(fileProvider, subpath)
		{ }

		public override DocumentContent Open()
		{
			using (var inputStream = FileInfo.CreateReadStream())
			using (var reader = new StreamReader(inputStream))
			{
				var outputStream = new MemoryStream();
				var writer = new StreamWriter(outputStream);
				//TODO: add anchors for titles
				// * See https://github.com/Knagis/CommonMark.NET/wiki#htmlformatter
				// * Or https://github.com/aspnet/vsweb-docs/blob/master/src/app_code/Helpers.cshtml#L149
				CommonMark.CommonMarkConverter.Convert(reader, writer);
				writer.Flush();
				outputStream.Position = 0;
				return new DocumentContent(outputStream, outputStream.Length);
			}
		}
	}
}

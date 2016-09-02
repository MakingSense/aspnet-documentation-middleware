using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace MakingSense.AspNet.Documentation
{
	public class DefaultDocumentHandlerResolver : IDocumentHandlerResolver
	{
		protected virtual IEnumerable<IDocumentHandler> GetHandlers(IFileProvider fileProvider, string subpath)
		{
			yield return new HtmlDocumentHandler(fileProvider, subpath);
			yield return new MarkdownDocumentHandler(fileProvider, subpath);
		}

		public bool TryResolveHandler(IFileProvider fileProvider, string subpath, out IDocumentHandler handler)
		{
			handler = GetHandlers(fileProvider, subpath)
				.Where(x => x.CanHandleRequest)
				.FirstOrDefault();
			return handler != null;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace MakingSense.AspNet.Documentation
{
	public interface IDocumentHandlerResolver
	{
		bool TryResolveHandler(IFileProvider fileProvider, string subpath, out IDocumentHandler handler);
	}
}

using Microsoft.AspNet.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MakingSense.AspNet.Documentation
{
    public interface IDocumentHandlerResolver
    {
		bool TryResolveHandler(IFileProvider fileProvider, string subpath, out IDocumentHandler handler);
    }
}

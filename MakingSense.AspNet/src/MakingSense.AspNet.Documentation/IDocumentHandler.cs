using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MakingSense.AspNet.Documentation
{
	public interface IDocumentHandler
	{
		bool CanHandleRequest { get; }
		DateTimeOffset? LastModified { get; }
		DocumentContent Open();
		int StatusCode { get; }
	}
}

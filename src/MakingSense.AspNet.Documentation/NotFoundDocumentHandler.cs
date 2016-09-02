using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace MakingSense.AspNet.Documentation
{
	public class NotFoundDocumentHandler : IDocumentHandler
	{
		public IFileInfo _notFoundFile;

		public bool CanHandleRequest => _notFoundFile != null;

		public DateTimeOffset? LastModified => null;

		public int StatusCode => StatusCodes.Status404NotFound;

		public NotFoundDocumentHandler(IFileInfo notFoundFile)
		{
			_notFoundFile = notFoundFile;
		}

		public DocumentContent Open()
		{
			return new DocumentContent(_notFoundFile.CreateReadStream(), _notFoundFile.Length);
		}
	}
}

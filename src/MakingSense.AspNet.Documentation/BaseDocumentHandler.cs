using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace MakingSense.AspNet.Documentation
{
	public abstract class BaseDocumentHandler : IDocumentHandler
	{
		protected abstract string[] AcceptedExtensions { get; }

		protected IFileInfo FileInfo { get; private set; }

		public bool CanHandleRequest => FileInfo != null;

		public DateTimeOffset? LastModified => FileInfo?.LastModified;

		public int StatusCode => StatusCodes.Status200OK;

		public BaseDocumentHandler(IFileProvider fileProvider, string subpath)
		{
			FileInfo = AcceptedExtensions
				.Select(x => fileProvider.GetFileInfo(subpath + x))
				.Where(x => x.Exists)
				.FirstOrDefault();
		}

		public abstract DocumentContent Open();
	}
}

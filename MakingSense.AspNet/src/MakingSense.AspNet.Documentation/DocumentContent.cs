using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MakingSense.AspNet.Documentation
{
	public class DocumentContent : IDisposable
	{
		public Stream ContentStream { get; private set; }
		public long Length { get; private set; }

		public DocumentContent(Stream contentStream, long length)
		{
			ContentStream = contentStream;
			Length = length;
		}

		public void Dispose()
		{
			var stream = ContentStream;
			ContentStream = null;
			if (stream != null)
			{
				ContentStream.Dispose();
			}
		}
	}
}

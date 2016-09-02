using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace MakingSense.AspNet.Documentation
{
	public class DirectoryHandler : IDocumentHandler
	{
		public bool CanHandleRequest { get; set; }

		public DateTimeOffset? LastModified { get; internal set; }

		public int StatusCode => StatusCodes.Status200OK;

		private IFileProvider _fileProvider;

		private string _path;

		private Regex _extensionsOptionsRegex;

		private DirectoryOptions _directoryOptions;

		public class DirectoryEntry
		{
			public string Name { get; set; }
			public bool IsDirectory { get; set; }
		}

		public DirectoryHandler(IFileProvider fileProvider, string path, DirectoryOptions options)
		{
			_fileProvider = fileProvider;
			_path = path;
			_extensionsOptionsRegex = new Regex(String.Join("|", options.DirectoryBrowsingStripExtensions.Select(i => String.Concat(i.FirstOrDefault() == '.' ? @"\" : "", i, @"$"))));
			_directoryOptions = options;
			CanHandleRequest = _fileProvider.GetDirectoryContents(_path).Exists;
		}

		public DocumentContent Open()
		{
			var directoryEntries = _fileProvider.GetDirectoryContents(_path).Select(x => new { Name = RemoveExtension(x.Name), IsDirectory = x.IsDirectory });

			var htmlListItems = directoryEntries.Select(x => _directoryOptions.DirectoryListItemTemplate.Replace("{href}", x.Name + (x.IsDirectory ? "/" : "")).Replace("{name}", x.Name + (x.IsDirectory ? "/" : "")));
			var content = GetHtmlContent(htmlListItems);

			var contentAsBytes = System.Text.Encoding.UTF8.GetBytes(content);
			var contentAsStream = new MemoryStream(contentAsBytes);

			return new DocumentContent(contentAsStream, contentAsStream.Length);
		}

		private string GetHtmlContent(IEnumerable<string> directoryList)
		{
			var cleanPath = _path.Trim('/');
			var directoryItems = String.Join("", directoryList.ToArray());
			return _directoryOptions.DirectoryListTemplate.Replace("{path}", cleanPath).Replace("{items}", directoryItems);

		}

		private string RemoveExtension(string filePath)
		{
			return _extensionsOptionsRegex.Replace(filePath, String.Empty);
		}
	}
}

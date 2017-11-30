using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

namespace MakingSense.AspNetCore.Documentation
{
	public class DocumentationOptions
	{
		/// <summary>
		/// The relative URL path that maps to FileProvider resources.
		/// </summary>
		public PathString RequestPath { get; set; } = PathString.Empty;

		/// <summary>
		/// The relative FileProvider path that maps from URL.
		/// </summary>
		public PathString FileProviderSubPath { get; set; } = PathString.Empty;

		/// <summary>
		/// The file system used to locate resources
		/// </summary>
		public IFileProvider FileProvider { get; set; }

		/// <summary>
		/// Not found handling is disabled by default, set NotFoundHtmlPath to enable it.
		/// </summary>
		public bool EnableNotFoundHandling => NotFoundHtmlPath != null || NotFoundHtmlFile != null;

		[Obsolete("Use NotFoundHtmlPath property")]
		public IFileInfo NotFoundHtmlFile { get; set; }

		public string NotFoundHtmlPath { get; set; }

		/// <summary>
		/// Default files are disabled by default, set DefaultFileName to enable it.
		/// </summary>
		public bool EnableDefaultFiles => !string.IsNullOrWhiteSpace(DefaultFileName);

		public string DefaultFileName { get; set; }

		public TimeSpan CacheMaxAge { get; set; }

		[Obsolete("Use LayoutFilePath property")]
		public IFileInfo LayoutFile { get; set; }

		public string LayoutFilePath { get; set; }

		public DirectoryOptions DirectoryOptions { get; set; } = new DirectoryOptions();

		public string DefaultLanguage { get; set; }

		/// <summary>
		/// Use this property among LayoutFilePath.
		/// </summary>
		public string[] SupportedLanguages { get; set; } = new string[] { };

		internal void ResolveFileProvider(IHostingEnvironment hostingEnv)
		{
			if (FileProvider == null)
			{
				FileProvider = hostingEnv.WebRootFileProvider;
			}
		}
	}

	public class DirectoryOptions
	{
		public bool EnableDirectoryBrowsing { get; set; } = true;

		public string[] DirectoryBrowsingStripExtensions { get; set; } = new[] { ".html", ".htm", ".markdown", ".md" };

		public string DirectoryListTemplate { get; set; } = @"<h1>{path}</h1><ul>{items}</ul>";

		public string DirectoryListItemTemplate { get; set; } = @"<li><a href=""{href}"">{name}</a></li>";
	}
}

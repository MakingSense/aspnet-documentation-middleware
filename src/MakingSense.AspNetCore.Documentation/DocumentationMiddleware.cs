using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Framework.Internal;
using System.Text.RegularExpressions;
using Microsoft.Extensions.FileProviders;

namespace MakingSense.AspNetCore.Documentation
{
	public class DocumentationMiddleware
	{
		const string CONTENT_MARK = "{content}";
		const string DEFAULT_LAYOUT = @"<!DOCTYPE html>
<html lang=""en"" dir=""ltr"">
<head>
  <meta charset=""UTF-8"" />
  <title>Rest API Documentation</title>
</head>
<body>
" + CONTENT_MARK + @"
</body>
</html>";

		private readonly RequestDelegate _next;
		private readonly ILogger _logger;
		private readonly IHostingEnvironment _hostingEnv;
		private readonly DocumentationOptions _options;
		private readonly IDocumentHandlerResolver _documentHandlerResolver;
		private byte[] _defaultLayoutHead;
		private byte[] _defaultLayoutTail;
		private Dictionary<string, byte[]> _layoutHeadsByLanguage = new Dictionary<string, byte[]>();
		private Dictionary<string, byte[]> _layoutTailsByLanguage = new Dictionary<string, byte[]>();
		private Regex _langPatternRegex = new Regex(@"^(\/([a-zA-Z][a-zA-Z]))?(\/(.*))?$", RegexOptions.IgnoreCase);
		private IFileInfo _notFoundHtmlFile;
		private Dictionary<string, IFileInfo> _notFoundHtmlFilesPerLanguage = new Dictionary<string, IFileInfo>();

		public DocumentationMiddleware(
			[NotNull] RequestDelegate next,
			[NotNull] ILogger<DocumentationMiddleware> logger,
			[NotNull] IHostingEnvironment hostingEnv,
			[NotNull] IDocumentHandlerResolver documentHandlerResolver,
			[NotNull] DocumentationOptions options)
		{
			options.ResolveFileProvider(hostingEnv);

			_next = next;
			_logger = logger;
			_hostingEnv = hostingEnv;
			_options = options;
			_documentHandlerResolver = documentHandlerResolver;

			ConfigureNotFoundFiles();
			ParseLayout();
		}

		private void ParseLayout()
		{
			var layoutFile = _options.LayoutFilePath == null && _options.LayoutFile != null ?
				_options.LayoutFile : _options.FileProvider.GetFileInfo(_options.LayoutFilePath);

			if(layoutFile.Exists)
			{
				ParseLayoutFile(layoutFile, out _defaultLayoutHead, out _defaultLayoutTail);

				if (_options.LayoutFilePath != null)
				{
					var layoutFilesPerLanguage = GetFileInfoByLanguage(_options.LayoutFilePath);

					foreach (var fileByLang in layoutFilesPerLanguage)
					{
						ParseLayoutFile(fileByLang.Value, out byte[] head, out byte[] tail);

						_layoutHeadsByLanguage.Add(fileByLang.Key, head);
						_layoutTailsByLanguage.Add(fileByLang.Key, tail);
					}
				}
			}
			else
			{
				GetLayoutHeadAndTail(DEFAULT_LAYOUT, out _defaultLayoutHead, out _defaultLayoutTail);
			}

		}

		private static void ParseLayoutFile(IFileInfo file, out byte[] head, out byte[] tail)
		{
			string content = null;

			using (var stream = file.CreateReadStream())
			using (var reader = new StreamReader(stream))
			{
				content = reader.ReadToEnd();
			}

			GetLayoutHeadAndTail(content, out head, out tail);
		}

		private static void GetLayoutHeadAndTail(string content, out byte[] head, out byte[] tail)
		{
			var contentMarkIndex = content.IndexOf(CONTENT_MARK);
			head = Encoding.UTF8.GetBytes(content.Substring(0, contentMarkIndex));
			tail = Encoding.UTF8.GetBytes(content.Substring(contentMarkIndex + CONTENT_MARK.Length));
		}

		public async Task Invoke(HttpContext context)
		{
			PathString subpath;
			string lang;

			if (context.Request.IsAGetRequest()
				&& TryGetDocumentsSubpathAndLanguage(context.Request, out subpath, out lang))
			{
				bool isDirectory;
				string filePath = GetFilePath(subpath, out isDirectory);
				lang = string.IsNullOrEmpty(lang) ? _options.DefaultLanguage : lang;

				IDocumentHandler handler;
				var foundHandler = (lang != null && _documentHandlerResolver.TryResolveHandler(_options.FileProvider, filePath + $".{lang}", out handler))
					|| _documentHandlerResolver.TryResolveHandler(_options.FileProvider, filePath, out handler);
				if ((foundHandler || _options.DirectoryOptions.EnableDirectoryBrowsing) && isDirectory && !context.Request.Path.EndsInSlash())
				{
					// If the path matches a directory but does not end in a slash, redirect to add the slash.
					// This prevents relative links from breaking.
					context.Response.StatusCode = 301;
					context.Response.Headers["Location"] = context.Request.PathBase + context.Request.Path + "/" + context.Request.QueryString;
					return;
				}

				if (!foundHandler)
				{
					if (isDirectory && _options.DirectoryOptions.EnableDirectoryBrowsing)
					{
						handler = new DirectoryHandler(_options.FileProvider, subpath, _options.DirectoryOptions);
					}
					else if (_options.EnableNotFoundHandling)
					{
						var notFoundFile = lang != null
							&& _notFoundHtmlFilesPerLanguage.ContainsKey(lang)
							? _notFoundHtmlFilesPerLanguage[lang]
							: _notFoundHtmlFile;
						handler = new NotFoundDocumentHandler(notFoundFile);
					}
					else
					{
						return;
					}
				}

				ApplyResponseHeaders(context.Response, handler);
				await ApplyResponseContent(context.Response, handler, lang);

				// Do not continue with the rest of middlewares
				return;
			}

			await _next(context);
		}

		private async Task ApplyResponseContent(HttpResponse response, IDocumentHandler handler, string lang)
		{
			var layoutHead = lang != null
				&& _layoutHeadsByLanguage.ContainsKey(lang)
				? _layoutHeadsByLanguage[lang]
				: _defaultLayoutHead;

			var layoutTail = lang != null
				&& _layoutTailsByLanguage.ContainsKey(lang)
				? _layoutTailsByLanguage[lang]
				: _defaultLayoutTail;

			using (var content = handler.Open())
			{
				response.ContentLength = layoutHead.Length + content.Length + layoutTail.Length;
				await response.Body.WriteAsync(layoutHead, 0, layoutHead.Length);
				await content.ContentStream.CopyToAsync(response.Body);
				await response.Body.WriteAsync(layoutTail, 0, layoutTail.Length);
			}
		}

		private void ApplyResponseHeaders(HttpResponse response, IDocumentHandler handler)
		{
			response.ContentType = "text/html";
			response.StatusCode = handler.StatusCode;
			var headers = response.GetTypedHeaders();
			headers.LastModified = handler.LastModified;
			headers.CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
			{
				MaxAge = _options.CacheMaxAge,
				Public = true
			};
		}

		private string GetFilePath(string subpath, out bool isDirectory)
		{
			isDirectory = _options.EnableDefaultFiles && _options.FileProvider.GetDirectoryContents(subpath).Exists;
			return isDirectory ? Helpers.ConcatPathSegments(subpath, _options.DefaultFileName)
				: subpath;
		}

		private bool TryGetDocumentsSubpathAndLanguage(HttpRequest request, out PathString subpath, out string lang)
		{
			lang = null;

			if (!request.Path.StartsWithSegments(_options.RequestPath, out subpath))
			{
				return false;
			}

			var match = _langPatternRegex.Match(subpath);

			lang = match.Groups[2].Value;
			subpath = _options.FileProviderSubPath.Add(match.Groups[3].Value);

			return true;
		}

		private void ConfigureNotFoundFiles()
		{
			_notFoundHtmlFile = _options.NotFoundHtmlPath == null && _options.NotFoundHtmlFile != null ?
				_options.NotFoundHtmlFile : _options.FileProvider.GetFileInfo(_options.NotFoundHtmlPath);

			if (_options.NotFoundHtmlPath != null && _notFoundHtmlFile.Exists)
			{
				_notFoundHtmlFilesPerLanguage = GetFileInfoByLanguage(_options.NotFoundHtmlPath);
			}
		}

		private Dictionary<string, IFileInfo> GetFileInfoByLanguage(string path)
		{
			var fileInfoByLang = new Dictionary<string, IFileInfo>();

			foreach (var lang in _options.SupportedLanguages)
			{
				var pathWithoutExt = path.Substring(0, path.LastIndexOf('.'));
				var ext = path.Substring(path.LastIndexOf('.'));

				var fileInfo = _options.FileProvider.GetFileInfo(pathWithoutExt + $".{lang}" + ext);

				if (fileInfo.Exists)
				{
					fileInfoByLang.Add(lang, fileInfo);
				}
			}

			return fileInfoByLang;
		}

	}
}

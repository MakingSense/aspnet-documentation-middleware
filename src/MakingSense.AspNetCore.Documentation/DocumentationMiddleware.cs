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
		private string _layoutHead;
		private string _layoutTail;

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
			ParseLayout();
		}

		private void ParseLayout()
		{
			string layoutContent = null;
			if (_options.LayoutFile != null && _options.LayoutFile.Exists)
			{
				using (var stream = _options.LayoutFile.CreateReadStream())
				using (var reader = new StreamReader(stream))
				{
					layoutContent = reader.ReadToEnd();
				}
			}
			else
			{
				layoutContent = DEFAULT_LAYOUT;
			}
			var contentMarkIndex = layoutContent.IndexOf(CONTENT_MARK);
			_layoutHead = layoutContent.Substring(0, contentMarkIndex);
			_layoutTail = layoutContent.Substring(contentMarkIndex + CONTENT_MARK.Length);
		}

		public async Task Invoke(HttpContext context)
		{
			PathString subpath;
			if (context.Request.IsAGetRequest()
				&& TryGetDocumentsSubpath(context.Request, out subpath))
			{
				bool isDirectory;
				string filePath = GetFilePath(subpath, out isDirectory);

				IDocumentHandler handler;
				var foundHandler = _documentHandlerResolver.TryResolveHandler(_options.FileProvider, filePath, out handler);
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
						handler = new NotFoundDocumentHandler(_options.NotFoundHtmlFile);
					}
					else
					{
						return;
					}
				}

				ApplyResponseHeaders(context.Response, handler);
				await ApplyResponseContent(context.Response, handler);

				// Do not continue with the rest of middlewares
				return;
			}

			await _next(context);
		}

		private async Task ApplyResponseContent(HttpResponse response, IDocumentHandler handler)
		{
			//TODO: update response.ContentLength
			//https://github.com/aspnet/vsweb-docs/blob/aa6db2a87ed91214509382684d0a093945539dc8/src/app_code/PageSystem.cs#L73

			using (var content = handler.Open())
			{
				response.ContentLength = _layoutHead.Length + content.Length + _layoutTail.Length;
				await response.Body.WriteAsync(_layoutHead);
				await content.ContentStream.CopyToAsync(response.Body);
				await response.Body.WriteAsync(_layoutTail);
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

		private bool TryGetDocumentsSubpath(HttpRequest request, out PathString subpath)
		{
			if (!request.Path.StartsWithSegments(_options.RequestPath, out subpath))
			{
				return false;
			}

			subpath = _options.FileProviderSubPath.Add(subpath);

			return true;
		}

	}
}

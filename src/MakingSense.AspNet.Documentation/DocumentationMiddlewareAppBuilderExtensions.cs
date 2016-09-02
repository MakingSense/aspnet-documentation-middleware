using System;
using MakingSense.AspNet.Documentation;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNetCore.Builder
{
	public static class DocumentationMiddlewareAppBuilderExtensions
	{
		public static IApplicationBuilder UseDocumentation([NotNull] this IApplicationBuilder app)
		{
			return app.UseDocumentation(new DocumentationOptions());
		}

		public static IApplicationBuilder UseDocumentation([NotNull] this IApplicationBuilder app, [NotNull] DocumentationOptions options)
		{
			return app.UseDocumentation(new DefaultDocumentHandlerResolver(), options);
		}

		public static IApplicationBuilder UseDocumentation([NotNull] this IApplicationBuilder app, [NotNull] IDocumentHandlerResolver documentHandlerResolver, [NotNull] DocumentationOptions options)
		{
			return app.UseMiddleware<DocumentationMiddleware>(documentHandlerResolver, options);
		}
	}
}

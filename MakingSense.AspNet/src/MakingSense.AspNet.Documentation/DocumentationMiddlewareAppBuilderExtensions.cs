using System;
using Microsoft.Framework.Internal;
using MakingSense.AspNet.Documentation;

namespace Microsoft.AspNet.Builder
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

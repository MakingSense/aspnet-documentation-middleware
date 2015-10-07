using Microsoft.AspNet.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MakingSense.AspNet.Documentation
{
	internal static class Helpers
	{
		internal static bool IsAGetRequest(this HttpRequest request)
		{
			return "GET".Equals(request.Method, StringComparison.OrdinalIgnoreCase);
		}

		internal static string ConcatPathSegments(string left, string right)
		{
			return left.TrimEnd('/') + "/" + right.TrimStart('/');
		}

		internal static bool EndsInSlash(this PathString path)
		{
			return path.Value.EndsWith("/", StringComparison.Ordinal);
		}

		internal static async Task WriteAsync(this Stream stream, string value)
		{
			using (var writer = new StreamWriter(stream))
			{
				await writer.WriteAsync(value);
			}
		}
	}
}

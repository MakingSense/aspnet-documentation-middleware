# Documentation Middleware

This package allows to capture GET requests and if the route matches with an existing markdown or HTML file, renders it inside a layout.

## Usage 

It is useful to use it along with StaticFiles in order to allow to also render images and other files.

Example:

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();
    }

    public void Configure(IApplicationBuilder app, IApplicationEnvironment appEnv)
    {
        app.UseMvc();

        app.UseStaticFiles();

        var documentationFilesProvider = new PhysicalFileProvider(appEnv.ApplicationBasePath);
        app.UseDocumentation(new DocumentationOptions()
        {
            DefaultFileName = "index",
            RequestPath = "/docs",
            NotFoundHtmlFile = documentationFilesProvider.GetFileInfo("DocumentationTemplates\\NotFound.html"),
            LayoutFile = documentationFilesProvider.GetFileInfo("DocumentationTemplates\\Layout.html")
        });
    }
}
```

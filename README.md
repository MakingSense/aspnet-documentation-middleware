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

## Versioning and publishing

`project.json`:

```javascript
{
  "version": "0.1.0-beta7-*",
```

For a version release, `version` property in `project.json` should NOT contain `-*` because our `appveyor.yml` configuration file, uses it to inject build number. See <https://ci.appveyor.com/project/andresmoschini/makingsense-aspnet-documentation>. It requires to update both `project.json and `appvoyer.yml`.

**Examples**: `1.0.5-beta7` should be considered the _definitive_ `1.0.5` version for `beta7` framework, `1.0.5-beta7-237` is a _pre-release_ of `1.0.5-beta7`. If the library is upgraded for framework `beta8`, the new version will be `1.0.5-beta8`, when it is upgraded to final DNX version, the new version will be something like `1.0.5` and pre-releases versions something like `1.0.5-785`.

Before merge any PR, `version` property in `project.json` should be updated using [Semantic Versioning](http://semver.org/).

I wish to use [GitVersion](http://gitversion.readthedocs.org/en/stable/) but it seems that it is [not enough mature for DNX projects](https://github.com/GitTools/GitVersion/issues/647).

Maybe we could upload only _definitive_ versions to _NuGet Gallery_ and all versions including _definitive_ and _pre-release_ ones to _MyGet_.

## Multilanguage support

Too add multilanguage support, append two digits language code to the file name before the extension.

Set DefaultLanguage property to add a language fallback for URLs with no language code specified.

**Example:**

```csharp
    app.UseDocumentation(new DocumentationOptions()
    {
        RequestPath = "/docs",
        DefaultLanguage = "en",
        ...
```

```
    /docs/es/filename   =>  filename.es    (and if not exist)  =>  filename
    /docs/en/filename   =>  filename.en    (and if not exist)  =>  filename
    /docs/filename      =>  filename.en    (and if not exist)  =>  filename
```
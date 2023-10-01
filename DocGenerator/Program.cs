using Markdig;
using Markdig.Syntax;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using System;
using System.Text;
using System.Net;
using System.Diagnostics;
using Timer = System.Timers.Timer;

var pipeline = new MarkdownPipelineBuilder()
    .UseAutoIdentifiers()
    .UseAutoLinks()
    .UseEmojiAndSmiley()
    .UseEmphasisExtras()
    .UseGenericAttributes()
    .UseGridTables()
    .UseListExtras()
    .UseSmartyPants()
    .UseTaskLists()
    .Use<DocsExtension>()
    .Build();

var docsPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), $"../../../../docs/"));
var pagesPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), $"../../../../pages/"));

var exporting = false;

void Export()
{
    exporting = true;

    if (Directory.Exists(docsPath))
    {
        Directory.Delete(docsPath, true);
        Console.WriteLine("Regenerating docs...");
    }
    Directory.CreateDirectory(docsPath);

    var rootCategory = new Category()
    {
        Title = string.Empty,
        FilePath = pagesPath,
        WebPath = "./",
    };
    var categories = new List<Category>() { rootCategory };

    var categoryPaths = Directory.GetDirectories(pagesPath);
    foreach (var catPath in categoryPaths)
    {
        categories.Add(new Category()
        {
            Title = new DirectoryInfo(catPath).Name,
            FilePath = catPath,
            WebPath = Path.GetRelativePath(pagesPath, catPath),
        });
    }

    var layoutHtml = string.Empty;

    foreach (var category in categories)
    {
        var pagePaths = Directory.GetFiles(category.FilePath);
        foreach (var pagePath in pagePaths)
        {
            try
            {
                var relativePath = Path.GetRelativePath(pagesPath, pagePath);
                if (pagePath.EndsWith(".md"))
                {
                    var pathContents = File.ReadAllText(pagePath);
                    var markdown = Markdown.Parse(pathContents, pipeline);

                    var pageTitle = Path.GetFileNameWithoutExtension(pagePath);

                    if (markdown.FirstOrDefault(b => b is HeadingBlock) is HeadingBlock heading)
                    {
                        var headingStr = pathContents.Substring(heading.Span.Start, heading.Span.Length);
                        while (headingStr.StartsWith('#')) headingStr = headingStr.Substring(1);
                        while (headingStr.StartsWith(' ')) headingStr = headingStr.Substring(1);
                        pageTitle = headingStr;
                    }

                    var pageHtml = markdown.ToHtml(pipeline);

                    var isIndex = pagePath.EndsWith("index.md");

                    var page = new Page
                    {
                        Title = pageTitle,
                        FilePath = pagePath,
                        WebPath = relativePath.Replace('\\', '/').Replace(".md", ".html"),
                        Html = pageHtml,
                        IsIndex = isIndex,
                    };
                    if (isIndex)
                    {
                        category.Pages.Insert(0, page);
                        category.Title = pageTitle;
                    }
                    else
                    {
                        category.Pages.Add(page);
                    }
                }
                else if (pagePath.EndsWith("_layout.html"))
                {
                    layoutHtml = File.ReadAllText(pagePath);
                }
                else
                {
                    var outputPath = Path.GetFullPath(Path.Combine(docsPath, relativePath));
                    Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? string.Empty);
                    File.Copy(pagePath, outputPath, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        category.Pages.Sort((a, b) => a.IsIndex != b.IsIndex ? -a.IsIndex.CompareTo(b.IsIndex) : a.Title.CompareTo(b.Title));
    }

    var navHtml = $"<ul>" +
        $"{string.Join("", rootCategory.Pages.Select(p => $"<li><a href=\"/{p.WebPath}\">{(p.IsIndex ? "<img src=\"/logo.png\" />" : string.Empty)}<span>{p.Title}</span></a></li>"))}" +
        $"{string.Join("", categories.Where(c => c != rootCategory).Select(c => $"<li><a href=\"/{c.WebPath}/\">{c.Title}</a><ul>" +
        $"{string.Join("", c.Pages.Select(p => p.IsIndex ? string.Empty : $"<li><a href=\"/{p.WebPath}\">{p.Title}</a></li>"))}" +
        $"</ul></li>"))}" +
        $"</ul>";

    foreach (var category in categories)
    {
        foreach (var page in category.Pages)
        {
            var title = rootCategory.Title;
            if (category != rootCategory || !page.IsIndex)
                title = $"{page.Title} - {title}";

            var mainHtml = page.Html;

            var siblingPages = category.Pages.Where(p => p != page && !p.IsIndex);

            if (category == rootCategory && page.IsIndex)
            {
                mainHtml += $"<h3>All Pages</h3><ul>{string.Join("", categories.Select(c => $"<li>{c.Title}<ul>{string.Join("", c.Pages.Where(p => p != page && !p.IsIndex).Select(p => $"<li><a href=\"/{p.WebPath}\">{p.Title}</a></li>"))}</ul></li>"))}</ul>";
            }
            else if (page.IsIndex && siblingPages.Any())
            {
                mainHtml += $"<ul>{string.Join("", siblingPages.Select(p => $"<li><a href=\"/{p.WebPath}\">{p.Title}</a></li>"))}</ul>";
            }
            else if (siblingPages.Any())
            {
                mainHtml += $"<h3>Related Pages</h3><ul>{string.Join("", siblingPages.Select(p => $"<li><a href=\"/{p.WebPath}\">{p.Title}</a></li>"))}</ul>";
            }

            var html = layoutHtml
                .Replace("<main>", $"<main>{mainHtml}")
                .Replace("<nav>", $"<nav>{navHtml}")
                .Replace("<title>", $"<title>{title}");

            var outputPath = Path.GetFullPath(Path.Combine(docsPath, page.WebPath));
            outputPath = Path.ChangeExtension(outputPath, ".html");
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? string.Empty);
            File.WriteAllText(outputPath, html);
        }
    }

    exporting = false;
}

Export();

var dirty = false;
var exportTime = DateTime.Now;

object lockObj = new();

var timer = new Timer(500);
timer.Elapsed += (s, e) =>
{
    if (dirty && !exporting)
    {
        lock (lockObj)
        {
            dirty = false;
            Export();
            exportTime = DateTime.Now;
        }
    }
};
timer.Start();

void QueueExport() => dirty = true;

var watcher = new FileSystemWatcher(pagesPath)
{
    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.DirectoryName,
    IncludeSubdirectories = true,
    EnableRaisingEvents = true
};

watcher.Created += (s, e) => QueueExport();
watcher.Changed += (s, e) => QueueExport();
watcher.Renamed += (s, e) => QueueExport();
watcher.Deleted += (s, e) => QueueExport();
watcher.Error += (s, e) => Console.WriteLine(e.GetException());

const int PORT = 32553;

var canceled = false;

var listener = new HttpListener();
listener.Prefixes.Add($"http://localhost:{PORT}/");
listener.Start();

void Receive()
{
    listener.BeginGetContext(new AsyncCallback((result) =>
    {
        if (!listener.IsListening) return;
        var context = listener.EndGetContext(result);
        var req = context.Request;
        var res = context.Response;

        Console.WriteLine($"{req.HttpMethod} {req.Url?.LocalPath}");

        if (req.Url?.LocalPath == "/hotreload")
        {
            var receivedTime = DateTime.Now;
            res.KeepAlive = true;
            res.StatusCode = (int)HttpStatusCode.Accepted;
            res.OutputStream.Flush();

            Receive();

            while (exportTime < receivedTime)
            {
                Thread.Sleep(100);
            }
            res.OutputStream.Close();
        }
        else
        {
            var filePath = Path.Combine(docsPath, $".{req.Url?.LocalPath}") ?? string.Empty;
            if (filePath.EndsWith("/")) filePath += "index.html";

            lock (lockObj)
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        res.StatusCode = (int)HttpStatusCode.OK;
                        res.ContentType = Path.GetExtension(filePath).ToLower() switch
                        {
                            ".avi" => "video/x-msvideo",
                            ".bin" => "application/octet-stream",
                            ".bmp" => "image/bmp",
                            ".css" => "text/css",
                            ".gz" => "application/gzip",
                            ".gif" => "image/gif",
                            ".htm" => "text/html",
                            ".html" => "text/html",
                            ".jpeg" => "image/jpeg",
                            ".jpg" => "image/jpeg",
                            ".js" => "text/javascript",
                            ".json" => "application/json",
                            ".mjs" => "application/json",
                            ".mp3" => "audio/mp3",
                            ".mp4" => "video/mp4",
                            ".mpeg" => "video/mpeg",
                            ".ogg" => "audio/ogg",
                            ".otf" => "font/otf",
                            ".png" => "image/png",
                            ".svg" => "image/svg+xml",
                            ".ttf" => "font/ttf",
                            ".txt" => "text/plain",
                            ".wav" => "audio/wav",
                            ".weba" => "audio/webm",
                            ".webp" => "audio/webp",
                            ".woff" => "font/woff",
                            ".woff2" => "font/woff2",
                            ".xhtml" => "application/xhtml+xml",
                            ".zip" => "application/zip",
                            ".7z" => "application/x-7z-compressed",
                            _ => "application/octet-stream",
                        };
                        var fileStream = new FileStream(filePath, FileMode.Open);
                        res.ContentLength64 = fileStream.Length;
                        res.AddHeader("Date", DateTime.Now.ToString("R"));
                        res.AddHeader("Last-Modified", File.GetLastWriteTime(filePath).ToString("R"));
                        res.AddHeader("Cache-Control", "no-store");
                        fileStream.CopyTo(res.OutputStream);
                        fileStream.Close();
                        res.OutputStream.Flush();
                    }
                    else
                    {
                        res.StatusCode = (int)HttpStatusCode.NotFound;
                        res.ContentType = "text/plain";
                        res.AddHeader("Cache-Control", "no-store");
                        res.OutputStream.Write(Encoding.UTF8.GetBytes($"Not Found: {req.Url?.LocalPath}"), 0, 0);
                    }
                }
                catch (Exception ex)
                {
                    res.StatusCode = (int)HttpStatusCode.InternalServerError;
                    res.ContentType = "text/plain";
                    res.AddHeader("Cache-Control", "no-store");
                    res.OutputStream.Write(Encoding.UTF8.GetBytes($"Internal Server Error: {ex}"), 0, 0);
                }
            }

            res.OutputStream.Close();
        }

        Receive();
    }), listener);
}

Console.CancelKeyPress += Console_CancelKeyPress;

void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
{
    e.Cancel = true;
    canceled = true;
}

Receive();
Process.Start(new ProcessStartInfo { FileName = $"http://localhost:{PORT}/", UseShellExecute = true });

while (!canceled) { }

listener.Stop();

class Category
{
    public string Title = string.Empty;
    public string WebPath = string.Empty;
    public string FilePath = string.Empty;
    public List<Page> Pages = new();
}

class Page
{
    public string Title = string.Empty;
    public string WebPath = string.Empty;
    public string FilePath = string.Empty;
    public string Html = string.Empty;
    public bool IsIndex = false;
}

class DocsExtension : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {

    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is HtmlRenderer r && !r.ObjectRenderers.Contains<HeadingLinkRenderer>())
        {
            renderer.ObjectRenderers.InsertBefore<HeadingRenderer>(new HeadingLinkRenderer());
        }
    }
}

class HeadingLinkRenderer : IMarkdownObjectRenderer
{
    public bool Accept(RendererBase renderer, Type objectType)
    {
        return objectType == typeof(HeadingBlock);
    }

    public void Write(RendererBase renderer, MarkdownObject objectToRender)
    {
        if (objectToRender is HeadingBlock heading)
        {
            var id = heading.GetAttributes().Id;
            if (renderer is HtmlRenderer r && r.EnableHtmlForBlock)
            {
                r.Write($"<h{heading.Level}");
                r.WriteAttributes(heading);
                r.Write($"><a href=\"#{id}\">");
                r.WriteLeafInline(heading);
                r.Write($"</a></h{heading.Level}>");
                r.EnsureLine();
            }
        }
    }
}

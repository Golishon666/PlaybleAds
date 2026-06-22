using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class PlayableBuildTools
{
    private const string ScenePath = "Assets/Scenes/PlayableAdsShort.unity";
    private const string BuildPath = "Build/WebGL";
    private const string DistPath = "Dist";
    private const string SingleHtmlPath = "Dist/PlayableAdsShort.html";
    private const string ReportPath = "Build/Reports";

    [MenuItem("Playable/Build WebGL With Analyze")]
    public static void BuildWebGLWithAnalyze()
    {
        BuildWebGL();
    }

    [MenuItem("Playable/Build Single HTML")]
    public static void BuildSingleHtml()
    {
        BuildWebGL();
        PackSingleHtml();
        WriteSingleFileAnalyze();
        UnityEngine.Debug.Log($"Single HTML build written to {SingleHtmlPath}");
    }

    [MenuItem("Playable/Pack Current WebGL To Single HTML")]
    public static void PackCurrentWebGLToSingleHtml()
    {
        PackSingleHtml();
        WriteSingleFileAnalyze();
        UnityEngine.Debug.Log($"Single HTML build written to {SingleHtmlPath}");
    }

    private static BuildReport BuildWebGL()
    {
        Directory.CreateDirectory(BuildPath);
        Directory.CreateDirectory(ReportPath);

        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

        var previousCompression = PlayerSettings.WebGL.compressionFormat;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;

        try
        {
            var stopwatch = Stopwatch.StartNew();
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = BuildPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.DetailedBuildReport
            });

            stopwatch.Stop();
            WriteAnalyzeReport(report, stopwatch.Elapsed);

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException($"WebGL build failed: {report.summary.result}");
            }

            return report;
        }
        finally
        {
            PlayerSettings.WebGL.compressionFormat = previousCompression;
        }
    }

    private static void PackSingleHtml()
    {
        Directory.CreateDirectory(DistPath);

        var loader = FindOne(BuildPath, "*.loader.js");
        var data = FindOne(BuildPath, "*.data");
        var framework = FindOne(BuildPath, "*.framework.js");
        var wasm = FindOne(BuildPath, "*.wasm");

        var html = new StringBuilder();
        html.AppendLine("<!doctype html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("  <meta charset=\"utf-8\">");
        html.AppendLine("  <meta name=\"viewport\" content=\"width=device-width,initial-scale=1,viewport-fit=cover,user-scalable=no\">");
        html.AppendLine("  <title>Playable Ads Short</title>");
        html.AppendLine("  <style>");
        html.AppendLine("    html,body,#unity-container{width:100%;height:100%;margin:0;overflow:hidden;background:#10281f}");
        html.AppendLine("    #unity-canvas{width:100%;height:100%;display:block;outline:0}");
        html.AppendLine("    #progress{position:fixed;left:15%;right:15%;bottom:8%;height:8px;background:#163f32;border-radius:4px}");
        html.AppendLine("    #bar{width:0;height:100%;background:#ffe13d;border-radius:4px;transition:width .12s linear}");
        html.AppendLine("  </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine("  <div id=\"unity-container\"><canvas id=\"unity-canvas\" tabindex=\"-1\"></canvas></div>");
        html.AppendLine("  <div id=\"progress\"><div id=\"bar\"></div></div>");
        html.AppendLine("  <script>");
        html.AppendLine("    const payloads = {");
        html.AppendLine($"      loader: \"{EncodeGzipBase64(loader)}\",");
        html.AppendLine($"      data: \"{EncodeGzipBase64(data)}\",");
        html.AppendLine($"      framework: \"{EncodeGzipBase64(framework)}\",");
        html.AppendLine($"      wasm: \"{EncodeGzipBase64(wasm)}\"");
        html.AppendLine("    };");
        html.AppendLine("    async function blobUrl(base64, type) {");
        html.AppendLine("      if (!(\"DecompressionStream\" in self)) {");
        html.AppendLine("        throw new Error(\"This browser does not support gzip decompression for the single-file build.\");");
        html.AppendLine("      }");
        html.AppendLine("      const binary = atob(base64);");
        html.AppendLine("      const chunks = [];");
        html.AppendLine("      for (let offset = 0; offset < binary.length; offset += 1048576) {");
        html.AppendLine("        const slice = binary.slice(offset, offset + 1048576);");
        html.AppendLine("        const bytes = new Uint8Array(slice.length);");
        html.AppendLine("        for (let i = 0; i < slice.length; i++) bytes[i] = slice.charCodeAt(i);");
        html.AppendLine("        chunks.push(bytes);");
        html.AppendLine("      }");
        html.AppendLine("      const stream = new Blob(chunks, {type: \"application/gzip\"}).stream().pipeThrough(new DecompressionStream(\"gzip\"));");
        html.AppendLine("      const buffer = await new Response(stream).arrayBuffer();");
        html.AppendLine("      return URL.createObjectURL(new Blob([buffer], {type}));");
        html.AppendLine("    }");
        html.AppendLine("    (async () => {");
        html.AppendLine("      const urls = {");
        html.AppendLine("        loader: await blobUrl(payloads.loader, \"text/javascript\"),");
        html.AppendLine("        data: await blobUrl(payloads.data, \"application/octet-stream\"),");
        html.AppendLine("        framework: await blobUrl(payloads.framework, \"text/javascript\"),");
        html.AppendLine("        wasm: await blobUrl(payloads.wasm, \"application/wasm\")");
        html.AppendLine("      };");
        html.AppendLine("      const script = document.createElement(\"script\");");
        html.AppendLine("      script.src = urls.loader;");
        html.AppendLine("      script.onload = () => createUnityInstance(document.querySelector(\"#unity-canvas\"), {");
        html.AppendLine("        dataUrl: urls.data,");
        html.AppendLine("        frameworkUrl: urls.framework,");
        html.AppendLine("        codeUrl: urls.wasm,");
        html.AppendLine("        companyName: \"Playable Ads Short\",");
        html.AppendLine("        productName: \"Playable Ads Short\",");
        html.AppendLine("        productVersion: \"1.0\"");
        html.AppendLine("      }, progress => {");
        html.AppendLine("        document.querySelector(\"#bar\").style.width = `${progress * 100}%`;");
        html.AppendLine("      }).then(() => document.querySelector(\"#progress\").remove())");
        html.AppendLine("        .catch(error => alert(error));");
        html.AppendLine("      document.body.appendChild(script);");
        html.AppendLine("    })().catch(error => alert(error));");
        html.AppendLine("  </script>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        File.WriteAllText(SingleHtmlPath, html.ToString(), Encoding.UTF8);
    }

    private static void WriteSingleFileAnalyze()
    {
        Directory.CreateDirectory(ReportPath);

        var html = new FileInfo(SingleHtmlPath);
        if (!html.Exists)
        {
            throw new FileNotFoundException("Single HTML file was not created.", SingleHtmlPath);
        }

        var files = Directory.GetFiles(BuildPath, "*", SearchOption.AllDirectories)
            .Select(path => new FileInfo(path))
            .OrderByDescending(file => file.Length)
            .ToArray();

        var builder = new StringBuilder();
        builder.AppendLine("Playable Ads Short - Single File Analyze");
        builder.AppendLine($"GeneratedLocal: {DateTime.Now:O}");
        builder.AppendLine($"Output: {html.FullName}");
        builder.AppendLine($"SingleFileBytes: {html.Length.ToString(CultureInfo.InvariantCulture)}");
        builder.AppendLine($"SingleFileMiB: {BytesToMiB((ulong)html.Length):F2}");
        builder.AppendLine();
        builder.AppendLine("Original WebGL files:");

        foreach (var file in files)
        {
            builder.AppendLine($"{BytesToMiB((ulong)file.Length),8:F2} MiB  {file.FullName}");
        }

        File.WriteAllText(Path.Combine(ReportPath, "single-file-analyze.txt"), builder.ToString(), Encoding.UTF8);
    }

    private static void WriteAnalyzeReport(BuildReport report, TimeSpan wallTime)
    {
        var summary = report.summary;
        var files = report.GetFiles()
            .OrderByDescending(file => file.size)
            .ToArray();

        var builder = new StringBuilder();
        builder.AppendLine("Playable Ads Short - WebGL Build Analyze");
        builder.AppendLine($"GeneratedUtc: {DateTime.UtcNow:O}");
        builder.AppendLine($"Result: {summary.result}");
        builder.AppendLine($"Platform: {summary.platform}");
        builder.AppendLine($"OutputPath: {summary.outputPath}");
        builder.AppendLine($"UnityBuildTime: {summary.totalTime}");
        builder.AppendLine($"CommandWallTime: {wallTime}");
        builder.AppendLine($"TotalSizeBytes: {summary.totalSize.ToString(CultureInfo.InvariantCulture)}");
        builder.AppendLine($"TotalSizeMiB: {BytesToMiB(summary.totalSize):F2}");
        builder.AppendLine($"Scenes: {ScenePath}");
        builder.AppendLine();
        builder.AppendLine("Largest build files:");

        foreach (var file in files.Take(25))
        {
            builder.AppendLine($"{BytesToMiB(file.size),8:F2} MiB  {file.role,-14}  {file.path}");
        }

        var reportFile = Path.Combine(ReportPath, "webgl-build-analyze.txt");
        File.WriteAllText(reportFile, builder.ToString(), Encoding.UTF8);

        var csv = new StringBuilder();
        csv.AppendLine("size_bytes,size_mib,role,path");
        foreach (var file in files)
        {
            csv.AppendLine($"{file.size},{BytesToMiB(file.size):F2},{EscapeCsv(file.role.ToString())},{EscapeCsv(file.path)}");
        }

        File.WriteAllText(Path.Combine(ReportPath, "webgl-build-files.csv"), csv.ToString(), Encoding.UTF8);
        UnityEngine.Debug.Log($"Build analyze written to {reportFile}");
    }

    private static double BytesToMiB(ulong bytes)
    {
        return bytes / 1024d / 1024d;
    }

    private static string FindOne(string directory, string pattern)
    {
        var matches = Directory.GetFiles(directory, pattern, SearchOption.AllDirectories);
        if (matches.Length != 1)
        {
            throw new InvalidOperationException($"Expected one {pattern}, found {matches.Length}");
        }

        return matches[0];
    }

    private static string EncodeGzipBase64(string path)
    {
        byte[] source = File.ReadAllBytes(path);
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, System.IO.Compression.CompressionLevel.Optimal, leaveOpen: true))
        {
            gzip.Write(source, 0, source.Length);
        }

        return Convert.ToBase64String(output.ToArray());
    }

    private static string EscapeCsv(string value)
    {
        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}

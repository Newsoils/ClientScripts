using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using CLIP.Framework_Unity.Asset;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Regenerates <c>Assets/Scripts/Proto/Sources/all.json</c> and per-package <c>*.json</c> files
/// using the same message-id rules as <c>export_cmd.py</c> (first pass only; no module/cross).
/// <c>name</c> uses C# namespaces from protobuf output (e.g. <c>Cmd.UserVerifyVerReq</c>).
/// </summary>
public static class ProtoAllJsonGenerator
{
    const string MenuPath = "Tools/Proto/Regenerate all.json from .proto files";

    static readonly Regex PackageRegex = new Regex(@"package[ \t]+(\w+);", RegexOptions.Multiline);
    static readonly Regex SeqRegex = new Regex(@"//SEQ\[+(.*),(.*)\]", RegexOptions.Multiline);
    static readonly Regex MessageRegex = new Regex(
        @"message[ \t]+(\w+Req\b)|message[ \t]+(\w+Res\b)|message[ \t]+(\w+S2C\b)|message[ \t]+(\w+S2S\b)");

    [MenuItem(MenuPath)]
    public static void Regenerate()
    {
        var sourcesRoot = Path.Combine(Application.dataPath, "Scripts", "Proto", "Sources");
        if (!Directory.Exists(sourcesRoot))
        {
            Debug.LogError($"Proto sources folder not found: {sourcesRoot}");
            return;
        }

        var parser = new ProtoParserState();
        var paths = Directory.GetFiles(sourcesRoot, "*.proto", SearchOption.AllDirectories);
        Array.Sort(paths, StringComparer.Ordinal);
        foreach (var path in paths)
        {
            try
            {
                ParseFile(path, parser);
            }
            catch (Exception e)
            {
                Debug.LogError($"ProtoAllJsonGenerator: failed on {path}\n{e}");
            }
        }

        Dump(sourcesRoot, parser);
        PublishAllJsonToResources(sourcesRoot);
        AssetDatabase.Refresh();
        Debug.Log($"ProtoAllJsonGenerator: wrote all.json and package json files under {sourcesRoot}");
    }

    static void Dump(string outDir, ProtoParserState parser)
    {
        foreach (var list in parser.Maps.Values)
            list.Sort((a, b) => a.Id.CompareTo(b.Id));

        WriteAllJson(outDir, parser);
        foreach (var kv in parser.Maps)
        {
            var path = Path.Combine(outDir, kv.Key + ".json");
            var singleNs = new SortedDictionary<string, List<Entry>>(StringComparer.Ordinal);
            singleNs[kv.Key] = new List<Entry>(kv.Value);
            WriteJsonFile(path, singleNs, singleFileArray: true);
        }
    }

    /// <summary>Same convention as typical protoc C# output: <c>cmd</c> → <c>Cmd</c>.</summary>
    internal static string ProtoPackageToCSharpNamespace(string package)
    {
        if (string.IsNullOrEmpty(package))
            return package;
        return char.ToUpperInvariant(package[0]) + package.Substring(1);
    }

    static void ParseFile(string filename, ProtoParserState parser)
    {
        var allText = File.ReadAllText(filename);
        var packageMatch = PackageRegex.Match(allText);
        if (!packageMatch.Success)
            return;
        var package = packageMatch.Groups[1].Value;

        var seqMatch = SeqRegex.Match(allText);
        if (!seqMatch.Success)
            return;
        var minSeq = int.Parse(seqMatch.Groups[1].Value.Trim());
        _ = int.Parse(seqMatch.Groups[2].Value.Trim());

        var csharpNs = ProtoPackageToCSharpNamespace(package);
        var prefix = package + ".";

        var idmaps = new Dictionary<int, EntryDraft>();
        var keymaps = new Dictionary<string, int>();
        var finds = MessageRegex.Matches(allText);
        if (finds.Count == 0)
            return;
        if (string.IsNullOrEmpty(package))
            return;

        foreach (Match m in finds)
        {
            var n = FirstNonEmpty(m, 1, 2, 3, 4);
            if (n == null)
                continue;

            var fullnameProto = prefix + n;
            int crcNum;
            if (fullnameProto.EndsWith("Req", StringComparison.Ordinal))
            {
                var key = fullnameProto.Substring(0, fullnameProto.Length - 3);
                crcNum = minSeq + 1 + minSeq % 2;
                keymaps[key] = crcNum;
            }
            else if (fullnameProto.EndsWith("Res", StringComparison.Ordinal))
            {
                var key = fullnameProto.Substring(0, fullnameProto.Length - 3);
                if (!keymaps.TryGetValue(key, out var baseId))
                {
                    baseId = minSeq + 1 + minSeq % 2;
                    keymaps[key] = baseId;
                }
                crcNum = baseId + 1;
            }
            else
            {
                crcNum = minSeq + minSeq % 2 + 2;
            }

            if (crcNum > minSeq)
                minSeq = crcNum;

            var displayName = csharpNs + "." + n;
            var draft = new EntryDraft { Namespace = package, Id = crcNum, Name = displayName };
            idmaps[crcNum] = draft;
        }

        foreach (var kv in idmaps)
            parser.Reg(kv.Value.Namespace, kv.Value.Id, kv.Value.Name);
    }

    static string FirstNonEmpty(Match m, int g1, int g2, int g3, int g4)
    {
        if (m.Groups[g1].Success && m.Groups[g1].Length > 0)
            return m.Groups[g1].Value;
        if (m.Groups[g2].Success && m.Groups[g2].Length > 0)
            return m.Groups[g2].Value;
        if (m.Groups[g3].Success && m.Groups[g3].Length > 0)
            return m.Groups[g3].Value;
        if (m.Groups[g4].Success && m.Groups[g4].Length > 0)
            return m.Groups[g4].Value;
        return null;
    }

    static void WriteAllJson(string outDir, ProtoParserState parser)
    {
        var path = Path.Combine(outDir, "all.json");
        WriteJsonFile(path, parser.Maps, singleFileArray: false);
    }

    /// <summary>
    /// 将 all.json 拷贝到 Resources 目录（跨平台可靠，Android/iOS 均可用 Resources.Load 读取）。
    /// </summary>
    static void PublishAllJsonToResources(string sourcesRoot)
    {
        var src = Path.Combine(sourcesRoot, "all.json");
        var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
        if (string.IsNullOrEmpty(projectRoot))
        {
            Debug.LogError("ProtoAllJsonGenerator: project root not found.");
            return;
        }

        var dest = Path.Combine(projectRoot, GameAssetsPathDefine.ProtoAllJsonPublishPath);
        var destDir = Path.GetDirectoryName(dest);
        if (!File.Exists(src))
        {
            Debug.LogWarning($"ProtoAllJsonGenerator: skip publish, missing {src}");
            return;
        }

        if (string.IsNullOrEmpty(destDir))
        {
            Debug.LogError($"ProtoAllJsonGenerator: invalid publish path {dest}");
            return;
        }

        Directory.CreateDirectory(destDir);
        File.Copy(src, dest, overwrite: true);
        Debug.Log($"ProtoAllJsonGenerator: published all.json -> {dest}");
    }

    static void WriteJsonFile(string path, SortedDictionary<string, List<Entry>> maps, bool singleFileArray)
    {
        var sb = new StringBuilder(1 << 16);
        if (singleFileArray)
        {
            foreach (var kv in maps)
            {
                AppendArray(sb, kv.Value, root: true);
                break;
            }
        }
        else
        {
            sb.Append("{\n");
            var firstNs = true;
            foreach (var kv in maps)
            {
                if (!firstNs)
                    sb.Append(",\n");
                firstNs = false;
                sb.Append("  ");
                AppendEscapedJsonString(sb, kv.Key);
                sb.Append(": ");
                AppendArray(sb, kv.Value, root: false);
            }
            sb.Append("\n}\n");
        }

        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
    }

    static void AppendArray(StringBuilder sb, List<Entry> entries, bool root)
    {
        sb.Append("[\n");

        for (var i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            sb.Append(root ? "  " : "    ");
            sb.Append("{\n");
            sb.Append(root ? "    " : "      ");
            sb.Append("\"id\": ");
            sb.Append(e.Id);
            sb.Append(",\n");
            sb.Append(root ? "    " : "      ");
            sb.Append("\"name\": ");
            AppendEscapedJsonString(sb, e.Name);
            sb.Append("\n");
            sb.Append(root ? "  " : "    ");
            sb.Append("}");
            if (i < entries.Count - 1)
                sb.Append(",");
            sb.Append("\n");
        }
        if (!root)
            sb.Append("  ]");
        else
            sb.Append("]\n");
    }

    static void AppendEscapedJsonString(StringBuilder sb, string s)
    {
        sb.Append('"');
        foreach (var c in s)
        {
            switch (c)
            {
                case '\\': sb.Append("\\\\"); break;
                case '"': sb.Append("\\\""); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    if (c < 0x20)
                    {
                        sb.Append("\\u");
                        sb.Append(((int)c).ToString("x4"));
                    }
                    else
                    {
                        sb.Append(c);
                    }
                    break;
            }
        }
        sb.Append('"');
    }

    struct EntryDraft
    {
        public string Namespace;
        public int Id;
        public string Name;
    }

    struct Entry
    {
        public int Id;
        public string Name;
    }

    sealed class ProtoParserState
    {
        public readonly SortedDictionary<string, List<Entry>> Maps = new SortedDictionary<string, List<Entry>>(StringComparer.Ordinal);

        readonly HashSet<int> _ids = new HashSet<int>();

        public void Reg(string namespaceKey, int id, string name)
        {
            if (_ids.Contains(id))
                throw new InvalidOperationException($"duplicate id {id}, name={name}");

            if (!Maps.TryGetValue(namespaceKey, out var list))
            {
                list = new List<Entry>();
                Maps[namespaceKey] = list;
            }

            _ids.Add(id);
            list.Add(new Entry { Id = id, Name = name });
        }
    }
}

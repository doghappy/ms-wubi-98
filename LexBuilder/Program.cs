using System.Text;

const int tableOffset = 0xA8;
const string outputPath = "ChsWubiNew.lex";

if (args.Length != 1)
{
    Console.Error.WriteLine("用法：LexBuilder <码表>");
    return 1;
}

var inputPath = Path.GetFullPath(args[0]);
if (!File.Exists(inputPath))
{
    Console.Error.WriteLine($"找不到输入码表：{inputPath}");
    return 1;
}

try
{
    var entries = ReadTable(inputPath);
    WriteLex(outputPath, entries);
    Console.WriteLine($"已转换 {entries.Sum(group => group.Value.Count)} 条词条，输出文件：{outputPath}");
    return 0;
}
catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or FormatException or OverflowException)
{
    Console.Error.WriteLine($"转换失败：{exception.Message}");
    return 1;
}

static SortedDictionary<string, List<string>> ReadTable(string inputPath)
{
    var entries = new SortedDictionary<string, List<string>>(StringComparer.Ordinal);
    using var reader = new StreamReader(inputPath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

    var lineNumber = 0;
    while (reader.ReadLine() is { } line)
    {
        lineNumber++;
        if (string.IsNullOrWhiteSpace(line))
        {
            continue;
        }

        var separator = line.IndexOf('\t');
        if (separator <= 0 || separator == line.Length - 1)
        {
            throw new FormatException($"第 {lineNumber} 行格式错误，应为“词语<TAB>编码”。");
        }

        var word = line[..separator].Trim();
        var code = line[(separator + 1)..].Trim();
        if (word.Length == 0 || code.Length is < 1 or > 4 ||
            code.Any(character => character is < 'a' or > 'z'))
        {
            throw new FormatException($"第 {lineNumber} 行的词语或编码无效。");
        }

        var wordByteLength = Encoding.Unicode.GetByteCount(word);
        if (wordByteLength + 16 > ushort.MaxValue)
        {
            throw new FormatException($"第 {lineNumber} 行的词语过长。");
        }

        if (!entries.TryGetValue(code, out var words))
        {
            words = [];
            entries.Add(code, words);
        }

        words.Add(word);
    }

    return entries;
}

static void WriteLex(string outputPath, SortedDictionary<string, List<string>> entries)
{
    var outputDirectory = Path.GetDirectoryName(outputPath);
    if (!string.IsNullOrEmpty(outputDirectory))
    {
        Directory.CreateDirectory(outputDirectory);
    }

    using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
    using var writer = new BinaryWriter(stream, Encoding.Unicode, leaveOpen: true);

    writer.Write("imscwubi"u8.ToArray());
    writer.Write((ushort)1);
    writer.Write((ushort)1);
    writer.Write(0x40);
    writer.Write(tableOffset);
    writer.Write(0);
    writer.Write(0x78563412);
    writer.Write(new byte[0x24]);

    stream.Position = 0x40;
    for (var i = 0; i < 26; i++)
    {
        writer.Write(0);
    }

    stream.Position = tableOffset;
    var alphaOffsets = new int[26];
    var previousLetter = 'a' - 1;

    foreach (var (code, words) in entries)
    {
        while (previousLetter < code[0])
        {
            previousLetter++;
            alphaOffsets[previousLetter - 'a'] = checked((int)(stream.Position - tableOffset));
        }

        if (words.Count > ushort.MaxValue + 1)
        {
            throw new FormatException($"编码“{code}”对应的词条过多，超出 lex 格式的限制。");
        }

        foreach (var (word, index) in words.Select((word, index) => (word, index)))
        {
            var wordBytes = Encoding.Unicode.GetBytes(word);
            writer.Write(checked((ushort)(16 + wordBytes.Length)));
            writer.Write(checked((ushort)index));
            writer.Write(checked((ushort)code.Length));

            for (var i = 0; i < 4; i++)
            {
                writer.Write(i < code.Length ? code[i] : (ushort)0);
            }

            writer.Write(wordBytes);
            writer.Write((ushort)0);
        }
    }

    while (previousLetter < 'z')
    {
        previousLetter++;
        alphaOffsets[previousLetter - 'a'] = checked((int)(stream.Position - tableOffset));
    }

    var fileSize = checked((int)stream.Position);
    stream.Position = 0x14;
    writer.Write(fileSize);
    stream.Position = 0x40;
    foreach (var offset in alphaOffsets)
    {
        writer.Write(offset);
    }
}

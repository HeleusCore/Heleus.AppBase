using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AppBuilder
{
    static class Images
    {
        static List<ImageOperation> Parse(DirectoryInfo path)
        {
            var result = new List<ImageOperation>();
            var imageLines = File.ReadAllLines(Path.Combine(path.FullName, "images.txt"));
            foreach (var line in imageLines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("//", StringComparison.Ordinal))
                    continue;

                var parts = line.Split(';');
                var fileInfo = parts[0].Trim().Split("=>");
                var parameters = parts[1].Trim().Split(' ');
                var options = (parts.Length >= 3) ? parts[2].Trim().Split(" ") : new string[0];

                var op = parameters[0].Trim().ToLower();
                if (op == ResizeImageOperation.OpName)
                    result.Add(new ResizeImageOperation(parameters.TakeLast(parameters.Length - 1).ToArray(), fileInfo[0], fileInfo[1], options));
                else if (op == BlendImageOperation.OpName)
                    result.Add(new BlendImageOperation(parameters.TakeLast(parameters.Length - 1).ToArray(), fileInfo[0], fileInfo[1], options));
                else if (op == ImageBlendOperation.OpName)
                    result.Add(new ImageBlendOperation(parameters.TakeLast(parameters.Length - 1).ToArray(), fileInfo[0], fileInfo[1], options));
            }

            return result;
        }

        public static void Generate(DirectoryInfo infoPath, DirectoryInfo templatePath, DirectoryInfo targetPath)
        {
            var operations = Parse(templatePath);
            operations.AddRange(Parse(infoPath));

            foreach (var operation in operations)
                operation.Process(infoPath, targetPath);
        }
    }
}

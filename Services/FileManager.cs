using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace AIDevHelper.Services
{
    public class FileManager
    {
        public string ProjectPath { get; private set; } = string.Empty;

        public void SetProjectPath(string path)
        {
            if (Directory.Exists(path))
            {
                ProjectPath = path;
            }
            else
            {
                throw new DirectoryNotFoundException("Folder not found: " + path);
            }
        }

        public void CreateFile(string relativePath, string content)
        {
            string fullPath = Path.Combine(ProjectPath, relativePath);
            string? directory = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(fullPath, content);
        }

        public void DeleteFile(string relativePath)
        {
            string fullPath = Path.Combine(ProjectPath, relativePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        public void DeletePath(string relativePath)
        {
            string fullPath = Path.Combine(ProjectPath, relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
            else if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
            }
        }

        public List<string> ReadGitIgnore()
        {
            List<string> patterns = new List<string>();
            string gitignorePath = Path.Combine(ProjectPath, ".gitignore");
            if (File.Exists(gitignorePath))
            {
                foreach (var line in File.ReadAllLines(gitignorePath))
                {
                    string trimmed = line.Trim();
                    if (!trimmed.StartsWith("#") && trimmed.Length > 0)
                    {
                        patterns.Add(trimmed);
                    }
                }
            }
            return patterns;
        }

        public bool IsIgnored(string fullPath, List<string> patterns)
        {
            if (patterns.Count == 0) return false;
            string relative = Path.GetRelativePath(ProjectPath, fullPath).Replace('\\', '/');
            foreach (var pat in patterns)
            {
                if (relative.Contains(pat))
                    return true;
            }
            return false;
        }
    }
}

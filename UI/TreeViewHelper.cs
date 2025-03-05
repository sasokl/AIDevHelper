using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace AIDevHelper.UI
{
    public static class TreeViewHelper
    {
        // Default folders to ignore regardless of .gitignore
        private static readonly List<string> DefaultIgnores = new List<string> { ".git", "bin", "obj" };

        public static TreeNode BuildDirectoryNode(DirectoryInfo dirInfo, string rootPath, bool applyGitIgnore)
        {
            List<string> ignorePatterns = new List<string>();
            if (applyGitIgnore)
            {
                string gitignorePath = Path.Combine(rootPath, ".gitignore");
                if (File.Exists(gitignorePath))
                {
                    var lines = File.ReadAllLines(gitignorePath);
                    foreach (var line in lines)
                    {
                        string trimmed = line.Trim();
                        if (!string.IsNullOrWhiteSpace(trimmed) && !trimmed.StartsWith("#"))
                            ignorePatterns.Add(trimmed);
                    }
                }
                ignorePatterns.AddRange(DefaultIgnores);
            }

            TreeNode node = new TreeNode(dirInfo.Name) { Tag = dirInfo.FullName };
            foreach (var dir in dirInfo.GetDirectories())
            {
                if (!IsIgnored(rootPath, dir.FullName, ignorePatterns))
                    node.Nodes.Add(BuildDirectoryNode(dir, rootPath, applyGitIgnore));
            }
            foreach (var file in dirInfo.GetFiles())
            {
                if (!IsIgnored(rootPath, file.FullName, ignorePatterns))
                {
                    TreeNode fileNode = new TreeNode(file.Name) { Tag = file.FullName };
                    node.Nodes.Add(fileNode);
                }
            }
            return node;
        }

        public static bool IsIgnored(string rootPath, string fullPath, List<string> patterns)
        {
            if (patterns == null || patterns.Count == 0) return false;
            string relative = Path.GetRelativePath(rootPath, fullPath).Replace('\\', '/');
            foreach (var pat in patterns)
            {
                if (string.IsNullOrWhiteSpace(pat)) continue;
                string trimmedPat = pat.Trim();
                if (trimmedPat.StartsWith("/"))
                    trimmedPat = trimmedPat.Substring(1);
                trimmedPat = trimmedPat.TrimEnd('/');
                if (relative.Equals(trimmedPat, StringComparison.OrdinalIgnoreCase) ||
                    relative.StartsWith(trimmedPat + "/", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public static string BuildPartialStructureText(TreeNodeCollection nodes)
        {
            if (nodes.Count == 0) return "Partial structure: (no files/folders)";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Partial structure (root-level + chosen items):");
            var root = nodes[0];
            sb.AppendLine($"/{root.Text}");
            foreach (TreeNode child in root.Nodes)
            {
                if (child.Checked || ChildHasChecked(child))
                    sb.AppendLine($"  /{child.Text}");
                else
                    sb.AppendLine($"  /{child.Text} -> ...");
            }
            return sb.ToString();
        }

        private static bool ChildHasChecked(TreeNode node)
        {
            if (node.Checked) return true;
            foreach (TreeNode child in node.Nodes)
                if (ChildHasChecked(child)) return true;
            return false;
        }

        public static string BuildFullStructureText(TreeNodeCollection nodes)
        {
            if (nodes.Count == 0) return "Full structure: (no files/folders)";
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Full structure of the project:");
            var root = nodes[0];
            sb.AppendLine($"/{root.Text}");
            BuildFullStructureRecursive(root, 1, sb);
            return sb.ToString();
        }

        private static void BuildFullStructureRecursive(TreeNode node, int level, StringBuilder sb)
        {
            foreach (TreeNode child in node.Nodes)
            {
                sb.AppendLine($"{new string(' ', level * 2)}/{child.Text}");
                if (child.Nodes.Count > 0)
                    BuildFullStructureRecursive(child, level + 1, sb);
            }
        }

        public static void TraverseNodes(TreeNodeCollection nodes, string rootFolder, ref int count, StringBuilder sb)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Checked && node.Tag is string fullPath && File.Exists(fullPath))
                {
                    string relativePath = Path.GetRelativePath(rootFolder, fullPath);
                    string code = File.ReadAllText(fullPath);
                    sb.AppendLine($"{count}) file_path: {relativePath}");
                    sb.AppendLine("code:");
                    sb.AppendLine("```");
                    sb.AppendLine(code);
                    sb.AppendLine("```");
                    sb.AppendLine();
                    count++;
                }
                if (node.Nodes.Count > 0)
                    TraverseNodes(node.Nodes, rootFolder, ref count, sb);
            }
        }

        // New: Calculate stats based solely on visible (TreeView) nodes
        public static (long totalLines, long totalChars) CalculateStatsFromNode(TreeNode node, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            long lines = 0, chars = 0;
            if (node.Tag is string fullPath && File.Exists(fullPath))
            {
                try
                {
                    string content = File.ReadAllText(fullPath);
                    lines = content.Split('\n').Length;
                    chars = content.Length;
                }
                catch { }
            }
            else
            {
                foreach (TreeNode child in node.Nodes)
                {
                    var (childLines, childChars) = CalculateStatsFromNode(child, token);
                    lines += childLines;
                    chars += childChars;
                }
            }
            return (lines, chars);
        }

        // Updated: Use tree-based stats calculation rather than scanning full FS
        public static void UpdateNodeStats(TreeNode node, string rootFolder, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;
            var (lines, chars) = CalculateStatsFromNode(node, token);
            string newText = node.Tag is string fullPath ? $"{Path.GetFileName(fullPath)} ({chars} chars, {lines} lines)" : node.Text;
            SetNodeTextSafe(node, newText);
        }

        public static void UpdateAllNodesStats(TreeNodeCollection nodes, string rootFolder, CancellationToken token)
        {
            foreach (TreeNode node in nodes)
            {
                if (token.IsCancellationRequested)
                    return;
                UpdateNodeStats(node, rootFolder, token);
                if (node.Nodes.Count > 0)
                    UpdateAllNodesStats(node.Nodes, rootFolder, token);
            }
        }

        private static void SetNodeTextSafe(TreeNode node, string text)
        {
            if (node.TreeView != null && node.TreeView.InvokeRequired)
            {
                node.TreeView.Invoke(new Action(() => node.Text = text));
            }
            else
            {
                node.Text = text;
            }
        }
    }
}

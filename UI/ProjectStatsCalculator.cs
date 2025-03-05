using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AIDevHelper.UI
{
    public static class ProjectStatsCalculator
    {
        public static async Task<(int totalLines, int totalChars)> CalculateAsync(TreeNodeCollection nodes, string rootFolder, CancellationToken token)
        {
            return await Task.Run(() =>
            {
                int totalLines = 0, totalChars = 0;
                foreach (TreeNode node in nodes)
                {
                    CalculateNodeStats(node, ref totalLines, ref totalChars, token);
                }
                return (totalLines, totalChars);
            }, token);
        }

        private static void CalculateNodeStats(TreeNode node, ref int totalLines, ref int totalChars, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (node.Checked && node.Tag is string fullPath && File.Exists(fullPath))
            {
                try
                {
                    string content = File.ReadAllText(fullPath);
                    totalLines += content.Split('\n').Length;
                    totalChars += content.Length;
                }
                catch { }
            }
            foreach (TreeNode child in node.Nodes)
            {
                CalculateNodeStats(child, ref totalLines, ref totalChars, token);
            }
        }
    }
}

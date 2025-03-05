using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AIDevHelper.Models;
using AIDevHelper.Services;
using Newtonsoft.Json;
using System.Resources;
using System.Globalization;

namespace AIDevHelper.UI
{
    public class ModifyProjectPanel : UserControl
    {
        private ResourceManager _resourceManager;
        private CultureInfo _currentCulture;

        private TableLayoutPanel mainLayoutPanel = new();
        private Panel leftPanel = new();
        private TableLayoutPanel leftLayout = new();
        private GroupBox grpFolder = new();
        private GroupBox grpOptions = new();
        private GroupBox grpTree = new();
        private TableLayoutPanel rightLayout = new();
        private Panel rightTopPanel = new();
        private TableLayoutPanel rightTopLayout = new();
        private GroupBox grpUserRequest = new();
        private GroupBox grpGenerateRequest = new();
        private Panel rightBottomPanel = new();
        private GroupBox grpAI = new();
        private Label lblFolderPath = new();
        private TextBox txtFolderPath = new();
        private Button btnSelectFolder = new();
        private CheckBox chkGitIgnore = new();
        private CheckBox chkCalculateStats = new();
        private Label lblStatsIndicator = new();
        private Button btnStopStats = new();
        private TreeView treeViewFiles = new();
        private Label lblSelectedStats = new();
        private Label lblUserRequest = new();
        private RichTextBox rtxtUserRequest = new();
        private RadioButton rdoPartialStructure = new();
        private RadioButton rdoFullStructure = new();
        private CheckBox chkFullCode = new();
        private CheckBox chkIncludeExplanation = new();
        private Button btnGenerateAIRequest = new();
        private RichTextBox rtxtGenerateRequest = new();
        private RichTextBox rtxtJsonResponse = new();
        private Button btnApplyChanges = new();
        private string _rootFolder = string.Empty;
        private string _selectionFilePath = string.Empty;
        private CancellationTokenSource? _statsCts;
        private bool isUpdatingCheck = false;
        private System.Windows.Forms.Timer _saveTimer = new System.Windows.Forms.Timer();

        public ModifyProjectPanel(CultureInfo culture)
        {
            _currentCulture = culture;
            _resourceManager = new ResourceManager("AIDevHelper.Resources.Lang", typeof(ModifyProjectPanel).Assembly);
            InitializeComponent();
            ApplyLanguage();
            _saveTimer.Interval = 1000;
            _saveTimer.Tick += (s, e) => { SaveSelections(); _saveTimer.Stop(); };
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            mainLayoutPanel.Dock = DockStyle.Fill;
            mainLayoutPanel.RowCount = 1;
            mainLayoutPanel.ColumnCount = 2;
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            leftPanel.Dock = DockStyle.Fill;
            leftPanel.AutoScroll = true;
            leftLayout.Dock = DockStyle.Fill;
            leftLayout.RowCount = 3;
            leftLayout.ColumnCount = 1;
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 110F));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // grpFolder with responsive layout
            grpFolder.Dock = DockStyle.Fill;
            var folderLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3
            };
            folderLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            folderLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            folderLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            folderLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            folderLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            lblFolderPath.AutoSize = true;
            lblFolderPath.Anchor = AnchorStyles.Left;
            txtFolderPath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtFolderPath.Width = 300;
            btnSelectFolder.AutoSize = true;
            btnSelectFolder.Anchor = AnchorStyles.Right;
            btnSelectFolder.Click += BtnSelectFolder_Click;
            chkGitIgnore.AutoSize = true;
            chkGitIgnore.Anchor = AnchorStyles.Left;
            chkGitIgnore.CheckedChanged += (s, e) => { if (!string.IsNullOrWhiteSpace(txtFolderPath.Text)) LoadTree(); };
            folderLayout.Controls.Add(lblFolderPath, 0, 0);
            folderLayout.SetColumnSpan(lblFolderPath, 2);
            folderLayout.Controls.Add(txtFolderPath, 0, 1);
            folderLayout.Controls.Add(btnSelectFolder, 1, 1);
            folderLayout.Controls.Add(chkGitIgnore, 0, 2);
            folderLayout.SetColumnSpan(chkGitIgnore, 2);
            grpFolder.Controls.Add(folderLayout);

            // grpOptions with responsive layout
            grpOptions.Dock = DockStyle.Fill;
            var optionsLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };
            chkCalculateStats.AutoSize = true;
            chkCalculateStats.CheckedChanged += (s, e) => { LoadTree(); };
            lblStatsIndicator.AutoSize = true;
            btnStopStats.AutoSize = true;
            btnStopStats.Click += BtnStopStats_Click;
            optionsLayout.Controls.Add(chkCalculateStats);
            optionsLayout.Controls.Add(lblStatsIndicator);
            optionsLayout.Controls.Add(btnStopStats);
            grpOptions.Controls.Add(optionsLayout);

            // grpTree
            grpTree.Dock = DockStyle.Fill;
            treeViewFiles.Dock = DockStyle.Fill;
            treeViewFiles.CheckBoxes = true;
            treeViewFiles.AfterCheck += TreeViewFiles_AfterCheck;
            lblSelectedStats.Dock = DockStyle.Bottom;
            lblSelectedStats.AutoSize = true;
            grpTree.Controls.Add(treeViewFiles);
            grpTree.Controls.Add(lblSelectedStats);

            leftLayout.Controls.Add(grpFolder, 0, 0);
            leftLayout.Controls.Add(grpOptions, 0, 1);
            leftLayout.Controls.Add(grpTree, 0, 2);
            leftPanel.Controls.Add(leftLayout);

            // Right panel
            rightLayout.Dock = DockStyle.Fill;
            rightLayout.RowCount = 2;
            rightLayout.ColumnCount = 1;
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 65F));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 35F));
            rightTopPanel.Dock = DockStyle.Fill;
            rightTopPanel.AutoScroll = true;
            rightTopLayout.Dock = DockStyle.Fill;
            rightTopLayout.RowCount = 2;
            rightTopLayout.ColumnCount = 1;
            rightTopLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 40F));
            rightTopLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 60F));

            // grpUserRequest with copy button
            grpUserRequest.Dock = DockStyle.Fill;
            lblUserRequest.Dock = DockStyle.Top;
            lblUserRequest.AutoSize = true;
            var userRequestLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            userRequestLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            userRequestLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            var btnCopyUserRequest = new Button
            {
                Text = "📋",
                Size = new Size(20, 20),
                Anchor = AnchorStyles.Right
            };
            btnCopyUserRequest.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(rtxtUserRequest.Text))
                    Clipboard.SetText(rtxtUserRequest.Text);
                else
                    MessageBox.Show(_resourceManager.GetString("NoTextToCopy", _currentCulture));
            };
            userRequestLayout.Controls.Add(btnCopyUserRequest, 0, 0);
            rtxtUserRequest.Dock = DockStyle.Fill;
            userRequestLayout.Controls.Add(rtxtUserRequest, 0, 1);
            grpUserRequest.Controls.Add(lblUserRequest);
            grpUserRequest.Controls.Add(userRequestLayout);

            // grpGenerateRequest with copy button
            grpGenerateRequest.Dock = DockStyle.Fill;
            var genLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4
            };
            genLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            genLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            genLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            genLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            genLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            genLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            rdoPartialStructure.Dock = DockStyle.Fill;
            rdoPartialStructure.Checked = true;
            rdoFullStructure.Dock = DockStyle.Fill;
            chkFullCode.Dock = DockStyle.Fill;
            chkFullCode.AutoSize = true;
            chkIncludeExplanation.Dock = DockStyle.Fill;
            chkIncludeExplanation.AutoSize = true;
            btnGenerateAIRequest.Dock = DockStyle.Fill;
            btnGenerateAIRequest.Click += BtnGenerateAIRequest_Click;
            var generateRequestLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            generateRequestLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            generateRequestLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            var btnCopyGenerateRequest = new Button
            {
                Text = "📋",
                Size = new Size(20, 20),
                Anchor = AnchorStyles.Right
            };
            btnCopyGenerateRequest.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(rtxtGenerateRequest.Text))
                    Clipboard.SetText(rtxtGenerateRequest.Text);
                else
                    MessageBox.Show(_resourceManager.GetString("NoTextToCopy", _currentCulture));
            };
            generateRequestLayout.Controls.Add(btnCopyGenerateRequest, 0, 0);
            rtxtGenerateRequest.Dock = DockStyle.Fill;
            generateRequestLayout.Controls.Add(rtxtGenerateRequest, 0, 1);
            genLayout.Controls.Add(rdoPartialStructure, 0, 0);
            genLayout.Controls.Add(rdoFullStructure, 1, 0);
            genLayout.Controls.Add(chkFullCode, 0, 1);
            genLayout.Controls.Add(chkIncludeExplanation, 1, 1);
            genLayout.Controls.Add(btnGenerateAIRequest, 0, 2);
            genLayout.SetColumnSpan(btnGenerateAIRequest, 2);
            genLayout.Controls.Add(generateRequestLayout, 0, 3);
            genLayout.SetColumnSpan(generateRequestLayout, 2);
            grpGenerateRequest.Controls.Add(genLayout);

            rightTopLayout.Controls.Add(grpUserRequest, 0, 0);
            rightTopLayout.Controls.Add(grpGenerateRequest, 0, 1);
            rightTopPanel.Controls.Add(rightTopLayout);

            // grpAI with copy button
            rightBottomPanel.Dock = DockStyle.Fill;
            grpAI.Dock = DockStyle.Fill;
            var jsonResponseLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            jsonResponseLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            jsonResponseLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            var btnCopyJsonResponse = new Button
            {
                Text = "📋",
                Size = new Size(20, 20),
                Anchor = AnchorStyles.Right
            };
            btnCopyJsonResponse.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(rtxtJsonResponse.Text))
                    Clipboard.SetText(rtxtJsonResponse.Text);
                else
                    MessageBox.Show(_resourceManager.GetString("NoTextToCopy", _currentCulture));
            };
            jsonResponseLayout.Controls.Add(btnCopyJsonResponse, 0, 0);
            rtxtJsonResponse.Dock = DockStyle.Fill;
            jsonResponseLayout.Controls.Add(rtxtJsonResponse, 0, 1);
            btnApplyChanges.Dock = DockStyle.Bottom;
            btnApplyChanges.Height = 40;
            btnApplyChanges.Click += BtnApplyChanges_Click;
            grpAI.Controls.Add(jsonResponseLayout);
            grpAI.Controls.Add(btnApplyChanges);
            rightBottomPanel.Controls.Add(grpAI);

            rightLayout.Controls.Add(rightTopPanel, 0, 0);
            rightLayout.Controls.Add(rightBottomPanel, 0, 1);
            mainLayoutPanel.Controls.Add(leftPanel, 0, 0);
            mainLayoutPanel.Controls.Add(rightLayout, 1, 0);
            this.Controls.Add(mainLayoutPanel);
            this.Dock = DockStyle.Fill;
            this.ResumeLayout(false);
        }

        public void SetCulture(CultureInfo culture)
        {
            _currentCulture = culture;
            ApplyLanguage();
        }

        private void ApplyLanguage()
        {
            grpFolder.Text = _resourceManager.GetString("ProjectFolder", _currentCulture);
            lblFolderPath.Text = _resourceManager.GetString("ProjectRootFolder", _currentCulture);
            btnSelectFolder.Text = _resourceManager.GetString("Select", _currentCulture);
            chkGitIgnore.Text = _resourceManager.GetString("ApplyGitIgnoreFilter", _currentCulture);
            grpOptions.Text = _resourceManager.GetString("Options", _currentCulture);
            chkCalculateStats.Text = _resourceManager.GetString("CalculateFileStats", _currentCulture);
            btnStopStats.Text = _resourceManager.GetString("Stop", _currentCulture);
            grpTree.Text = _resourceManager.GetString("ProjectStructure", _currentCulture);
            lblSelectedStats.Text = _resourceManager.GetString("SelectedItemsStats", _currentCulture) + ": 0 chars, 0 lines";
            grpUserRequest.Text = _resourceManager.GetString("TaskUserRequest", _currentCulture);
            lblUserRequest.Text = _resourceManager.GetString("DescribeYourTask", _currentCulture);
            grpGenerateRequest.Text = _resourceManager.GetString("GenerateAIRequest", _currentCulture);
            rdoPartialStructure.Text = _resourceManager.GetString("PartialStructure", _currentCulture);
            rdoFullStructure.Text = _resourceManager.GetString("FullStructure", _currentCulture);
            chkFullCode.Text = _resourceManager.GetString("ReturnFullCodeForEachFile", _currentCulture);
            chkIncludeExplanation.Text = _resourceManager.GetString("IncludeExplanationSummary", _currentCulture);
            btnGenerateAIRequest.Text = _resourceManager.GetString("GenerateRequest", _currentCulture);
            grpAI.Text = _resourceManager.GetString("AIInteraction", _currentCulture);
            btnApplyChanges.Text = _resourceManager.GetString("ApplyChanges", _currentCulture);
        }

        private void TreeViewFiles_AfterCheck(object? sender, TreeViewEventArgs e)
        {
            if (e?.Node == null) return;
            if (isUpdatingCheck) return;
            isUpdatingCheck = true;
            UpdateChildNodes(e.Node, e.Node.Checked);
            UpdateParentNodes(e.Node);
            isUpdatingCheck = false;
            UpdateSelectedStats();
            _saveTimer.Stop();
            _saveTimer.Start();
        }

        private void UpdateChildNodes(TreeNode? node, bool isChecked)
        {
            if (node == null) return;
            foreach (TreeNode child in node.Nodes)
            {
                child.Checked = isChecked;
                if (child.Nodes.Count > 0)
                    UpdateChildNodes(child, isChecked);
            }
        }

        private void UpdateParentNodes(TreeNode? node)
        {
            if (node == null || node.Parent == null) return;
            bool allChecked = true;
            foreach (TreeNode sibling in node.Parent.Nodes)
            {
                if (!sibling.Checked)
                {
                    allChecked = false;
                    break;
                }
            }
            node.Parent.Checked = allChecked;
            UpdateParentNodes(node.Parent);
        }

        private async void LoadTree()
        {
            if (string.IsNullOrWhiteSpace(txtFolderPath.Text)) return;
            _rootFolder = txtFolderPath.Text;
            _selectionFilePath = Path.Combine(_rootFolder, ".aidevhelper_selected.json");
            bool applyGitIgnore = chkGitIgnore.Checked;
            bool calculateStats = chkCalculateStats.Checked;
            treeViewFiles.Nodes.Clear();
            lblStatsIndicator.Text = "";
            _statsCts?.Cancel();
            _statsCts = new CancellationTokenSource();
            var rootDir = new DirectoryInfo(_rootFolder);
            var rootNode = TreeViewHelper.BuildDirectoryNode(rootDir, _rootFolder, applyGitIgnore);
            rootNode.Tag = _rootFolder;
            treeViewFiles.Nodes.Add(rootNode);
            treeViewFiles.CollapseAll();
            if (treeViewFiles.Nodes.Count > 0)
                treeViewFiles.Nodes[0].Expand();
            LoadSelections();
            if (calculateStats)
            {
                lblStatsIndicator.Text = _resourceManager.GetString("CalculatingStats", _currentCulture);
                try
                {
                    await Task.Run(() =>
                    {
                        TreeViewHelper.UpdateAllNodesStats(treeViewFiles.Nodes, _rootFolder, _statsCts.Token);
                    }, _statsCts.Token);
                    var (totalLines, totalChars) = await ProjectStatsCalculator.CalculateAsync(treeViewFiles.Nodes, _rootFolder, _statsCts.Token);
                    lblSelectedStats.Text = $"{_resourceManager.GetString("SelectedItemsStats", _currentCulture)}: {totalChars} chars, {totalLines} lines";
                    lblStatsIndicator.Text = _resourceManager.GetString("Done", _currentCulture);
                }
                catch (OperationCanceledException)
                {
                    lblStatsIndicator.Text = _resourceManager.GetString("Cancelled", _currentCulture);
                }
            }
            else
            {
                UpdateSelectedStats();
            }
        }

        private void UpdateSelectedStats()
        {
            long totalLines = 0;
            long totalChars = 0;
            foreach (TreeNode node in treeViewFiles.Nodes)
            {
                RecalcStats(node, ref totalLines, ref totalChars);
            }
            lblSelectedStats.Text = $"{_resourceManager.GetString("SelectedItemsStats", _currentCulture)}: {totalChars} chars, {totalLines} lines";
        }

        private void RecalcStats(TreeNode? node, ref long totalLines, ref long totalChars)
        {
            if (node == null) return;
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
                RecalcStats(child, ref totalLines, ref totalChars);
            }
        }

        private void BtnStopStats_Click(object? sender, EventArgs e)
        {
            _statsCts?.Cancel();
            lblStatsIndicator.Text = _resourceManager.GetString("Cancelled", _currentCulture);
        }

        private void BtnSelectFolder_Click(object? sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                txtFolderPath.Text = fbd.SelectedPath;
                chkGitIgnore.Checked = File.Exists(Path.Combine(fbd.SelectedPath, ".gitignore"));
                LoadTree();
            }
        }

        private void BtnGenerateAIRequest_Click(object? sender, EventArgs e)
        {
            string userRequest = rtxtUserRequest.Text.Trim();
            string structureText = rdoPartialStructure.Checked
                ? TreeViewHelper.BuildPartialStructureText(treeViewFiles.Nodes)
                : TreeViewHelper.BuildFullStructureText(treeViewFiles.Nodes);
            var sb = new StringBuilder();
            sb.AppendLine(userRequest);
            sb.AppendLine();
            sb.AppendLine(structureText);
            sb.AppendLine();
            sb.AppendLine(_resourceManager.GetString("OriginalFilesForModification", _currentCulture));
            int count = 1;
            TreeViewHelper.TraverseNodes(treeViewFiles.Nodes, _rootFolder, ref count, sb);
            sb.AppendLine();
            sb.AppendLine(_resourceManager.GetString("ReturnResultInJson", _currentCulture));
            sb.AppendLine("{");
            sb.AppendLine("  \"delete\": [\"path/to/file1\", ...],");
            if (chkFullCode.Checked)
            {
                sb.AppendLine("  \"modify\": [ { \"path\": \"...\", \"code\": \"...\" }, ...],");
                sb.AppendLine("  \"create\": [ { \"path\": \"...\", \"code\": \"...\" }, ...]");
                sb.AppendLine("}");
                sb.AppendLine();
                sb.AppendLine(_resourceManager.GetString("ReturnFullCodeInstruction", _currentCulture));
            }
            else
            {
                sb.AppendLine("  \"modify\": [ { \"path\": \"...\", \"code\": \"...\", \"lineStart\": 10, \"lineEnd\": 20 }, ...],");
                sb.AppendLine("  \"create\": [ { \"path\": \"...\", \"code\": \"...\" }, ...]");
                sb.AppendLine("}");
                sb.AppendLine();
                sb.AppendLine(_resourceManager.GetString("LineStartEndInstruction", _currentCulture));
            }
            if (chkIncludeExplanation.Checked)
            {
                sb.AppendLine();
                sb.AppendLine(_resourceManager.GetString("IncludeExplanationInstruction", _currentCulture));
            }
            sb.AppendLine();
            sb.AppendLine(_resourceManager.GetString("CleanCodeInstruction", _currentCulture));
            sb.AppendLine(_resourceManager.GetString("TestInstructions", _currentCulture));
            sb.AppendLine(_resourceManager.GetString("MultipleApproachesInstruction", _currentCulture));
            sb.AppendLine(_resourceManager.GetString("KeyPointsInstruction", _currentCulture));
            rtxtGenerateRequest.Text = sb.ToString();
        }

        private void BtnApplyChanges_Click(object? sender, EventArgs e)
        {
            string jsonResponse = rtxtJsonResponse.Text.Trim();
            if (string.IsNullOrEmpty(jsonResponse))
            {
                MessageBox.Show(_resourceManager.GetString("PasteJsonResponse", _currentCulture));
                return;
            }
            try
            {
                var jsonHandler = new JsonHandler();
                var changes = jsonHandler.ParseProjectChanges(jsonResponse);
                var fileManager = new FileManager();
                fileManager.SetProjectPath(_rootFolder);
                if (changes.Delete != null)
                {
                    foreach (var pathToDelete in changes.Delete)
                        fileManager.DeletePath(pathToDelete);
                }
                if (changes.Modify != null)
                {
                    foreach (var file in changes.Modify)
                    {
                        string fullPath = Path.Combine(fileManager.ProjectPath, file.Path);
                        if (file.LineStart.HasValue && file.LineEnd.HasValue)
                        {
                            if (File.Exists(fullPath))
                            {
                                var lines = new List<string>(File.ReadAllLines(fullPath));
                                int start = Math.Max(file.LineStart.Value - 1, 0);
                                int end = Math.Min(file.LineEnd.Value - 1, lines.Count - 1);
                                int cnt = (end - start + 1);
                                if (cnt > 0)
                                    lines.RemoveRange(start, cnt);
                                lines.InsertRange(start, file.Code.Split('\n'));
                                File.WriteAllLines(fullPath, lines);
                            }
                            else
                            {
                                fileManager.CreateFile(file.Path, file.Code);
                            }
                        }
                        else
                        {
                            fileManager.CreateFile(file.Path, file.Code);
                        }
                    }
                }
                if (changes.Create != null)
                {
                    foreach (var f in changes.Create)
                    {
                        fileManager.CreateFile(f.Path, f.Code);
                    }
                }
                LoadTree();
                MessageBox.Show(_resourceManager.GetString("ChangesAppliedSuccessfully", _currentCulture));
            }
            catch (Exception ex)
            {
                MessageBox.Show(_resourceManager.GetString("ErrorApplyingChanges", _currentCulture) + ": " + ex.Message);
            }
        }

        private List<string> GetCheckedRelativePaths()
        {
            List<string> checkedPaths = new List<string>();
            CollectCheckedPaths(treeViewFiles.Nodes, checkedPaths);
            return checkedPaths;
        }

        private void CollectCheckedPaths(TreeNodeCollection nodes, List<string> checkedPaths)
        {
            foreach (TreeNode node in nodes)
            {
                if (node.Checked && node.Tag is string fullPath)
                {
                    string relativePath = Path.GetRelativePath(_rootFolder, fullPath).Replace('\\', '/');
                    checkedPaths.Add(relativePath);
                }
                if (node.Nodes.Count > 0)
                    CollectCheckedPaths(node.Nodes, checkedPaths);
            }
        }

        private void SaveSelections()
        {
            try
            {
                var checkedPaths = GetCheckedRelativePaths();
                string json = JsonConvert.SerializeObject(checkedPaths, Formatting.Indented);
                File.WriteAllText(_selectionFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error saving selections: " + ex.Message);
            }
        }

        private void LoadSelections()
        {
            if (File.Exists(_selectionFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_selectionFilePath);
                    var selectedPaths = JsonConvert.DeserializeObject<List<string>>(json);
                    if (selectedPaths != null)
                    {
                        foreach (var path in selectedPaths)
                        {
                            var node = FindNodeByRelativePath(path);
                            if (node != null)
                                node.Checked = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error loading selections: " + ex.Message);
                }
            }
        }

        private TreeNode? FindNodeByRelativePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return null;
            string[] parts = relativePath.Split('/');
            TreeNode? current = treeViewFiles.Nodes[0];
            foreach (string part in parts)
            {
                current = current.Nodes.Cast<TreeNode>().FirstOrDefault(n => n.Text == part);
                if (current == null)
                    return null;
            }
            return current;
        }
    }
}

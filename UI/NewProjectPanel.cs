using System;
using System.Drawing;
using System.Windows.Forms;
using System.Resources;
using System.Globalization;

namespace AIDevHelper.UI
{
    public class NewProjectPanel : UserControl
    {
        private ResourceManager _resourceManager;
        private CultureInfo _currentCulture;

        private TableLayoutPanel mainLayoutPanel = new TableLayoutPanel();
        private GroupBox grpProjectInfo = new GroupBox();
        private TableLayoutPanel infoLayout = new TableLayoutPanel();
        private Label lblProjectName = new Label();
        private TextBox txtProjectName = new TextBox();
        private Label lblTechStack = new Label();
        private TextBox txtTechStack = new TextBox();
        private Label lblDescription = new Label();
        private RichTextBox rtxtDescription = new RichTextBox();
        private Label lblRequirements = new Label();
        private RichTextBox rtxtRequirements = new RichTextBox();
        private GroupBox grpNewProjectActions = new GroupBox();
        private TableLayoutPanel actionsLayout = new TableLayoutPanel();
        private Button btnGenerateRequest = new Button();
        private RichTextBox rtxtRequestOutput = new RichTextBox();

        public NewProjectPanel(CultureInfo culture)
        {
            _currentCulture = culture;
            _resourceManager = new ResourceManager("AIDevHelper.Resources.Lang", typeof(NewProjectPanel).Assembly);
            InitializeComponent();
            ApplyLanguage();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            mainLayoutPanel.Dock = DockStyle.Fill;
            mainLayoutPanel.ColumnCount = 2;
            mainLayoutPanel.RowCount = 1;
            mainLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // grpProjectInfo
            grpProjectInfo.Dock = DockStyle.Fill;
            infoLayout.Dock = DockStyle.Fill;
            infoLayout.ColumnCount = 1;
            infoLayout.RowCount = 8;
            infoLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            infoLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            infoLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            infoLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            infoLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            infoLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            infoLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 70F));
            infoLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            infoLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
            lblProjectName.TextAlign = ContentAlignment.MiddleLeft;
            lblProjectName.Dock = DockStyle.Fill;
            txtProjectName.Dock = DockStyle.Fill;
            lblTechStack.TextAlign = ContentAlignment.MiddleLeft;
            lblTechStack.Dock = DockStyle.Fill;
            txtTechStack.Dock = DockStyle.Fill;
            lblDescription.TextAlign = ContentAlignment.MiddleLeft;
            lblDescription.Dock = DockStyle.Fill;
            var descriptionLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            descriptionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            descriptionLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            var btnCopyDescription = new Button
            {
                Text = "📋",
                Size = new Size(20, 20),
                Anchor = AnchorStyles.Right
            };
            btnCopyDescription.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(rtxtDescription.Text))
                    Clipboard.SetText(rtxtDescription.Text);
                else
                    MessageBox.Show(_resourceManager.GetString("NoTextToCopy", _currentCulture));
            };
            descriptionLayout.Controls.Add(btnCopyDescription, 0, 0);
            rtxtDescription.Dock = DockStyle.Fill;
            descriptionLayout.Controls.Add(rtxtDescription, 0, 1);
            lblRequirements.TextAlign = ContentAlignment.MiddleLeft;
            lblRequirements.Dock = DockStyle.Fill;
            var requirementsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            requirementsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            requirementsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            var btnCopyRequirements = new Button
            {
                Text = "📋",
                Size = new Size(20, 20),
                Anchor = AnchorStyles.Right
            };
            btnCopyRequirements.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(rtxtRequirements.Text))
                    Clipboard.SetText(rtxtRequirements.Text);
                else
                    MessageBox.Show(_resourceManager.GetString("NoTextToCopy", _currentCulture));
            };
            requirementsLayout.Controls.Add(btnCopyRequirements, 0, 0);
            rtxtRequirements.Dock = DockStyle.Fill;
            requirementsLayout.Controls.Add(rtxtRequirements, 0, 1);
            infoLayout.Controls.Add(lblProjectName, 0, 0);
            infoLayout.Controls.Add(txtProjectName, 0, 1);
            infoLayout.Controls.Add(lblTechStack, 0, 2);
            infoLayout.Controls.Add(txtTechStack, 0, 3);
            infoLayout.Controls.Add(lblDescription, 0, 4);
            infoLayout.Controls.Add(descriptionLayout, 0, 5);
            infoLayout.Controls.Add(lblRequirements, 0, 6);
            infoLayout.Controls.Add(requirementsLayout, 0, 7);
            grpProjectInfo.Controls.Add(infoLayout);

            // grpNewProjectActions
            grpNewProjectActions.Dock = DockStyle.Fill;
            actionsLayout.Dock = DockStyle.Fill;
            actionsLayout.ColumnCount = 1;
            actionsLayout.RowCount = 2;
            actionsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            actionsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            btnGenerateRequest.Dock = DockStyle.Fill;
            btnGenerateRequest.Click += BtnGenerateRequest_Click;
            var requestOutputLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            requestOutputLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            requestOutputLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            var btnCopyRequestOutput = new Button
            {
                Text = "📋",
                Size = new Size(20, 20),
                Anchor = AnchorStyles.Right
            };
            btnCopyRequestOutput.Click += (s, e) =>
            {
                if (!string.IsNullOrEmpty(rtxtRequestOutput.Text))
                    Clipboard.SetText(rtxtRequestOutput.Text);
                else
                    MessageBox.Show(_resourceManager.GetString("NoTextToCopy", _currentCulture));
            };
            requestOutputLayout.Controls.Add(btnCopyRequestOutput, 0, 0);
            rtxtRequestOutput.Dock = DockStyle.Fill;
            requestOutputLayout.Controls.Add(rtxtRequestOutput, 0, 1);
            actionsLayout.Controls.Add(btnGenerateRequest, 0, 0);
            actionsLayout.Controls.Add(requestOutputLayout, 0, 1);
            grpNewProjectActions.Controls.Add(actionsLayout);

            mainLayoutPanel.Controls.Add(grpProjectInfo, 0, 0);
            mainLayoutPanel.Controls.Add(grpNewProjectActions, 1, 0);
            this.Controls.Add(mainLayoutPanel);
            this.Dock = DockStyle.Fill;
            this.Name = "NewProjectPanel";
            this.Size = new Size(900, 500);
            this.ResumeLayout(false);
        }

        public void SetCulture(CultureInfo culture)
        {
            _currentCulture = culture;
            ApplyLanguage();
        }

        private void ApplyLanguage()
        {
            grpProjectInfo.Text = _resourceManager.GetString("ProjectInfo", _currentCulture);
            lblProjectName.Text = _resourceManager.GetString("ProjectName", _currentCulture);
            lblTechStack.Text = _resourceManager.GetString("TechStack", _currentCulture);
            lblDescription.Text = _resourceManager.GetString("Description", _currentCulture);
            lblRequirements.Text = _resourceManager.GetString("AdditionalRequirements", _currentCulture);
            grpNewProjectActions.Text = _resourceManager.GetString("NewProjectActions", _currentCulture);
            btnGenerateRequest.Text = _resourceManager.GetString("GenerateRequest", _currentCulture);
        }

        private void BtnGenerateRequest_Click(object sender, EventArgs e)
        {
            string projectName = txtProjectName.Text.Trim();
            string techStack = txtTechStack.Text.Trim();
            string description = rtxtDescription.Text.Trim();
            string requirements = rtxtRequirements.Text.Trim();
            string prompt = _resourceManager.GetString("CreateNewProject", _currentCulture) + ":\n" +
                $"- {_resourceManager.GetString("ProjectName", _currentCulture)}: {projectName}\n" +
                $"- {_resourceManager.GetString("TechStack", _currentCulture)}: {techStack}\n" +
                $"- {_resourceManager.GetString("Description", _currentCulture)}: {description}\n" +
                $"- {_resourceManager.GetString("AdditionalRequirements", _currentCulture)}: {requirements}\n\n" +
                _resourceManager.GetString("ResponseRequirements", _currentCulture) + ":\n" +
                _resourceManager.GetString("ReturnResultInJson", _currentCulture) + "\n" +
                "{ \"folders\": [...], \"files\": [ { \"path\": \"...\", \"code\": \"...\" } ] }\n" +
                _resourceManager.GetString("DoNotShortenCode", _currentCulture) + "\n\n" +
                _resourceManager.GetString("CleanCodeInstruction", _currentCulture) + "\n\n" +
                _resourceManager.GetString("TestInstructions", _currentCulture) + "\n" +
                _resourceManager.GetString("MultipleApproachesInstruction", _currentCulture) + "\n" +
                _resourceManager.GetString("KeyPointsInstruction", _currentCulture);
            rtxtRequestOutput.Text = prompt;
        }
    }
}

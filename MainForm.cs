using System;
using System.Windows.Forms;
using AIDevHelper.UI;
using System.Globalization;

namespace AIDevHelper
{
    public partial class MainForm : Form
    {
        private TabControl tabControlMain = new TabControl();
        private ComboBox cmbLanguage = new ComboBox();
        private CultureInfo _currentCulture;

        public MainForm(CultureInfo culture)
        {
            _currentCulture = culture;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "AIDevHelper";
            this.Width = 1600;
            this.Height = 1300;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Language selection combo box
            cmbLanguage.Items.AddRange(new string[] { "English", "Русский", "Français", "Deutsch" });
            cmbLanguage.SelectedIndex = _currentCulture.TwoLetterISOLanguageName switch
            {
                "ru" => 1,
                "fr" => 2,
                "de" => 3,
                _ => 0
            };
            cmbLanguage.Dock = DockStyle.Top;
            cmbLanguage.SelectedIndexChanged += CmbLanguage_SelectedIndexChanged;

            // Tab control for project panels
            tabControlMain.Dock = DockStyle.Fill;

            var newProjectPanel = new NewProjectPanel(_currentCulture);
            var modifyProjectPanel = new ModifyProjectPanel(_currentCulture);

            var tabNewProject = new TabPage("New Project") { Controls = { newProjectPanel } };
            var tabModifyProject = new TabPage("Modify Project") { Controls = { modifyProjectPanel } };

            tabControlMain.TabPages.Add(tabNewProject);
            tabControlMain.TabPages.Add(tabModifyProject);

            // Add controls to the form
            this.Controls.Add(tabControlMain);
            this.Controls.Add(cmbLanguage);
        }

        private void CmbLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedLang = cmbLanguage.SelectedItem.ToString();
            CultureInfo newCulture = selectedLang switch
            {
                "English" => new CultureInfo("en"),
                "Русский" => new CultureInfo("ru"),
                "Français" => new CultureInfo("fr"),
                "Deutsch" => new CultureInfo("de"),
                _ => CultureInfo.CurrentCulture
            };
            _currentCulture = newCulture;
            foreach (TabPage tab in tabControlMain.TabPages)
            {
                if (tab.Controls[0] is NewProjectPanel newPanel)
                {
                    newPanel.SetCulture(newCulture);
                }
                else if (tab.Controls[0] is ModifyProjectPanel modifyPanel)
                {
                    modifyPanel.SetCulture(newCulture);
                }
            }
        }
    }
}

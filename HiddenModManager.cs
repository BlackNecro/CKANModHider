using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using CKAN;
using CKAN.CmdLine;

namespace ModHidePlugin
{
    public class HiddenModManager
    {
        private ToolStripMenuItem HiddenModsButton;
        

        HashSet<string> HiddenMods = new HashSet<string>();
        private ToolStripMenuItem FilterToolButton;
        private FieldInfo OldFilterField;

        private HiddenModManager()
        {
            LoadSettings();
            AddContextMenu();
            AddHideButton();

        }

        private void UpdateCount()
        {
            HiddenModsButton.Text = String.Format("Hidden Mods ({0})", HiddenMods.Count);
        }

        private void AddHideButton()
        {
            HiddenModsButton = new ToolStripMenuItem("Hidden Mods");
            HiddenModsButton.Click += (s, e) => ShowHiddenStuff();
            UpdateCount();

            try
            {
                var field = typeof(Main).GetField("FilterToolButton",
                    BindingFlags.Instance | BindingFlags.GetField | BindingFlags.GetProperty | BindingFlags.NonPublic);
                FilterToolButton = (ToolStripMenuItem) field.GetValue(Main.Instance);
                FilterToolButton.DropDownItems.Add(HiddenModsButton);

                OldFilterField = typeof(MainModList).GetField("_modFilter",
                    BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message,"HiddenModsError");
            }

            Main.Instance.mainModList.ModFiltersUpdated += source => HideHiddenStuff();
        }

        public static HiddenModManager Instance = new HiddenModManager();

        public void LoadSettings()
        {
            if (File.Exists(HiddenModsFilePath))
            {
                var hiddenMods = File.ReadAllLines(HiddenModsFilePath);
                HiddenMods.Clear();
                HiddenMods.UnionWith(hiddenMods);
            }            
        }

        public void SaveSettings()
        {            
            File.WriteAllLines(HiddenModsFilePath,HiddenMods);
            UpdateCount();
        }

        public string HiddenModsFilePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),"HiddenMods.cfg");

        public void AddHide(string mod)
        {
            HiddenMods.Add(mod);
            SaveSettings();
            HideHiddenStuff();
        }

        public void RemoveHide(string mod)
        {
            HiddenMods.Remove(mod);
            SaveSettings();
            ShowHiddenStuff();
        }

        public bool IsHidden(string mod) => HiddenMods.Contains(mod);

        public void HideHiddenStuff()
        {
            foreach (DataGridViewRow row in Main.Instance.ModList.Rows)
            {
                var mod = (GUIMod) row.Tag;
                row.Visible = Main.Instance.mainModList.IsVisible(mod) && !IsHidden(mod.Identifier);
            }
        }

        public void ShowHiddenStuff()
        {
            Main.Instance.mainModList.ModFilter = GUIModFilter.All;
            FilterToolButton.Text = "Filter (Hidden)";
            foreach (DataGridViewRow row in Main.Instance.ModList.Rows)
            {
                var mod = (GUIMod)row.Tag;
                row.Visible = IsHidden(mod.Identifier);
            }
        }

        public void AddContextMenu()
        {

            Main.Instance.ModList.CellMouseClick += ModListOnCellMouseClick;
        }

        private void ModListOnCellMouseClick(object sender, DataGridViewCellMouseEventArgs eventArgs)
        {
            if(eventArgs.Button != MouseButtons.Right)
                return;
            
                
            var row = Main.Instance.ModList.Rows[eventArgs.RowIndex];
            var mod = (GUIMod)row.Tag;
            MenuItem menuItem;
            if (IsHidden(mod.Identifier))
            {
                menuItem = new MenuItem("Show", (s, e) => RemoveHide(mod.Identifier));
            }
            else
            {
                menuItem = new MenuItem("Hide", (s, e) => AddHide(mod.Identifier));

            }
            var contextmenu = new ContextMenu(new[] { menuItem });

            contextmenu.Show(Main.Instance.ModList, Main.Instance.ModList.PointToClient(Cursor.Position));

        }
    }
}
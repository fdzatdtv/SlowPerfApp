using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace SlowPerfWpfApp
{
    /// <summary>
    /// Interaction logic for RecentListView.xaml
    /// </summary>
    public partial class RecentListView : UserControl
    {
        private List<RecentListEntry> _entries;

        public event EventHandler RecentSelectionChanged;
        public RecentListEntry SelectedItem => lvRecent.SelectedItem as RecentListEntry;

        public RecentListView()
        {
            InitializeComponent();

            InitEntries();

            Loaded += RecentListView_Loaded;
        }

        private void RecentListView_Loaded(object sender, RoutedEventArgs e)
        {
            lvRecent.ItemsSource = _entries;
            if (_entries.Count > 0)
            {
                _ = LoadRecentList(out var selected);
                if (selected == null)
                    lvRecent.SelectedIndex = 0;
                else
                {
                    int selectedIndex = _entries.FindIndex(entry => entry.Path == selected.Path);
                    lvRecent.SelectedIndex = selectedIndex >= 0 ? selectedIndex : 0;
                }
            }
        }

        private void InitEntries()
        {
            List<RecentListEntry> entries = LoadRecentList(out _);
            if (entries != null && entries.Count > 0)
                _entries = entries;
            else
            {
                _entries = new List<RecentListEntry>();
                _entries.AddRange(from path in new[]
                    {
                        "Sales01.csv",
                        //"Sales02.csv",
                    }
                    let entry = CreateEntry(path)
                    where entry != null
                    select entry);
            }
        }

        private RecentListEntry CreateEntry(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;
            try
            {
                return new RecentListEntry(path);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void LvRecent_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SaveRecentList();
            Mouse.SetCursor(Cursors.Wait);
            RecentSelectionChanged?.Invoke(this, EventArgs.Empty);
            Mouse.SetCursor(Cursors.None);
        }

        private void OnAddButtonClicked(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Csv files (*.csv)|*.csv|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                Mouse.SetCursor(Cursors.Wait);
                RecentListEntry entry = CreateEntry(openFileDialog.FileName);
                if (entry != null)
                {
                    _entries.Add(entry);
                    SaveRecentList();
                    lvRecent.ItemsSource = null;
                    lvRecent.ItemsSource = _entries;
                    lvRecent.SelectedItem = entry;
                }
                Mouse.SetCursor(Cursors.None);
            }
        }

        private void SaveRecentList()
        {
            int selectedIndex = lvRecent.SelectedIndex;
            string content = "";
            int currIndex = 0;
            foreach (var path in from entry in _entries select entry.Path)
            {
                if (currIndex++ == selectedIndex)
                    content += "1,";
                else
                    content += "0,";
                content += path;
                content += "\n";
            }

            content = content.Remove(content.Length - 1, 1);
            File.WriteAllText("SlowPerfWpfAppRecentList.txt", content);
        }

        private List<RecentListEntry> LoadRecentList(out RecentListEntry selected)
        {
            selected = null;
            var entries = new List<RecentListEntry>();
            string content;
            try
            {
                content = File.ReadAllText("SlowPerfWpfAppRecentList.txt");
            }
            catch (Exception)
            {
                return entries;
            }

            foreach (string line in content.Split('\n'))
            {
                string[] splitted = line.Split(',');
                string path = splitted[1];
                RecentListEntry entry = CreateEntry(path);
                if (entry != null)
                {
                    entries.Add(entry);
                    if (splitted[0] == "1")
                        selected = entry;
                }
            }

            return entries;
        }
    }

    public class RecentListEntry
    {
        public string Path { get; }
        public string Filename { get; }
        public string Name { get; }

        public RecentListEntry(string path)
        {
            if (!System.IO.Path.IsPathRooted(path))
                path = System.IO.Path.GetFullPath(path);
            if (!File.Exists(path))
                throw new FileNotFoundException($"Can't find file: {path}", path);
            Path = path;
            Filename = System.IO.Path.GetFileName(Path);
            Name = System.IO.Path.GetFileNameWithoutExtension(Path);
        }
    }
}

// � 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Linq;
using System.Windows;
using Sitecore.Rocks.Commands;
using Sitecore.Rocks.ContentTrees;
using Sitecore.Rocks.Data;
using Sitecore.Rocks.UI.Libraries.ContentTrees.VirtualItems;
using Sitecore.Rocks.UI.Libraries.Dialogs;
using Sitecore.Rocks.UI.Libraries.SearchLibraries;
using Sitecore.Rocks.UI.Repositories;

namespace Sitecore.Rocks.UI.Libraries.Commands.Libraries
{
    [Command]
    public class NewSearchLibrary : CommandBase
    {
        public NewSearchLibrary()
        {
            Text = "New Search Library";
            Group = "New";
            SortingValue = 3000;
            Icon = new Icon("Sitecore.Rocks", "/Resources/16x16/search.png");
        }

        public override bool CanExecute(object parameter)
        {
            var context = parameter as ContentTreeContext;
            if (context == null)
            {
                return false;
            }

            if (context.SelectedItems.Count() != 1)
            {
                return false;
            }

            var item = context.SelectedItems.FirstOrDefault() as LibrariesRootTreeViewItem;
            return item != null;
        }

        public override void Execute(object parameter)
        {
            var repository = RepositoryManager.GetRepository(RepositoryManager.Folders);
            var folderRepository = repository.Entries.FirstOrDefault();
            if (folderRepository == null)
            {
                return;
            }

            var databaseUri = DatabaseUri.Empty;
            var searchText = string.Empty;
            var name = "Folder";
            do
            {
                var dialog = new SearchDialog(searchText, databaseUri, name);
                if (AppHost.Shell.ShowDialog(dialog) != true)
                {
                    return;
                }

                name = dialog.SearchName;
                databaseUri = dialog.DatabaseUri ?? DatabaseUri.Empty;
                searchText = dialog.SearchText;

                if (LibraryManager.Libraries.Any(w => string.Compare(w.Name, name, StringComparison.InvariantCultureIgnoreCase) == 0))
                {
                    AppHost.MessageBox(string.Format("A folder with the name '{0}' already exists.\n\nPlease choose another name.", name), "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    break;
                }
            }
            while (true);

            var fileName = IO.File.GetSafeFileName(name + ".xml");
            fileName = Path.Combine(folderRepository.Path, fileName);

            var folder = new SearchLibrary(fileName, name)
            {
                DatabaseUri = databaseUri,
                SearchText = searchText
            };

            folder.Save();
            folder.Initialize();

            LibraryManager.Add(folder);

            folder.IsExpanded = true;
            folder.Refresh();
        }
    }
}

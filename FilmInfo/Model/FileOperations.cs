using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FilmInfo.Model
{
    public class FileOperations
    {
        public static string GetDirectory()
        {
            using (var dlg = new CommonOpenFileDialog())
            {
                dlg.Title = "Choose root Folder...";
                dlg.IsFolderPicker = true;
                dlg.AddToMostRecentlyUsedList = false;
                dlg.AllowNonFileSystemItems = false;
                dlg.EnsureFileExists = true;
                dlg.EnsurePathExists = true;
                dlg.EnsureReadOnly = false;
                dlg.EnsureValidNames = true;
                dlg.Multiselect = false;
                dlg.ShowPlacesList = true;
                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    return dlg.FileName;
                }
            }
            throw new DirectoryNotFoundException();
        }

        public static BitmapImage LoadBitmapImage(string path)
        {
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }
    }
}

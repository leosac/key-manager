using Microsoft.Win32;

namespace Leosac.KeyManager.Library.UI.Helpers
{
    public static class KeyFileDialogHelper
    {
        private const string FileFilter = "Binary Files (*.bin)|*.bin|Text Files (*.txt)|*.txt|PEM Files (*.pem)|*.pem";

        public static void Import(Action<byte[]> importBinary, Action<string, KeyValueStringFormat?> importText)
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = true,
                Filter = FileFilter
            };
            if (dialog.ShowDialog() != true)
            {
                return;
            }

            if (dialog.FilterIndex == 1)
            {
                importBinary(System.IO.File.ReadAllBytes(dialog.FileName));
            }
            else
            {
                importText(System.IO.File.ReadAllText(dialog.FileName), dialog.FilterIndex == 3 ? KeyValueStringFormat.Pem : null);
            }
        }

        public static void Export(Func<byte[]> exportBinary, Func<KeyValueStringFormat?, string> exportText)
        {
            var dialog = new SaveFileDialog
            {
                Filter = FileFilter
            };
            if (dialog.ShowDialog() != true)
            {
                return;
            }

            if (dialog.FilterIndex == 1)
            {
                System.IO.File.WriteAllBytes(dialog.FileName, exportBinary());
            }
            else
            {
                System.IO.File.WriteAllText(dialog.FileName, exportText(dialog.FilterIndex == 3 ? KeyValueStringFormat.Pem : null));
            }
        }
    }
}

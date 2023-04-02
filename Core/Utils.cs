using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using MahApps.Metro.Controls;
using System.Windows.Input;

namespace Core
{
    public class Utils
    {
        // TODO:MVVMらしい方法へ移行
        // https://mahapps.github.io/controls/dialogs.html#:~:text=You%20can%20open%20dialogs%20from%20your%20viewmodel%20by%20using%20the%20IDialogCoordinator.
        // https://elf-mission.net/programming/wpf/getting-started-2020/step04/#:~:text=%E3%82%8C%E3%81%BE%E3%81%99%E3%80%82-,RegisterInstance,-ex.)%20containerRegistry.RegisterInstance
        public static MetroWindow GetMainWindow()
        {
            var w = Application.Current.MainWindow as MetroWindow;
            if (w == null) throw new Exception("");
            return w;
        }

        public static (Key key, ModifierKeys modifierKeys) ParseHotKeyText(string hotKeyText)
        {
            var modifiers = ModifierKeys.None;
            var splited = hotKeyText.Split('+');
            for (var i = 0; i < splited.Length - 1; ++i)
            {
                if (!Enum.TryParse(splited[i], out ModifierKeys m))
                {
                    m = splited[i] == "Ctrl" ? ModifierKeys.Control : ModifierKeys.None;
                }
                modifiers = modifiers | m;
            }
            if (!Enum.TryParse(splited.Last(), out Key key))
            {
                key = Key.None;
            }
            return (key, modifiers);
        }

        public static BitmapSource LoadBitmapFromPath(string path)
        {
            if (path == "") return null;
            var bitmap = new BitmapImage();
            var stream = File.OpenRead(path);
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;

            var metaData = BitmapFrame.Create(stream).Metadata as BitmapMetadata;
            stream.Position = 0;
            bitmap.EndInit();
            stream.Close();
            string query = "/app1/ifd/exif:{uint=274}";
            if (metaData == null || !metaData.ContainsQuery(query))
                return bitmap;


            switch (Convert.ToUInt32(metaData.GetQuery(query)))
            {
                case 1:
                    return bitmap;
                case 3:
                    return transformBitmap(
                        bitmap,
                        new RotateTransform(180));
                case 6:
                    return transformBitmap(
                        bitmap,
                        new RotateTransform(90));
                case 8:
                    return transformBitmap(
                        bitmap,
                        new RotateTransform(270));
                case 2:
                    return transformBitmap(
                        bitmap,
                        new ScaleTransform(-1, 1, 0, 0));
                case 4:
                    return transformBitmap(
                        bitmap,
                        new ScaleTransform(1, -1, 0, 0));
                case 5:
                    return transformBitmap(
                        transformBitmap(
                            bitmap,
                            new RotateTransform(90)
                        ),
                        new ScaleTransform(-1, 1, 0, 0));
                case 7:
                    return transformBitmap(
                        transformBitmap(
                            bitmap,
                            new RotateTransform(270)
                        ),
                        new ScaleTransform(-1, 1, 0, 0));
            }
            return bitmap;
        }
        private static BitmapSource transformBitmap(BitmapSource source, Transform transform)
        {
            var result = new TransformedBitmap();
            result.BeginInit();
            result.Source = source;
            result.Transform = transform;
            result.EndInit();
            return result;
        }
    }
}

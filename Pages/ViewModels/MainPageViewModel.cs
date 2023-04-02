using Core;
using Pages.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Media;
using System.Diagnostics;
using System.Reflection;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Pages.ViewModels
{
    public class MainPageViewModel : DisposableBindableBase, INavigationAware
    {
        public ReactivePropertySlim<MatrixTransform> Transform { get; }

        // 注意 : UI(mah:NumericUpDown)に合わせて、値は1から始まる
        public ReactivePropertySlim<int> ImageIndex { get; }

        public ReadOnlyReactivePropertySlim<List<string>> DisplayingImageSources { get; }

        public ReadOnlyReactivePropertySlim<string> ImageFileName { get; }

        public ReadOnlyReactivePropertySlim<int> MaxImagesCount { get; }

        public ReactiveCommand FirstImageCommand { get; }
        public ReactiveCommand LastImageCommand { get; }
        public ReactiveCommand PreviousImageCommand { get; }
        public ReactiveCommand NextImageCommand { get; }
        public ReactiveCommand PageBackCommand { get; }

        public ReactiveCommand ResetTransfromCommand { get; }

        public ReactiveCommand CopyImageFileNameCommand { get; }

        public ReactiveCommand SearchImageByFileNameCommand { get; }

        public ReactiveCommand SaveImageCommand { get; }

        public ReactiveCommand NavigateToSelectImagesSourceCommand { get; }

        public MainPageViewModel(IRegionManager regionManager, ImagesSourcesList imagesSources)
        {
            NavigateToSelectImagesSourceCommand = new ReactiveCommand()
                .WithSubscribe(
                    () => regionManager.RequestNavigate("ContentRegion", nameof(SelectImagesSourcesPage))
                ).AddTo(Disposables);
            Transform = new ReactivePropertySlim<MatrixTransform>(new MatrixTransform()).AddTo(Disposables);
            ImageIndex = new ReactivePropertySlim<int>(1).AddTo(Disposables);
            MaxImagesCount = imagesSources.Sources.ToCollectionChanged()
                .Select((_) => imagesSources.Sources.Max(x => x.ImageFilePaths.Value.Count))
                .ToReadOnlyReactivePropertySlim(imagesSources.Sources.Max(x => x.ImageFilePaths.Value.Count))
                .AddTo(Disposables);
            DisplayingImageSources = ImageIndex.Select(index =>
            {
                return imagesSources.Sources.Select(source =>
                {
                    if (index == 0 || source.ImageFilePaths.Value.Count < index)
                        return "";
                    else

                        return source.ImageFilePaths.Value[index - 1];
                }).ToList();
            }).ToReadOnlyReactivePropertySlim().AddTo(Disposables);
            ImageFileName = DisplayingImageSources.Select(sources =>
            {
                var path = sources.FirstOrDefault(it => it != "", "");
                return Path.GetFileName(path);
            }).ToReadOnlyReactivePropertySlim().AddTo(Disposables);

            FirstImageCommand = ImageIndex
                .Select(index => index > 1)
                .ToReactiveCommand()
                .WithSubscribe(() => ImageIndex.Value = 1)
                .AddTo(Disposables);

            LastImageCommand = ImageIndex
                .Select(index => index < MaxImagesCount.Value)
                .ToReactiveCommand()
                .WithSubscribe(() => ImageIndex.Value = MaxImagesCount.Value)
                .AddTo(Disposables);

            PreviousImageCommand = ImageIndex
                .Select(index => index > 1)
                .ToReactiveCommand()
                .WithSubscribe(() => ImageIndex.Value--)
                .AddTo(Disposables);

            NextImageCommand = ImageIndex
                .Select(index => index < MaxImagesCount.Value)
                .ToReactiveCommand()
                .WithSubscribe(() => ImageIndex.Value++)
                .AddTo(Disposables);

            ImageIndex
                .Subscribe((_) => Transform.Value = new MatrixTransform())
                .AddTo(Disposables);

            ResetTransfromCommand = new ReactiveCommand()
                .WithSubscribe(() => Transform.Value = new MatrixTransform())
                .AddTo(Disposables);
            CopyImageFileNameCommand = new ReactiveCommand()
                .WithSubscribe(() => Clipboard.SetData(DataFormats.Text, ImageFileName.Value))
                .AddTo(Disposables);
            SearchImageByFileNameCommand = new ReactiveCommand()
                .WithSubscribe(async () =>
                {
                    var input = await Utils.GetMainWindow().ShowInputAsync(
                        "画像検索", "画像のファイル名を入力してください"
                    );
                    if (input == null)
                        return;
                    var source = imagesSources.Sources.FirstOrDefault(x => x.ImageFilePaths.Value.Count > 0, null);
                    if (source == null)
                    {
                        await Utils.GetMainWindow().ShowMessageAsync(
                            "エラー",
                            "画像が1枚も開かれていないため、検索できませんでした。"
                        );
                    }
                    var imageFilePaths = source.ImageFilePaths.Value;
                    for (int i = 1; i <= imageFilePaths.Count; ++i)
                    {
                        var imageFilePath = imageFilePaths[i - 1];
                        if (imageFilePath == null) continue;
                        if (Path.GetFileName(imageFilePath).Contains(input))
                        {
                            ImageIndex.Value = i;
                            return;
                        }
                    }
                    await Utils.GetMainWindow().ShowMessageAsync(
                        "画像検索結果",
                        $"{input}が含まれる画像が存在しませんでした。"
                    );
                })
                .AddTo(Disposables);
            SaveImageCommand = new ReactiveCommand()
                .WithSubscribe(async () =>
                {
                    var defaultFileName = ImageFileName.Value;
                    var defaultFileExt = Path.GetExtension(defaultFileName).ToLower(); // ドットを含む拡張子
                    var saveFileDialog = new SaveFileDialog();
                    saveFileDialog.FileName = defaultFileName;
                    saveFileDialog.Filter = $"Image File|*{defaultFileExt}";
                    if (saveFileDialog.ShowDialog() != true)
                        return;

                    var controller = await Utils.GetMainWindow().ShowProgressAsync("保存中", "画像を保存中です...");
                    controller.SetIndeterminate();
                    await Task.Run(() =>
                    {
                        saveImage(saveFileDialog.FileName);
                    });
                    await controller.CloseAsync();
                })
                .AddTo(Disposables);
        }
        private BitmapEncoder tryFindBitmapEncoderByExt(string fileExtention)
        {
            switch (fileExtention)
            {
                case ".jpg":
                case ".jpeg":
                    return new JpegBitmapEncoder();
                case ".png":
                    return new PngBitmapEncoder();
                case ".bmp":
                    return new BmpBitmapEncoder();

                case ".gif":
                    return new GifBitmapEncoder();

                case ".tiff":
                    return new TiffBitmapEncoder();
                default:
                    Utils.GetMainWindow().ShowMessageAsync(
                       "エラー",
                       $"拡張子 {fileExtention} の画像を保存することはできません。"
                    );
                    return null;
            }
        }
        private void saveImage(string saveAs)
        {
            var ext = Path.GetExtension(saveAs).ToLower();
            var encoder = tryFindBitmapEncoderByExt(ext);
            if (encoder == null)
                return;
            var bitmaps = DisplayingImageSources.Value.Select(it =>
                Utils.LoadBitmapFromPath(it)
            ).ToList();
            var bitmapIndexes = Enumerable.Range(0, bitmaps.Count);

            var columns = Core.Properties.Settings.Default.Columns;
            var widths = Enumerable.Range(0, columns)
                .Select(x =>
                {
                    return bitmapIndexes
                        .Where(i => i % columns == x)
                        .Max(i =>
                        {
                            var b = bitmaps[i];
                            if (b == null)
                                return 0;
                            return b.PixelWidth;
                        });
                }).ToList();
            var heights = Enumerable.Range(0, (int)Math.Ceiling((double)bitmaps.Count / columns))
                .Select(x =>
                {
                    return bitmapIndexes
                        .Where(i => i / columns == x)
                        .Max(i =>
                        {
                            var b = bitmaps[i];
                            if (b == null)
                                return 0;
                            return b.PixelHeight;
                        });
                }).ToList();

            var drawingVisual = new DrawingVisual();
            using (var context = drawingVisual.RenderOpen())
            {
                for (var i = 0; i < bitmaps.Count; ++i)
                {
                    var x = i % columns;
                    var y = i / columns;
                    var left = widths.GetRange(0, x).Sum();
                    var top = heights.GetRange(0, y).Sum();
                    var b = bitmaps[i];
                    if (b != null)
                        context.DrawImage(b, new Rect(left, top, b.PixelWidth, b.PixelHeight));
                }
            }
            var target = new RenderTargetBitmap(widths.Sum(), heights.Sum(), 96, 96, PixelFormats.Pbgra32);
            target.Render(drawingVisual);
            encoder.Frames.Add(BitmapFrame.Create(target));
            using (Stream stream = File.Create(saveAs))
                encoder.Save(stream);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}

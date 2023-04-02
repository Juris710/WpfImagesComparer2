using Core;
using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;

namespace UserControls.ViewModels
{
    public class ImagesSourcePickerViewModel : DisposableBindableBase
    {
        const int MAX_DISPLAYING_IMAGES = 500;

        public ReadOnlyReactivePropertySlim<string> Title { get; }
        public ReadOnlyReactivePropertySlim<List<string>> DisplayingImageFilePaths { get; }

        public ReadOnlyReactivePropertySlim<Visibility> ImagesEmptyTextVisibility { get; }

        public ReactivePropertySlim<Visibility> OverlayVisibility { get; }

        public ReactiveCommand ClearCommand { get; }
        public ReactiveCommand<DragEventArgs> DropCommand { get; }
        public ReactiveCommand<DragEventArgs> PreviewDragOverCommand { get; }
        public ReactiveCommand<DragEventArgs> PreviewDragLeaveCommand { get; }

        public ImagesSourcePickerViewModel(IObservable<List<string>> ImageFilePaths, Action<List<string>> SetImageFilePaths)
        {
            Title = ImageFilePaths
                .Select(x => $"画像枚数 : {x.Count}枚")
                .ToReadOnlyReactivePropertySlim()
                .AddTo(Disposables);
            DisplayingImageFilePaths = ImageFilePaths.Select(x =>
            {
                var count = x.Count;
                if (count > MAX_DISPLAYING_IMAGES)
                {
                    var l = x.GetRange(0, MAX_DISPLAYING_IMAGES);
                    l.Add($"{count - MAX_DISPLAYING_IMAGES} 枚の画像が省略されました。");
                    return l;
                }
                else
                    return x;
            }).ToReadOnlyReactivePropertySlim()
            .AddTo(Disposables);
            ImagesEmptyTextVisibility = ImageFilePaths.Select((x) =>
            {
                if (x.Count == 0)
                    return Visibility.Visible;
                return Visibility.Hidden;
            }).ToReadOnlyReactivePropertySlim()
            .AddTo(Disposables);
            ClearCommand = new ReactiveCommand()
                .WithSubscribe(() => SetImageFilePaths(new List<string> { }))
                .AddTo(Disposables);
            OverlayVisibility = new ReactivePropertySlim<Visibility>(Visibility.Hidden);
            PreviewDragOverCommand = new ReactiveCommand<DragEventArgs>()
                .WithSubscribe((e) =>
                {
                    OverlayVisibility.Value = Visibility.Visible;
                }).AddTo(Disposables);
            PreviewDragLeaveCommand = new ReactiveCommand<DragEventArgs>()
                .WithSubscribe((e) =>
                {
                    OverlayVisibility.Value = Visibility.Hidden;
                }).AddTo(Disposables);
            DropCommand = new ReactiveCommand<DragEventArgs>()
                .WithSubscribe((e) =>
                {
                    OverlayVisibility.Value = Visibility.Hidden;
                    if (!e.Data.GetDataPresent(DataFormats.FileDrop)) // ドロップされたものがファイルかどうか確認する。
                        return;
                    var paths = (string[])e.Data.GetData(DataFormats.FileDrop);
                    SetImageFilePaths(FetchImageFiles(paths));
                })
                .AddTo(Disposables);
        }
        public static List<string> FetchImageFiles(string[] targetPaths)
        {
            var files = new List<string>();
            foreach (var p in targetPaths)
            {
                if (Directory.Exists(p))
                    files.AddRange(Directory.EnumerateFiles(p, "*.*", SearchOption.AllDirectories));
                else if (File.Exists(p))
                    files.Add(p);
            }
            return files.Where(IsImageFile).ToList();
        }

        public static bool IsImageFile(string path)
        {
            var imageExtensions = new List<string> { ".bmp", ".jpeg", ".jpg", ".png", ".tiff", ".gif" };
            return imageExtensions.Contains(Path.GetExtension(path).ToLower());
        }
    }
}

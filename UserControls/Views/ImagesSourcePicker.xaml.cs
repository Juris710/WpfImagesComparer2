using System.Diagnostics;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows;
using UserControls.ViewModels;
using Reactive.Bindings.ObjectExtensions;
using System.Reactive.Linq;
using System.Linq;
using Core;
using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;

using System.IO;
using System.Reactive.Disposables;

namespace UserControls.Views
{
    /// <summary>
    /// Interaction logic for ImagesSourcePicker
    /// </summary>
    public partial class ImagesSourcePicker : UserControl
    {

        public List<string> ImageFilePaths
        {
            get { return (List<string>)GetValue(ImageFilePathsProperty); }
            set
            {
                SetValue(ImageFilePathsProperty, value);
            }
        }
        public static readonly DependencyProperty ImageFilePathsProperty =
            DependencyProperty.Register("ImageFilePaths", typeof(List<string>), typeof(ImagesSourcePicker), new PropertyMetadata(new List<string> { }));

        public ImagesSourcePicker()
        {
            InitializeComponent();

            Root.DataContext = new ImagesSourcePickerViewModel(
                this.ObserveDependencyProperty(ImageFilePathsProperty)
                    .Select(x => ImageFilePaths),
                (newImageFilePaths) => ImageFilePaths = newImageFilePaths
            );
        }
    }
}

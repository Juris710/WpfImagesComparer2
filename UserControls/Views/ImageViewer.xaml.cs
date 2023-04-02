using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace UserControls.Views
{
    /// <summary>
    /// Interaction logic for ImageViewer
    /// </summary>
    public partial class ImageViewer : UserControl
    {
        public string Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(string), typeof(ImageViewer), new PropertyMetadata(""));



        public MatrixTransform Transform
        {
            get { return (MatrixTransform)GetValue(TransformProperty); }
            set { SetValue(TransformProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Transform.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TransformProperty =
            DependencyProperty.Register("Transform", typeof(MatrixTransform), typeof(ImageViewer), new PropertyMetadata(new MatrixTransform()));

        private Point _start;

        public ImageViewer()
        {
            InitializeComponent();
            ImageView.SetBinding(Image.SourceProperty, new Binding(nameof(Source)) { Source = this, Converter = new PathToImageConverter() });
            ImageView.SetBinding(Image.RenderTransformProperty, new Binding(nameof(Transform)) { Source = this });
        }

        private void Container_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _start = e.GetPosition(sender as IInputElement);
            ImageView.CaptureMouse();
        }
        private void Container_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ImageView.ReleaseMouseCapture();
        }

        private void Container_MouseMove(object sender, MouseEventArgs e)
        {
            if (!ImageView.IsMouseCaptured) return;
            var position = e.GetPosition(sender as IInputElement);
            var v = _start - position;
            var matrix = Transform.Value;
            matrix.Translate(-v.X, -v.Y);
            Transform = new MatrixTransform(matrix);
            _start = position;
        }

        private void Container_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var matrix = Transform.Value;
            double scale = 1.2;
            if (e.Delta < 0)
            {
                // 縮小処理
                scale = 1 / scale;
            }
            matrix.ScaleAt(scale, scale, ImageView.ActualWidth / 2, ImageView.ActualHeight / 2);
            Transform = new MatrixTransform(matrix);
        }
    }
}

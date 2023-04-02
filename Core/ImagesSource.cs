using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Core
{
    public class ImagesSource : DisposableBindableBase
    {
        public ReactivePropertySlim<List<string>> ImageFilePaths { get; }

        public ImagesSource()
        {
            ImageFilePaths = new ReactivePropertySlim<List<string>>(new List<string> { }).AddTo(Disposables);
        }
    }
}

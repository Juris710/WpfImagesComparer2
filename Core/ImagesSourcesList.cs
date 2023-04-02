using Core.Properties;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class ImagesSourcesList : DisposableBindableBase
    {
        public ReactiveCollection<ImagesSource> Sources { get; protected set; }

        public ReactivePropertySlim<int> Count { get; protected set; }


        public ImagesSourcesList()
        {
            Sources = new ReactiveCollection<ImagesSource>().AddTo(Disposables);
            Count = new ReactivePropertySlim<int>(Settings.Default.Columns).AddTo(Disposables);
            Count.Subscribe(x =>
            {
                if (Sources.Count < x)
                {
                    Enumerable.Range(0, x - Sources.Count).ToList().ForEach(_ =>
                    {
                        Sources.Add(new ImagesSource());
                    });
                }
                if (Sources.Count > x)
                {
                    Enumerable.Range(0, Sources.Count - x).ToList().ForEach(_ =>
                    {
                        Sources.RemoveAt(Sources.Count - 1);
                    });
                }
            }).AddTo(Disposables);

        }
        public override string ToString()
        {
            return string.Join(",", Sources.Select(x => x.ImageFilePaths.Value.Count.ToString()));
        }
    }
}

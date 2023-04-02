using Core;
using Pages.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows.Media;

namespace Pages.ViewModels
{
    public class SelectImagesSourcesPageViewModel : DisposableBindableBase
    {
        public ReactiveCommand NavigateToMainCommand { get; }

        public ImagesSourcesList ImagesSources { get; }
        public SelectImagesSourcesPageViewModel(IRegionManager regionManager, ImagesSourcesList imagesSources)
        {
            NavigateToMainCommand = new ReactiveCommand()
                .WithSubscribe(
                    () => regionManager.RequestNavigate("ContentRegion", nameof(MainPage))
                ).AddTo(Disposables);
            ImagesSources = imagesSources;
        }
    }
}

using ControlzEx.Theming;
using Core;
using MahApps.Metro.Controls.Dialogs;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Core.Properties;


namespace WpfImagesComparer2.ViewModels
{
    public class MainWindowViewModel : DisposableBindableBase
    {
        public ReactivePropertySlim<bool> IsSettingsFlyoutOpen { get; }
        public ReactiveCommand OpenSettingsCommand { get; }

        public ReactiveCommand ResetSettingsCommand { get; }

        public ReadOnlyReactiveCollection<string> BaseColorSchemes { get; }
        public ReadOnlyReactiveCollection<string> ColorSchemes { get; }

        public ReactiveCommand ShowAttributesCommand { get; }


        public MainWindowViewModel()
        {
            IsSettingsFlyoutOpen = new ReactivePropertySlim<bool>(false).AddTo(Disposables);
            OpenSettingsCommand = new ReactiveCommand().AddTo(Disposables);
            OpenSettingsCommand.Subscribe(() =>
            {
                IsSettingsFlyoutOpen.Value = true;
            });

            BaseColorSchemes = new ObservableCollection<string>(
                ThemeManager.Current.Themes
                    .GroupBy(x => x.BaseColorScheme)
                    .OrderBy(x => x.Key)
                    .Select(x => x.Key)
            ).ToReadOnlyReactiveCollection().AddTo(Disposables);

            ColorSchemes = new ObservableCollection<string>(
                ThemeManager.Current.Themes
                    .GroupBy(x => x.ColorScheme)
                    .OrderBy(x => x.Key)
                    .Select(x => x.Key)
            ).ToReadOnlyReactiveCollection().AddTo(Disposables);

            Settings.Default.ObserveProperty(x => x.BaseColorSchemeIndex)
                .CombineLatest(Settings.Default.ObserveProperty(x => x.ColorSchemeIndex))
                .Subscribe((x) =>
                {
                    var baseColorScheme = BaseColorSchemes[x.First];
                    var colorScheme = ColorSchemes[x.Second];
                    ThemeManager.Current.ChangeTheme(System.Windows.Application.Current, baseColorScheme, colorScheme);
                }
            ).AddTo(Disposables);

            ResetSettingsCommand = new ReactiveCommand().AddTo(Disposables);
            ResetSettingsCommand.Subscribe(() =>
            {
                ConfirmAndResetSettings();
            });

            ShowAttributesCommand = new ReactiveCommand().AddTo(Disposables);
            ShowAttributesCommand.Subscribe(() =>
            {
                Utils.GetMainWindow().ShowMessageAsync("Attributes", "Photo icons created by Freepik - Flaticon\nhttps://www.flaticon.com/free-icons/photo");
            });
        }

        private async void ConfirmAndResetSettings()
        {
            var dialogSettings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "設定をリセット",
                NegativeButtonText = "キャンセル",
            };
            var result = await Utils.GetMainWindow().ShowMessageAsync(
                 "確認",
                 "設定をリセットしますか？",
                 MessageDialogStyle.AffirmativeAndNegative,
                 dialogSettings
             );
            if (result == MessageDialogResult.Affirmative)
            {
                Settings.Default.Reset();
                await Utils.GetMainWindow().ShowMessageAsync("完了", "設定をリセットしました。");
            }
        }
    }
}

namespace SoarCraft.QYun.UnityABStudio.Views {
    using System;
    using Windows.Storage.Pickers;
    using CommunityToolkit.Mvvm.DependencyInjection;
    using Microsoft.UI.Xaml;
    using Microsoft.UI.Xaml.Controls;
    using Serilog;
    using ViewModels;
    using WinRT.Interop;
    using Microsoft.UI.Xaml.Input;
    using System.Threading.Tasks;

    public sealed partial class OverViewPage : Page {
        public OverViewModel ViewModel { get; }

        private readonly ILogger logger = Ioc.Default.GetRequiredService<ILogger>();

        public OverViewPage() {
            this.ViewModel = Ioc.Default.GetService<OverViewModel>();
            this.InitializeComponent();

#if DEBUG
            logger.Debug($"Loading {nameof(OverViewPage)}");
#endif
        }

        private void LoadPanel_OnLoaded(object sender, RoutedEventArgs e) {
            //#if DEBUG
            //_ = ViewModel.BuildTestBundleListAsync().ConfigureAwait(false);
            //#endif

            #region CommandBar

            var openFileCommand = new StandardUICommand(StandardUICommandKind.Open);
            openFileCommand.ExecuteRequested += OpenFileCommandOnExecuteRequested;
            this.OpenFiles.Command = openFileCommand;

            var openFolderCommand = new StandardUICommand(StandardUICommandKind.Open);
            openFolderCommand.ExecuteRequested += OpenFolderCommandOnExecuteRequested;
            this.OpenFolder.Command = openFolderCommand;

            var ejectCommand = new StandardUICommand(StandardUICommandKind.Delete);
            ejectCommand.ExecuteRequested += EjectCommandOnExecuteRequested;
            this.EjectItems.Command = ejectCommand;

            #endregion
        }

        private void EjectCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args) =>
            this.ViewModel.EjectFiles(this.LoadedList.SelectedItems);

        private async void OpenFolderCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args) {
            this.OpenFolder.IsEnabled = false;

            var picker = new FolderPicker {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            picker.FileTypeFilter.Add("*");
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(App.MainWindow));

            var folder = await picker.PickSingleFolderAsync();
            this.OpenFolder.IsEnabled = true;

            if (folder == null)
                return;

            _ = ViewModel.LoadAssetFolderAsync(folder).ConfigureAwait(false);
        }

        private async void OpenFileCommandOnExecuteRequested(XamlUICommand sender, ExecuteRequestedEventArgs args) =>
            await PickABFilesAsync();

        private async void QuickExportButton_OnClick(object sender, RoutedEventArgs e) {
            this.QuickExportButton.IsEnabled = false;
            this.QuickInfoBar.Severity = InfoBarSeverity.Warning;

            #region Debug

            //#if DEBUG
            //            this.logger.Debug($"{this.ViewModel.ExpAnimator}");
            //            this.logger.Debug($"{this.ViewModel.ExpAudioClip}");
            //            this.logger.Debug($"{this.ViewModel.ExpFont}");
            //            this.logger.Debug($"{this.ViewModel.ExpMesh}");
            //            this.logger.Debug($"{this.ViewModel.ExpMonoBehaviour}");
            //            this.logger.Debug($"{this.ViewModel.ExpMovieTexture}");
            //            this.logger.Debug($"{this.ViewModel.ExpShader}");
            //            this.logger.Debug($"{this.ViewModel.ExpSprite}");
            //            this.logger.Debug($"{this.ViewModel.ExpTexture2D}");
            //            this.logger.Debug($"{this.ViewModel.ExpTextAsset}");
            //            this.logger.Debug($"{this.ViewModel.ExpVideoClip}");
            //#endif

            #endregion

            #region PickABFiles

            if (ViewModel.bundleList.Count == 0)
                await PickABFilesAsync();

            if (ViewModel.bundleList.Count == 0) {
                this.QuickInfoBar.Severity = InfoBarSeverity.Informational;
                this.QuickExportButton.IsEnabled = true;
                return;
            }

            #endregion

            #region PickSaveFolder

            var picker = new FolderPicker {
                SuggestedStartLocation = PickerLocationId.Downloads
            };
            picker.FileTypeFilter.Add("*");
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(App.MainWindow));

            var saveFolder = await picker.PickSingleFolderAsync();
            if (saveFolder == null) {
                this.QuickInfoBar.Severity = InfoBarSeverity.Informational;
                this.QuickExportButton.IsEnabled = true;
                return;
            }

            #endregion

            this.QuickText.Text = "导出中，请耐心等待，最多三分钟";
            var res = await this.ViewModel.QuickExportAsync(saveFolder, QuickText).ConfigureAwait(false);

            _ = DispatcherQueue.TryEnqueue(() => {
                if (res > 0) {
                    this.QuickInfoBar.Severity = InfoBarSeverity.Success;
                    this.QuickText.Text = $"成功导出{res}个对象，IO写入还在进行中，请勿立刻关闭程序";
                } else {
                    this.QuickInfoBar.Severity = InfoBarSeverity.Error;
                    this.QuickText.Text = "导出超时，错误详情请查看日志";
                }

                this.QuickExportButton.IsEnabled = true;
            });
        }

        private async Task PickABFilesAsync() {
            this.OpenFiles.IsEnabled = false;

            var picker = new FileOpenPicker {
                SuggestedStartLocation = PickerLocationId.ComputerFolder
            };
            picker.FileTypeFilter.Add(".ab");
            InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(App.MainWindow));

            var abFile = await picker.PickMultipleFilesAsync();
            this.OpenFiles.IsEnabled = true;

            if (abFile.Count == 0)
                return;

            await ViewModel.LoadAssetFilesAsync(abFile);
        }

        #region QuickBoxSet

        private void AnimatorBox_OnChecked(object sender, RoutedEventArgs e) => ViewModel.ExpAnimator = true;

        private void AnimatorBox_OnUnchecked(object sender, RoutedEventArgs e) => ViewModel.ExpAnimator = false;

        private void AudioClipBox_OnChecked(object sender, RoutedEventArgs e) => ViewModel.ExpAudioClip = true;

        private void AudioClipBox_OnUnchecked(object sender, RoutedEventArgs e) => ViewModel.ExpAudioClip = false;

        private void FontBox_OnChecked(object sender, RoutedEventArgs e) => ViewModel.ExpFont = true;

        private void FontBox_OnUnchecked(object sender, RoutedEventArgs e) => ViewModel.ExpFont = false;

        private void MeshBox_OnChecked(object sender, RoutedEventArgs e) => ViewModel.ExpMesh = true;

        private void MeshBox_OnUnchecked(object sender, RoutedEventArgs e) => ViewModel.ExpMesh = false;

        private void MonoBehaviourBox_OnChecked(object sender, RoutedEventArgs e) => ViewModel.ExpMonoBehaviour = true;

        private void MonoBehaviourBox_OnUnchecked(object sender, RoutedEventArgs e) =>
            ViewModel.ExpMonoBehaviour = false;

        private void MovieTextureBox_OnChecked(object sender, RoutedEventArgs e) => ViewModel.ExpMovieTexture = true;

        private void MovieTextureBox_OnUnchecked(object sender, RoutedEventArgs e) => ViewModel.ExpMovieTexture = false;

        private void ShaderBox_OnChecked(object sender, RoutedEventArgs e) => ViewModel.ExpShader = true;

        private void ShaderBox_OnUnchecked(object sender, RoutedEventArgs e) => ViewModel.ExpShader = false;

        private void SpriteBox_OnChecked(object sender, RoutedEventArgs e) => ViewModel.ExpSprite = true;

        private void SpriteBox_OnUnchecked(object sender, RoutedEventArgs e) => ViewModel.ExpSprite = false;

        private void Texture2DBox_OnChecked(object sender, RoutedEventArgs e) => ViewModel.ExpTexture2D = true;

        private void Texture2DBox_OnUnchecked(object sender, RoutedEventArgs e) => ViewModel.ExpTexture2D = false;

        private void TextAssetBox_OnChecked(object sender, RoutedEventArgs e) => ViewModel.ExpTextAsset = true;

        private void TextAssetBox_OnUnchecked(object sender, RoutedEventArgs e) => ViewModel.ExpTextAsset = false;

        private void VideoClipBox_OnChecked(object sender, RoutedEventArgs e) => ViewModel.ExpVideoClip = true;

        private void VideoClipBox_OnUnchecked(object sender, RoutedEventArgs e) => ViewModel.ExpVideoClip = false;

        #endregion
    }
}

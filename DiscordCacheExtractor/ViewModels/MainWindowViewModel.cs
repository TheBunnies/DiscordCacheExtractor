using Avalonia;
using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using Avalonia.VisualTree;
using Avalonia.Controls.ApplicationLifetimes;
using System.Threading.Tasks;
using Avalonia.Controls.Primitives;
using System.IO;
using MessageBox.Avalonia;
using System.Linq;
using MessageBox.Avalonia.Enums;
using MessageBox.Avalonia.DTO;

namespace DiscordCacheExtractor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private int _fileCount;
        public int FileCount
        {
            get => _fileCount;
            set
            {
                this.RaiseAndSetIfChanged(ref _fileCount, value);
            }
        }

        private DateTime _date;
        public DateTime Date
        {
            get => _date;
            set
            {
                this.RaiseAndSetIfChanged(ref _date, value);
            }
        }
        private string _filePath;
        public string FilePath
        {
            get => _filePath;
            set
            {
                this.RaiseAndSetIfChanged(ref _filePath, value);
            }
        }
        public ReactiveCommand<Unit, Unit> BrowseFoldersCommand { get; }
        public ReactiveCommand<Unit, Unit> StartCommand {get;}
        public ReactiveCommand<Unit, Unit> ClearCommand {get;}

        public MainWindowViewModel()
        {
            var dialog = new OpenFolderDialog();
            BrowseFoldersCommand = ReactiveCommand.CreateFromTask(() => OpenDialogAsync());
            StartCommand = ReactiveCommand.CreateFromTask(() => StartAsync(), this.WhenAnyValue(x => x.FilePath, x => !string.IsNullOrWhiteSpace(x)));
            ClearCommand = ReactiveCommand.CreateFromTask(() => ClearAsync());

        }
        private async Task OpenDialogAsync()
        { 
            var dialog = new OpenFolderDialog();
            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                FilePath = await dialog.ShowAsync(desktop.MainWindow);
                var path = GetChachePath();
                FileCount = new DirectoryInfo(path).GetFiles().Length;
            }
        }
        private async Task StartAsync()
        {
            var path = GetChachePath();
            if (path == null)
            {
                await MessageBoxManager
                .GetMessageBoxStandardWindow(new MessageBoxStandardParams{
                    ButtonDefinitions = ButtonEnum.Ok,
                    ContentTitle = "Not found",
                    ContentMessage = "The path to discord chache was not found! Make sure dicsord is installed",
                    Icon = Icon.Database,
                    Style = Style.UbuntuLinux
                }).Show();
                return;
            }
            try
            {
                var files = Directory.GetFiles(path);

                for (int i = 0; i < files.Length; i++)
                {
                    var fileName = Path.GetFileName(files[i]);
                    if (fileName.StartsWith("f") && (File.GetCreationTime(files[i]).Day == Date.Day || Date.Year != DateTime.Now.Year))
                    {
                        File.Copy(files[i], Path.ChangeExtension(Path.Combine(FilePath, fileName), ".png"));
                    }
                }
            }
            catch(Exception ex)
            {
                await MessageBoxManager
                .GetMessageBoxStandardWindow(new MessageBoxStandardParams{
                    ButtonDefinitions = ButtonEnum.Ok,
                    ContentTitle = "Oooops.. Something went wrong",
                    ContentMessage = ex.Message,
                    Icon = Icon.Error,
                    Style = Style.UbuntuLinux
                }).Show();
                return;
            }
            await MessageBoxManager
            .GetMessageBoxStandardWindow(new MessageBoxStandardParams{
                ButtonDefinitions = ButtonEnum.Ok,
                ContentTitle = "Success",
                ContentMessage = "The files were successfully processed",
                Icon = Icon.Info,
                Style = Style.UbuntuLinux
            }).Show();
        }

        private async Task ClearAsync()
        {
            var result = await MessageBoxManager
            .GetMessageBoxStandardWindow(new MessageBoxStandardParams{
                ButtonDefinitions = ButtonEnum.OkAbort,
                ContentTitle = "Warning!",
                ContentMessage = "You really want to do it? \n All of your discord chache will be cleared",
                Icon = Icon.Plus,
                Style = Style.UbuntuLinux
            }).Show();
            if (result == ButtonResult.Ok)
            {
                var path = GetChachePath();
                if (path == null)
                {
                    await MessageBoxManager
                    .GetMessageBoxStandardWindow(new MessageBoxStandardParams{
                        ButtonDefinitions = ButtonEnum.Ok,
                        ContentTitle = "Not found",
                        ContentMessage = "The path to discord chache was not found! Make sure dicsord is installed",
                        Icon = Icon.Database,
                        Style = Style.UbuntuLinux
                    }).Show();
                    return;
                }
                var files = Directory.GetFiles(path);
                for (int i = 0; i < files.Length; i++)
                    File.Delete(files[i]);

                await MessageBoxManager
                    .GetMessageBoxStandardWindow(new MessageBoxStandardParams{
                        ButtonDefinitions = ButtonEnum.Ok,
                        ContentTitle = "Success",
                        ContentMessage = "The discord chache was successfully cleared",
                        Icon = Icon.Info,
                        Style = Style.UbuntuLinux
                    }).Show();
            }
        }
        private string GetChachePath()
        {
            var drives = DriveInfo.GetDrives();
            for (int i = 0; i < drives.Length; i++)
            {
                var path = drives[i].Name + @"Users\" + Environment.UserName + @"\AppData\Roaming\discord\Cache";
                if (Directory.Exists(path))
                {
                    return path;
                }
            }
            return null;
        }
    }

}

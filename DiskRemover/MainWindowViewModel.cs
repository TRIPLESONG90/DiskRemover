using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DiskRemover
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        string? saveDirectory;

        [ObservableProperty]
        int? days;

        [ObservableProperty]
        double? usage;
        CancellationToken token = new CancellationToken();
        Remover _remover;
        AppSetting _setting;
        public MainWindowViewModel(Remover remover, ILogger<MainWindowViewModel> logger, AppSetting setting)
        {
            _remover = remover;
            _setting = setting;
            SetRemover();


            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    await Task.Delay(100);
                    if (SaveDirectory == null)
                        continue;

                    if (!System.IO.Directory.Exists(SaveDirectory))
                        continue;

                    var drive = new System.IO.DriveInfo(SaveDirectory);
                    var usage = drive.TotalSize - drive.AvailableFreeSpace;
                    var usagePercent = (usage / (double)drive.TotalSize) * 100;
                    Usage = Math.Round( usagePercent,2);
                }
            });



            Task.Factory.StartNew(() =>
            {

                remover.StartAsync(token);
            });



            this.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SaveDirectory))
                {
                    _setting.SaveDirectory = SaveDirectory;
                    _setting.Save();
                    SetRemover();
                }
                if (e.PropertyName == nameof(Days))
                {
                    _setting.Days = Days;
                    _setting.Save();
                    SetRemover();
                }
            };
        }


        [RelayCommand]
        private void SelectDirectory()
        {
            var openFolderDialog = new OpenFolderDialog
            {
                Multiselect = false,
                Title = "Select a folder"
            };

            // 대화 상자 표시
            bool? result = openFolderDialog.ShowDialog();

            // 결과 처리
            if (result == true)
            {
                // 선택한 폴더 경로 가져오기
                SaveDirectory = openFolderDialog.FolderName;
                // 폴더 경로 사용 로직 추가
            }
        }
        private void SetRemover()
        {
            _remover.SetDirectory(_setting.SaveDirectory);
            SaveDirectory = _setting.SaveDirectory;
            _remover.SetDays(_setting.Days);
            Days = _setting.Days;
        }
        
    }
}

using Microsoft.Win32;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System;
using Prism.Mvvm;
using Prism.Commands;

namespace CarPaintingProcess.ViewModels
{
    public class DefectDetectionViewModel : BindableBase
    {
        private string _str;
        public string Str
        {
            get { return _str; }
            set { SetProperty(ref _str, value); }
        }

        public ICommand LoadImageCommand { get; private set; }

        // 이미지 소스 필드 및 프로퍼티
        private BitmapImage _DetectImageSource;
        public BitmapImage DetectImageSource
        {
            get { return _DetectImageSource; }
            set 
            { 
                SetProperty(ref _DetectImageSource, value); 
                RaisePropertyChanged(nameof(IsImageLoaded));
            }
        }

        //이미지가 로드되었는지 확인하는 속성
        public bool IsImageLoaded => DetectImageSource == null;

        public DefectDetectionViewModel()
        {
            LoadImageCommand = new DelegateCommand(LoadImage);
        }

        // 이미지 파일 불러오는 Command 및 함수

        private void LoadImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "이미지 파일|*.jpg;*.jpeg;*.png;*.bmp";
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                BitmapImage imageSource = new BitmapImage(new Uri(filePath));
                DetectImageSource = imageSource;
            }
        }
    }
}

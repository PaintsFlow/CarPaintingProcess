using Microsoft.Win32;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System;
using Prism.Mvvm;
using Prism.Commands;
using System.Windows;
using MySqlX.XDevAPI.Common;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using LiveCharts;
using System.Windows.Media;
using LiveCharts.Wpf;
using CarPaintingProcess.Models.Services;
using System.Data;
using System.Collections.ObjectModel;
using CarPaintingProcess.Models;
using System.Security.RightsManagement;
using Google.Protobuf.WellKnownTypes;
using MaterialDesignColors;

namespace CarPaintingProcess.ViewModels
{
    public class DefectDetectionViewModel : BindableBase
    {
        private readonly DBlink _dblink;

        // Command
        public ICommand LoadImageCommand { get; private set; }
        public ICommand DetectColorCommand { get; }


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

        // 감지된 색상 data
        private SolidColorBrush _detectedColor;
        public SolidColorBrush DetectedColor
        {
            get { return _detectedColor; }
            set { SetProperty(ref _detectedColor, value); }
        }

        // 주문 제품 색상 data
        private SolidColorBrush _orderColor;
        public SolidColorBrush OrderColor
        {
            get { return _orderColor; }
            set { SetProperty(ref _orderColor, value); }
        }

        private Vec3b _detectedHSVColor;
        public Vec3b DetectedHSVColor
        {
            get { return _detectedHSVColor; }
            set { SetProperty(ref _detectedHSVColor, value); }
        }
        private Vec3b _orderHSVColor;
        public Vec3b OrderHSVColor
        {
            get { return _orderHSVColor; }
            set { SetProperty(ref _orderHSVColor, value); }
        }


        // 감지된 색상 HSV 값
        private string _detectedHSVText;
        public string DetectedHSVText
        {
            get { return _detectedHSVText; }
            set { SetProperty(ref _detectedHSVText, value); }
        }

        // 주문 색상 HSV 값
        private string _orderHSVText;
        public string OrderHSVText
        {
            get { return _orderHSVText; }
            set { SetProperty(ref _orderHSVText, value); }
        }

        // 결과 : 옆에 적힐 text
        private string _resultText;
        public string ResultText
        {
            get { return _resultText; }
            set { SetProperty(ref _resultText, value); }
        }

        // detected color chart data
        private SeriesCollection _detectedhsvChartData;
        public SeriesCollection DetectedHsvChartData
        {
            get { return _detectedhsvChartData; }
            set { SetProperty(ref _detectedhsvChartData, value); }
        }

        // order color chart data
        private SeriesCollection _orderhsvChartData;
        public SeriesCollection OrderHsvChartData
        {
            get { return _orderhsvChartData; }
            set { SetProperty(ref _orderhsvChartData, value); }
        }

        private ObservableCollection<DefectDetectionModel> _comboboxItem;
        public ObservableCollection<DefectDetectionModel> ComboboxItem
        {
            get { return _comboboxItem; }
            set { SetProperty(ref _comboboxItem, value);
            }
        }

        private DefectDetectionModel _selectedComboboxItem;
        public DefectDetectionModel SelectedComboboxItem
        {
            get { return _selectedComboboxItem; }
            set { SetProperty(ref _selectedComboboxItem, value);}
        }

        public DefectDetectionViewModel()
        {
            _dblink = DBlink.Instance();
            _dblink.Connect();
            ComboboxItem = new ObservableCollection<DefectDetectionModel>();
            ComboboxItemShow();
            LoadImageCommand = new DelegateCommand(LoadImage);
            DetectColorCommand = new DelegateCommand(ColorDetection);
            
        }

        // DB에서 제품 정보 가져오기
        private void ComboboxItemShow()
        {
            if (_dblink.ConnectOk())
            {
                string sql = "SELECT * FROM product p JOIN color c ON p.colorId = c.colorId";
                DataTable results = _dblink.Select(sql);
                ComboboxItem.Clear();

                foreach (DataRow row in results.Rows)
                {
                    var defectdetectionModel = new DefectDetectionModel
                    {
                        productId = Convert.ToInt32(row["productid"]),
                        productName = Convert.ToString(row["productname"]),
                        colorName = Convert.ToString(row["colorName"]),
                        colorH = Convert.ToInt32(row["hue"]),
                        colorS = Convert.ToInt32(row["saturation"]),
                        colorV = Convert.ToInt32(row["value"])
                    };
                    ComboboxItem.Add(defectdetectionModel);
                }
            }
            _dblink.Disconnect();
        }

        private void ProductinfoShow()
        {
            // HSV 값 가져오기
            double h = SelectedComboboxItem.colorH / 2.0; // OpenCV는 Hue를 0~179로 사용
            byte hue = Convert.ToByte(SelectedComboboxItem.colorH);
            double s = SelectedComboboxItem.colorS;
            double v = SelectedComboboxItem.colorV;

            string hsvColor = $"HSV: ({SelectedComboboxItem.colorH}, {SelectedComboboxItem.colorS}, {SelectedComboboxItem.colorV})";
            OrderHSVText = hsvColor;

            // HSV -> RGB 변환
            Mat hsvMat = new Mat(1, 1, MatType.CV_8UC3, new Scalar(h, s, v));
            Mat rgbMat = new Mat();

            Cv2.CvtColor(hsvMat, rgbMat, ColorConversionCodes.HSV2BGR);
            Vec3b rgbcolor = rgbMat.At<Vec3b>(0, 0);
            OrderHSVColor = hsvMat.At<Vec3b>(0, 0);
            
            OrderColor = new SolidColorBrush(Color.FromRgb(rgbcolor.Item2, rgbcolor.Item1, rgbcolor.Item0));
            UpdateHSVGraph("order", hue, OrderHSVColor.Item1, OrderHSVColor.Item2);
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

        private void ColorDetection()
        {
            if (IsImageLoaded)
            {
                MessageBox.Show("검사할 이미지가 없습니다. 이미지를 업로드 해주세요.","검사 실패", MessageBoxButton.OK ,MessageBoxImage.Warning);
                return;
            }

            if (SelectedComboboxItem == null)
            {
                MessageBox.Show("검사할 차량 종류를 선택해 주세요.", "검사 실패", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }          

            //productinfoshow
            ProductinfoShow();

            DetectedInfoShow();
            // 색상 비교
            ColorComparision();
        }
        
        private void DetectedInfoShow()
        {
            Mat image = DetectImageSource.ToMat();

            if (image.Empty())
            {
                MessageBox.Show("이미지 로드에 실패했습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            int centerX = image.Width / 2;
            int centerY = image.Height / 2;

            // 중앙 픽셀의 BGR 색상 추출
            Vec3b centralPixel = image.At<Vec3b>(centerY, centerX);
            byte blue = centralPixel.Item0;
            byte green = centralPixel.Item1;
            byte red = centralPixel.Item2;

            // HSV 변환
            Mat hsvImage = new Mat();
            Cv2.CvtColor(image, hsvImage, ColorConversionCodes.BGR2HSV);
            DetectedHSVColor = hsvImage.At<Vec3b>(centerY, centerX);
            double h = Convert.ToDouble(DetectedHSVColor.Item0) * 2;
            byte byteh = Convert.ToByte(h);

            DetectedColor = new SolidColorBrush(Color.FromRgb(red, green, blue));
            UpdateHSVGraph("detected", byteh, DetectedHSVColor.Item1, DetectedHSVColor.Item2);

            // 결과 텍스트
            string hsvColor = $"HSV: ({byteh}, {DetectedHSVColor.Item1}, {DetectedHSVColor.Item2})";
            DetectedHSVText = hsvColor;

        }
        private void ColorComparision()
        {
            int hueDiff = Math.Abs(OrderHSVColor.Item0 - DetectedHSVColor.Item0);
            int saturationDiff = Math.Abs(OrderHSVColor.Item1 - DetectedHSVColor.Item1);
            int valueDiff = Math.Abs(OrderHSVColor.Item2 - DetectedHSVColor.Item2);

            const int HUE_THRESHOLD = 5;  
            const int SV_THRESHOLD = 30;   

            bool isHsvMatch = hueDiff < HUE_THRESHOLD && saturationDiff < SV_THRESHOLD && valueDiff < SV_THRESHOLD;
            Console.WriteLine($"hueDiff : {hueDiff}, saturationDiff : {saturationDiff}, valueDiff: {valueDiff}, 결과: {isHsvMatch}");
            if (isHsvMatch) ResultText = "합격";
            else ResultText = "불합격";
        }

        private void UpdateHSVGraph(string kind,byte hue, byte saturation, byte value)
        {
            if (kind == "detected")
            {
                DetectedHsvChartData = new SeriesCollection
                {
                    new ColumnSeries { Title = "Hue", Values = new ChartValues<double> { hue }, Fill = Brushes.Red },
                    new ColumnSeries { Title = "Saturation", Values = new ChartValues<double> { saturation }, Fill = Brushes.Green },
                    new ColumnSeries { Title = "Value", Values = new ChartValues<double> { value }, Fill = Brushes.Blue }
                };
            }
            else
            {
                OrderHsvChartData = new SeriesCollection
                {
                    new ColumnSeries { Title = "Hue", Values = new ChartValues<double> { hue }, Fill = Brushes.Red },
                    new ColumnSeries { Title = "Saturation", Values = new ChartValues<double> { saturation }, Fill = Brushes.Green },
                    new ColumnSeries { Title = "Value", Values = new ChartValues<double> { value }, Fill = Brushes.Blue }
                };
            }
        }
    }
}

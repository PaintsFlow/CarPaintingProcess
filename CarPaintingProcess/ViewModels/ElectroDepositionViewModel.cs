using Prism.Mvvm;
using CarPaintingProcess.Models.Services;
using System.Threading.Tasks;
using CarPaintingProcess.Models;
using System;
using System.Collections.ObjectModel;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;
using System.Windows.Shapes;
using System.Reflection;
using ImTools;
using System.Windows.Threading;

namespace CarPaintingProcess.ViewModels
{
    public class ElectroDepositionViewModel : BindableBase
    {
        private readonly ConnectBrokerModel _connectBrokerModel;

        private string _str;
        public string Str
        {
            get { return _str; }
            set { SetProperty(ref _str, value); }
        }

        private SeriesCollection _waterlevelData;
        public SeriesCollection WaterlevelData
        {
            get { return _waterlevelData; }
            set { SetProperty(ref _waterlevelData, value); }
        }

        private SeriesCollection _viscosityData;
        public SeriesCollection ViscosityData
        {
            get { return _viscosityData; }
            set { SetProperty(ref _viscosityData, value); }
        }

        private SeriesCollection _phData;
        public SeriesCollection pHData
        {
            get { return _phData; }
            set { SetProperty(ref _phData, value); }
        }

        private SeriesCollection _currentData;
        public SeriesCollection CurrentData
        {
            get { return _currentData; }
            set { SetProperty(ref _currentData, value); }
        }

        private SeriesCollection _voltageData;
        public SeriesCollection VoltageData
        {
            get { return _voltageData; }
            set { SetProperty(ref _voltageData, value); }
        }
        public Func<double, string> TimeFormatter { get; set; }

        public ElectroDepositionViewModel()
        {
            _connectBrokerModel = ConnectBrokerModel.GetInstance();
            _connectBrokerModel.MessageReceived += OnMessageReceived;
            _connectBrokerModel.Consumerfunc();

            WaterlevelData = InitializeChartSeries("Water Level");
            ViscosityData = InitializeChartSeries("Viscosity");
            pHData = InitializeChartSeries("pH");
            CurrentData = InitializeChartSeries("Current");
            VoltageData = InitializeChartSeries("Voltage");
            TimeFormatter = value => DateTime.FromOADate(value).ToString("HH:mm:ss");

        }

        private SeriesCollection InitializeChartSeries(string title)
        {
            return new SeriesCollection
            {
                new LineSeries
                {
                    Title = title,
                    Values = new ChartValues<ObservablePoint>()
                }
            };
        }

        private void OnMessageReceived(string message) {

            _str = message;
            string[] value = _str.Split(',');

            if (value.Length < 6)
                return; // 데이터가 부족하면 오류 방지

            // 데이터 파싱
            double[] parsedValues = new double[5]; // 첫 번째 데이터는 time이므로 제외
            for (int i = 0; i < 5; i++) // 인덱스 수정 (0부터 4까지)
            {
                if (!double.TryParse(value[i + 1], out parsedValues[i]))
                    parsedValues[i] = 0; // 변환 실패 시 기본값
            }

            var time = Convert.ToDateTime(value[0]);
            double timeIndex = time.ToOADate(); // X축 값을 OLE 날짜 형식으로 변환

            // LiveCharts 데이터 추가
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                UpdateChartData(WaterlevelData, parsedValues[0], timeIndex);
                UpdateChartData(ViscosityData, parsedValues[1], timeIndex);
                UpdateChartData(pHData, parsedValues[2], timeIndex);
                UpdateChartData(CurrentData, parsedValues[3], timeIndex);
                UpdateChartData(VoltageData, parsedValues[4], timeIndex);

            }), DispatcherPriority.Background);
        }

        private void UpdateChartData(SeriesCollection chartData, double value, double timeIndex)
        {
            if (chartData == null || chartData.Count == 0)
                return;

            var lineSeries = chartData[0] as LineSeries;
            if (lineSeries == null || lineSeries.Values == null)
                return;

            var values = lineSeries.Values as ChartValues<ObservablePoint>;
            if (values == null)
                return;

            values.Add(new ObservablePoint(timeIndex, value));

            // 오래된 데이터 삭제 (60개 이상이면 삭제)
            if (values.Count > 60)
            {
                values.RemoveAt(0);
            }
        }

    }
}



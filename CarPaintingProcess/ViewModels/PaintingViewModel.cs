﻿using ImTools;
using LiveCharts;
using Prism.Mvvm;
using RabbitMQ.Client.Events;
using CarPaintingProcess.Models.Services;
using System;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System.Windows.Threading;

namespace CarPaintingProcess.ViewModels
{
    public class PaintingViewModel : BindableBase
    {
        private readonly ConnectBrokerModel _connectBrokerModel;
        private string _str;
        public string Str
        {
            get { return _str; }
            set { SetProperty(ref _str, value); }
        }

        private SeriesCollection _paintflowData;
        public SeriesCollection PaintflowData
        {
            get { return _paintflowData; }
            set { SetProperty(ref _paintflowData, value); }
        }

        private SeriesCollection _airspraypressureData;
        public SeriesCollection AirsprayPressureData
        {
            get { return _airspraypressureData; }
            set { SetProperty(ref _airspraypressureData, value); }
        }

        public Func<double,string> TimeFormatter { get; set; }

        public PaintingViewModel()
        {
            _connectBrokerModel = ConnectBrokerModel.GetInstance();
            _connectBrokerModel.MessageReceived += OnMessageReceived;
            _connectBrokerModel.Consumerfunc();

            PaintflowData = InitializeChartSeries("Paint Flow");
            AirsprayPressureData = InitializeChartSeries("AirSpray Pressure");
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

        private void OnMessageReceived(string message)
        {

            _str = message;
            string[] value = _str.Split(',');

            if (value.Length < 10)
                return; // 데이터가 부족하면 오류 방지

            // 데이터 파싱
            double[] parsedValues = new double[2]; // 첫 번째 데이터는 time이므로 제외
            for (int i = 0; i < 2; i++) // 인덱스 수정 (0부터 4까지)
            {
                if (!double.TryParse(value[i+8], out parsedValues[i]))
                    parsedValues[i] = 0; // 변환 실패 시 기본값
            }

            var time = Convert.ToDateTime(value[0]);
            double timeIndex = time.ToOADate(); // X축 값을 OLE 날짜 형식으로 변환

            // LiveCharts 데이터 추가
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                UpdateChartData(AirsprayPressureData, parsedValues[0], timeIndex);
                UpdateChartData(PaintflowData, parsedValues[1], timeIndex);
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

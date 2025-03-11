using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using OfficeOpenXml;
using Prism.Commands;
using Prism.Mvvm;
using System.IO;
using System;
using System.ComponentModel;
using CarPaintingProcess.Models.Services;
using System.Collections.ObjectModel;
using CarPaintingProcess.Models;
using System.Data;
using System.Threading.Tasks; 

namespace CarPaintingProcess.ViewModels
{
    public class SearchViewModel : BindableBase
    {
        private readonly DBlink _dbLink;
        private string _str;
        public string Str
        {
            get { return _str; }
            set { SetProperty(ref _str, value); }
        }

        private bool _isSearchEnabled = true;
        public bool IsSearchEnabled
        {
            get { return _isSearchEnabled; }
            set { SetProperty(ref _isSearchEnabled, value); }
        }

        // 선택된 날짜
        private DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get { return _selectedDate; }
            set { SetProperty(ref _selectedDate, value); }
        }

        // 테이블에 넣을 data
        private ObservableCollection<DryModel> _dryData;
        public ObservableCollection<DryModel> DryData
        {
            get { return _dryData; }
            set { SetProperty(ref _dryData, value); }
        }

        private ObservableCollection<ElectroDepositionModel> _electroDepositionData;
        public ObservableCollection<ElectroDepositionModel> ElectroDepositionData
        {
            get { return _electroDepositionData; }
            set { SetProperty(ref _electroDepositionData, value); }
        }

        private ObservableCollection<PaintingModel> _paintingData;
        public ObservableCollection<PaintingModel> PaintingData
        {
            get { return _paintingData; }
            set { SetProperty(ref _paintingData, value); }
        }

        private ObservableCollection<AlarmModel> _AlarmData;
        public ObservableCollection<AlarmModel> AlarmData
        {
            get { return _AlarmData; }
            set { SetProperty(ref _AlarmData, value); }
        }

        // 명령 바인딩
        public ICommand LoadDataCommand { get; }
        public ICommand SaveExcelCommand { get; }

        public SearchViewModel()
        {
            _dbLink = DBlink.Instance();

            // 비동기용 커맨드 바인딩
            LoadDataCommand = new DelegateCommand(async () => await LoadDataAsync());
            SaveExcelCommand = new DelegateCommand(DataToExcel);

            DryData = new ObservableCollection<DryModel>();
            PaintingData = new ObservableCollection<PaintingModel>();
            ElectroDepositionData = new ObservableCollection<ElectroDepositionModel>();
            AlarmData = new ObservableCollection<AlarmModel>();
        }

        private async Task LoadDataAsync()
        {
            if (!IsSearchEnabled) return;

            // 버튼 비활성화
            IsSearchEnabled = false;

            try
            {
                if (SelectedDate == null)
                {
                    MessageBox.Show("날짜를 선택해주세요", "날짜 선택", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string selectedDatestr = SelectedDate?.ToString("yyyy-MM-dd");
                string startDatestr = selectedDatestr + " 00:00:00";
                string endDatestr = selectedDatestr + " 23:59:59";

                await Task.Run(() =>
                {
                    _dbLink.Connect();
                    if (!_dbLink.ConnectOk()) return;

                    string sql = $"SELECT * FROM dry WHERE time BETWEEN '{startDatestr}' AND '{endDatestr}' ORDER BY time";
                    DataTable results = _dbLink.Select(sql);

                    // 가져온 데이터는 임시 리스트에 저장
                    var tempDryData = new ObservableCollection<DryModel>();
                    foreach (DataRow row in results.Rows)
                    {
                        tempDryData.Add(new DryModel
                        {
                            time = Convert.ToDateTime(row["time"]),
                            temperature = Convert.ToDouble(row["temperature"]),
                            humidity = Convert.ToDouble(row["humidity"])
                        });
                    }

                    sql = $"SELECT * FROM electroDeposition WHERE time BETWEEN '{startDatestr}' AND '{endDatestr}' ORDER BY time";
                    results = _dbLink.Select(sql);

                    var tempElectroData = new ObservableCollection<ElectroDepositionModel>();
                    foreach (DataRow row in results.Rows)
                    {
                        tempElectroData.Add(new ElectroDepositionModel
                        {
                            time = Convert.ToDateTime(row["time"]),
                            waterlevel = Convert.ToDouble(row["waterlevel"]),
                            viscosity = Convert.ToDouble(row["viscosity"]),
                            ph = Convert.ToDouble(row["pH"]),
                            current = Convert.ToDouble(row["current"]),
                            voltage = Convert.ToDouble(row["voltage"])
                        });
                    }
                    sql = $"SELECT * FROM paint WHERE time BETWEEN '{startDatestr}' AND '{endDatestr}' ORDER BY time";
                    results = _dbLink.Select(sql);

                    var tempPaintingData = new ObservableCollection<PaintingModel>();
                    foreach (DataRow row in results.Rows)
                    {
                        tempPaintingData.Add(new PaintingModel
                        {
                            time = Convert.ToDateTime(row["time"]),
                            paintamount = Convert.ToDouble(row["paintamount"]),
                            pressure = Convert.ToDouble(row["pressure"])
                        });
                    }
                    sql = $"SELECT * FROM alarm WHERE time BETWEEN '{startDatestr}' AND '{endDatestr}' ORDER BY time";
                    results = _dbLink.Select(sql);

                    var tempAlarmData = new ObservableCollection<AlarmModel>();
                    foreach (DataRow row in results.Rows)
                    {
                        tempAlarmData.Add(new AlarmModel
                        {
                            alarmid = Convert.ToInt32(row["alarmid"]),
                            time = Convert.ToDateTime(row["time"]),
                            sensor = Convert.ToString(row["sensor"]),
                            data = Convert.ToDouble(row["data"]),
                            message = Convert.ToString(row["message"])
                        });
                    }

                    _dbLink.Disconnect(); // DB 연결 해제

                    // UI 스레드에서 안전하게 컬렉션 갱신
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        DryData.Clear();
                        foreach (var item in tempDryData) DryData.Add(item);

                        ElectroDepositionData.Clear();
                        foreach (var item in tempElectroData) ElectroDepositionData.Add(item);

                        PaintingData.Clear();
                        foreach (var item in tempPaintingData) PaintingData.Add(item);

                        AlarmData.Clear();
                        foreach (var item in tempAlarmData) AlarmData.Add(item);
                    });
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터 조회 중 오류가 발생했습니다. \n{ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // 조회 버튼 다시 활성화
                IsSearchEnabled = true;
            }
        }

        // 엑셀로 저장하기
        private void DataToExcel()
        {
            try
            {
                string filePath = GetSaveFilePath("SensorData.xlsx");
                if (string.IsNullOrEmpty(filePath))
                {
                    MessageBox.Show("파일 저장이 취소되었습니다.", "저장 취소", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                // Excel 파일 생성 및 DataGrid 데이터 추가
                using (ExcelPackage package = new ExcelPackage())
                {
                    bool hasData = false; // 데이터가 있는지 체크

                    hasData |= AddSheetToExcel(package, "하도 전착 공정", ElectroDepositionData);
                    hasData |= AddSheetToExcel(package, "건조 공정", DryData);
                    hasData |= AddSheetToExcel(package, "도장 공정", PaintingData);

                    // 최소한 하나의 시트가 존재해야 함
                    if (!hasData)
                    {
                        MessageBox.Show("저장할 데이터가 없습니다.", "데이터 없음", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    File.WriteAllBytes(filePath, package.GetAsByteArray());

                    MessageBox.Show($"파일이 저장되었습니다. \n\n파일 경로 : {filePath}",
                        "저장 완료", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"오류 발생 : {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // DataGrid 데이터를 Excel Sheet에 추가하는 메서드
        private bool AddSheetToExcel<T>(ExcelPackage package, string sheetName, ObservableCollection<T> data)
        {
            if (data == null || data.Count == 0)
                return false;

            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(sheetName);
            var properties = typeof(T).GetProperties();

            // 컬럼 헤더 작성
            for (int i = 0; i < properties.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = properties[i].Name; // 속성 이름을 컬럼 헤더로 사용
            }

            // 데이터 행 작성
            int rowIndex = 2;
            foreach (var item in data)
            {
                for (int colIndex = 0; colIndex < properties.Length; colIndex++)
                {
                    var value = properties[colIndex].GetValue(item)?.ToString() ?? "";
                    worksheet.Cells[rowIndex, colIndex + 1].Value = value;
                }
                rowIndex++;
            }
            return true;
        }

        // 파일 저장 경로 선택 메서드
        private string GetSaveFilePath(string defaultFileName)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = defaultFileName,
                DefaultExt = ".xlsx",
                Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*"
            };

            return saveFileDialog.ShowDialog() == true ? saveFileDialog.FileName : null;
        }
    }
}

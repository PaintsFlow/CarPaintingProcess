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

        // 선택된 날짜
        private DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get { return _selectedDate; }
            set { SetProperty(ref _selectedDate, value); }
        }

        //테이블에 넣을 data
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

        public ICommand LoadDataCommand { get; }
        public ICommand SaveExcelCommand { get; }


        public SearchViewModel()
        {
            _dbLink = DBlink.Instance();
            SaveExcelCommand = new DelegateCommand(DataToExcel);
            LoadDataCommand = new DelegateCommand(LoadData);
            DryData = new ObservableCollection<DryModel>();
            PaintingData = new ObservableCollection<PaintingModel>();
            ElectroDepositionData = new ObservableCollection<ElectroDepositionModel>();

        }

        // DB에서 select 해오기
        private void LoadData()
        {
            _dbLink.Connect();

            if (SelectedDate == null)
            {
                MessageBox.Show("날짜를 선택해주세요", "날짜 선택", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else
            {
                string selectedDatestr = SelectedDate?.ToString("yyyy-MM-dd");
                string startDatestr = selectedDatestr + " 00:00:00";
                string endDatestr = selectedDatestr + " 23:59:59";

                if (_dbLink.ConnectOk())
                {
                    //건조
                    
                    string sql = $"SELECT * FROM dry WHERE time BETWEEN '{startDatestr}' AND '{endDatestr}' ORDER BY time";

                    DataTable results = _dbLink.Select(sql);
                    DryData.Clear();

                    foreach (DataRow row in results.Rows)
                    {
                        var dryModel = new DryModel
                        {
                            time = Convert.ToDateTime(row["time"]),
                            temperature = Convert.ToDouble(row["temperature"]),
                            humidity = Convert.ToDouble(row["humidity"])
                        };
                        DryData.Add(dryModel);
                    }

                    //하도전착
                    sql = $"SELECT * FROM electroDeposition WHERE time BETWEEN '{startDatestr}' AND '{endDatestr}' ORDER BY time";

                    results = _dbLink.Select(sql);
                    ElectroDepositionData.Clear();

                    foreach (DataRow row in results.Rows)
                    {
                        var electrodepositionModel = new ElectroDepositionModel
                        {
                            time = Convert.ToDateTime(row["time"]),
                            waterlevel = Convert.ToDouble(row["waterlevel"]),
                            viscosity = Convert.ToDouble(row["viscosity"]),
                            ph = Convert.ToDouble(row["pH"]),
                            current = Convert.ToDouble(row["current"]),
                            voltage = Convert.ToDouble(row["voltage"])
                        };
                        ElectroDepositionData.Add(electrodepositionModel);
                    }

                    //도장 공정
                    sql = $"SELECT * FROM paint WHERE time BETWEEN '{startDatestr}' AND '{endDatestr}' ORDER BY time";

                    results = _dbLink.Select(sql);
                    PaintingData.Clear();

                    foreach (DataRow row in results.Rows)
                    {
                        var paintingModel = new PaintingModel
                        {
                            time = Convert.ToDateTime(row["time"]),
                            paintamount = Convert.ToDouble(row["paintamount"]),
                            pressure = Convert.ToDouble(row["pressure"])
                        };
                        PaintingData.Add(paintingModel);
                    }
                }
            }
            _dbLink.Disconnect();
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

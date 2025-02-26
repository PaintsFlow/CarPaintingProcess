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

namespace CarPaintingProcess.ViewModels
{
    public class SearchViewModel : BindableBase
    {
        private string _str;
        public string Str
        {
            get { return _str; }
            set { SetProperty(ref _str, value); }
        }

        // datagrid 리스트 (각각의 표를 저장할 datagrid)
        private DataGrid _electroDepositionTable;
        public DataGrid ElectroDepositionTable
        {
            get { return _electroDepositionTable; }
            set { SetProperty(ref _electroDepositionTable, value); }
        }

        private DataGrid _dryTable;
        public DataGrid DryTable
        {
            get { return _dryTable; }
            set { SetProperty(ref _dryTable, value); }
        }

        private DataGrid _paintingTable;
        private DataGrid PaintingTable
        {
            get { return _paintingTable; }
            set { SetProperty(ref _paintingTable, value); }
        }

        public ICommand SaveExcelCommand { get; }

        public SearchViewModel()
        {
            SaveExcelCommand = new DelegateCommand(DataToExcel);
        }

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

                // Excel 파일 생성 및 DataGrid 데이터 추가
                using (ExcelPackage package = new ExcelPackage())
                {
                    AddSheetToExcel(package, "하도 전착 공정", ElectroDepositionTable);
                    AddSheetToExcel(package, "건조 공정", DryTable);
                    AddSheetToExcel(package, "도장 공정", PaintingTable);

                    // 파일 저장
                    File.WriteAllBytes(filePath, package.GetAsByteArray());

                }
                
                MessageBox.Show($"파일이 저장되었습니다. \n\n파일 경로 : {filePath}",
                    "저장 완료", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            catch (Exception ex)
            {
                MessageBox.Show($"오류 발생 : {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // DataGrid 데이터를 Excel Sheet에 추가하는 메서드
        private void AddSheetToExcel(ExcelPackage package, string sheetName, DataGrid dataGrid)
        {
            if (dataGrid?.ItemsSource == null)
                return;

            var worksheet = package.Workbook.Worksheets.Add(sheetName);
            var headers = dataGrid.Columns.Select(col => col.Header.ToString()).ToList();

            // 컬럼 헤더 작성
            for (int i = 0; i < headers.Count; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
            }

            // 데이터 행 작성
            int rowIndex = 2;
            foreach (var item in dataGrid.ItemsSource)
            {
                for (int colIndex = 0; colIndex < dataGrid.Columns.Count; colIndex++)
                {
                    var binding = dataGrid.Columns[colIndex].ClipboardContentBinding as System.Windows.Data.Binding;
                    if (binding != null)
                    {
                        var propertyName = binding.Path.Path;
                        var property = item.GetType().GetProperty(propertyName);
                        worksheet.Cells[rowIndex, colIndex + 1].Value = property?.GetValue(item)?.ToString() ?? "";
                    }
                }
                rowIndex++;
            }
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

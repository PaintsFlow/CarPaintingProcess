using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using DryIoc;
using CarPaintingProcess.ViewModels;


namespace CarPaintingProcess.Views.Controls
{
    /// <summary>
    /// DefectDetectionView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DefectDetectionView : UserControl
    {
        public DefectDetectionView()
        {
            InitializeComponent();
            DataContext = new DefectDetectionViewModel();
        }

        //private void ImageButton_Click(object sender, RoutedEventArgs e)
        //{
        //    string imgpath;
        //    OpenFileDialog openFileDialog = new OpenFileDialog();
        //    openFileDialog.Multiselect = false;
        //    openFileDialog.Filter = "JPEG, JPG files (*.jpeg 혹은 *.jpg)|*.jpeg;*.jpg|PNG files (*.png)|*.png|All files (*.*)|*.*";

        //    if (openFileDialog.ShowDialog() == true)
        //    {
        //        imgpath = openFileDialog.FileName;
        //        DetectImage.Source = new BitmapImage(new Uri(imgpath));

        //    }



        //}
    }
}

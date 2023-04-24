using EM.Bases;
using EM.WpfBases;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EM.Tools
{
    public class MainViewModel : ViewModel<MainWindow>
    {
        private string directory;
        /// <summary>
        /// 目录
        /// </summary>
        public string Directory
        {
            get { return directory; }
            set { SetProperty(ref directory, value); }
        }

        private string outPath;
        /// <summary>
        /// 输出目录
        /// </summary>
        public string OutPath
        {
            get { return outPath; }
            set { SetProperty(ref outPath, value); }
        }
        /// <summary>
        /// 选择目录
        /// </summary>
        public DelegateCommand SelectDirectoryCmd { get; }
        /// <summary>
        /// 保存
        /// </summary>
        public DelegateCommand SaveCmd { get; }
        /// <summary>
        /// 开始
        /// </summary>
        public DelegateCommand StartCmd { get; }
        public MainViewModel(MainWindow t) : base(t)
        {
            SelectDirectoryCmd = new DelegateCommand(SelectDirectory);
            SaveCmd = new DelegateCommand(Save);
            StartCmd = new DelegateCommand(Start);
        }

        private void Start()
        {
            var window = Window.GetWindow(View);
            if (string.IsNullOrEmpty(Directory))
            {
                MessageBox.Show(window, "目录不能为空");
                return;
            }
            if (string.IsNullOrEmpty(OutPath))
            {
                MessageBox.Show(window, "保存路径不能为空");
                return;
            }
            XSSFWorkbook sheets = new XSSFWorkbook();
            var sheet = sheets.CreateSheet();
            int rowIndex = 0;
            var firstRow = sheet.CreateRow(rowIndex++);
            string[] cols = { "序号", "路径" };
            for (int i = 0; i < cols.Length; i++)
            {
                var cell = firstRow.CreateCell(i);
                cell.SetCellValue(cols[i]);
            }
            WriteContents(sheet, directory, ref rowIndex);
            var destDirectory = Path.GetDirectoryName(OutPath);
            if (!System.IO.Directory.Exists(destDirectory))
            {
                System.IO.Directory.CreateDirectory(destDirectory);
            }
            if (File.Exists(OutPath))
            {
                File.Delete(OutPath);
            }
            using FileStream fs = File.OpenWrite(OutPath);
            sheets.Write(fs);
            MessageBox.Show(window, "成功");
        }
        private void WriteContents(ISheet sheet, string destDirectory, ref int rowIndex)
        {
            var files = System.IO.Directory.GetFiles(destDirectory);
            foreach (var file in files)
            {
                var row = sheet.CreateRow(rowIndex++);
                var cell0 = row.CreateCell(0);
                cell0.SetCellValue(rowIndex-1);
                var cell1 = row.CreateCell(1);
                cell1.SetCellValue(file.Replace($@"{Directory}\",""));
            }
            var directories = System.IO.Directory.GetDirectories(destDirectory);
            foreach (var item in directories)
            {
                WriteContents(sheet, item, ref rowIndex);
            }
        }
        private void Save()
        {
            SaveFileDialog dg = new SaveFileDialog()
            {
                Filter = "*.xlsx|*.xlsx"
            };
            if (dg.ShowDialog() == true)
            {
                OutPath = dg.FileName;
            }
        }

        private void SelectDirectory()
        {
            var commonOpenFileDialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };
            if (commonOpenFileDialog.ShowDialog(Window.GetWindow(View)) == CommonFileDialogResult.Ok)
            {
                Directory = commonOpenFileDialog.FileName;
            }
        }
    }
}

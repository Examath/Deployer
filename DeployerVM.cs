using CommunityToolkit.Mvvm.ComponentModel;
using Deployer.Properties;
using Examath.Core.Environment;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.ObjectModel;

namespace Deployer
{
    internal class DeployerVM : ObservableObject
    {
        private string _BomLocation = "";
        /// <summary>
        /// Gets or sets the location of the excel BoM
        /// </summary>
        public string BomLocation
        {
            get => _BomLocation;
            set => SetProperty(ref _BomLocation, value);
        }

        private string _FeedLocation = "";
        /// <summary>
        /// Gets or sets the location of the STL output directory
        /// </summary>
        public string FeedLocation
        {
            get => _FeedLocation;
            set => SetProperty(ref _FeedLocation, value);
        }

        private string _MaterialFilter = "PETG";
        /// <summary>
        /// Gets or sets the text that the material of the part must contain to be included
        /// </summary>
        public string MaterialFilter
        {
            get => _MaterialFilter;
            set => SetProperty(ref _MaterialFilter, value);
        }


        public ObservableCollection<Part> AvailableParts { get; private set; } = new();
        public ObservableCollection<Part> SelectedParts { get; private set; } = new();

        private string _OutputDirectory;
        /// <summary>
        /// Gets or sets 
        /// </summary>
        public string OutputDirectory
        {
            get => _OutputDirectory;
            set => SetProperty(ref _OutputDirectory, value);
        }

        private string _Suffix = " Dx";
        /// <summary>
        /// Gets or sets 
        /// </summary>
        public string Suffix
        {
            get => _Suffix;
            set => SetProperty(ref _Suffix, value);
        }


        public DeployerVM()

        {
            _OutputDirectory = Settings.Default.LastOutputDirectory;
        }



        public bool Load()
        {
            // getStr bom
            if (BomLocation == string.Empty)
            {
                OpenFileDialog openBomDialog = new()
                {
                    Title = "Open BoM as exported from Inventor",
                    Filter = "Excel|*.xlsx|All files|*.*",
                };

                if (Settings.Default.LastBomLocation != string.Empty)
                {
                    openBomDialog.InitialDirectory = Path.GetDirectoryName(Settings.Default.LastBomLocation);
                }

                if (openBomDialog.ShowDialog() == DialogResult.OK)
                {
                    BomLocation = openBomDialog.FileName;
                    Settings.Default.LastBomLocation = BomLocation;
                }
                else
                {
                    return false;
                }
            }

            // getStr directory
            {
                VistaFolderBrowserDialog folderBrowserDialog = new()
                {
                    Description = "Select directory with all the exported StlPaths",
                };

                if (Settings.Default.LastDirectory != string.Empty)
                {
                    folderBrowserDialog.SelectedPath = Settings.Default.LastDirectory;
                }

                if (folderBrowserDialog.ShowDialog() == true)
                {
                    FeedLocation = folderBrowserDialog.SelectedPath;
                    Settings.Default.LastDirectory = FeedLocation;
                }
            }

            // Cache stl filenames

            string[] StlPaths =  Directory.GetFiles(FeedLocation);
            Dictionary<string, string> feedstock = new();
            foreach (string path in StlPaths)
            {
                string[] pams = Path.GetFileNameWithoutExtension(path).Split("_");
                if (pams.Length == 3 && !feedstock.ContainsKey(pams[1]))
                {
                    feedstock.Add(pams[1], path);
                }
            }

            // Open and import bom

            try
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Open(BomLocation, false))
                {
                    IEnumerable<Sheet>? sheets = document.WorkbookPart?.Workbook.Descendants<Sheet>().Where(s => s.Name == "BOM");
                    if (sheets == null || sheets.Count() == 0)
                    {
                        // The specified worksheet does not exist.
                        return false;
                    }

                    string? sheetID = sheets.First().Id?.Value;
                    if (sheetID == null) return false;
                    WorksheetPart? worksheetPart = (WorksheetPart?)document.WorkbookPart?.GetPartById(sheetID);
                    if (worksheetPart == null) return false;
                    SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
                    SharedStringTable? sharedStringTable = document.WorkbookPart?.GetPartsOfType<SharedStringTablePart>().FirstOrDefault()?.SharedStringTable;
                    if (sharedStringTable == null) return false;

                    Dictionary<string, string> header = new();

                    foreach (Cell cell in sheetData.Elements<Row>().First().Elements<Cell>())
                    {
                        header.Add(
                            cell.CellReference?.Value?[..1] ?? "Z",
                            GetStringCellValue(cell, sharedStringTable)
                            );
                    }

                    foreach (Row r in sheetData.Elements<Row>().Skip(1))
                    {
                        Part part = new(header, r, sharedStringTable, feedstock);

                        if (part.Material != null && part.Material.StartsWith(MaterialFilter))
                        {
                            SelectedParts.Add(part);
                        }
                        else
                        {
                            AvailableParts.Add(part);
                        }
                        
                    }
                }
            }
            catch (Exception e)
            {
#if DEBUG
                throw;
#endif
                Messager.OutException(e, "Opening BoM");
                return false;
            }

            return true;
        }

        public static string GetStringCellValue(Cell cell, SharedStringTable sharedStringTable)
        {
            if (cell.CellValue is CellValue cellValue)
            {
                return sharedStringTable.ElementAt(int.Parse(cellValue.InnerText)).InnerText;
            }
            else return "ErrCellValueNull";
        }

        public bool Deploy()
        {
            Settings.Default.LastOutputDirectory = OutputDirectory;
            foreach (Part part in SelectedParts)
            {
                if (part.MatchedFile != null)
                {
                    File.Copy(part.MatchedFile, $"{OutputDirectory}/{part.OutputName}{Suffix}.stl");
                }
            }
            return true;
        }
    }
}

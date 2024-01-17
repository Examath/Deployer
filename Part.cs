using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace Deployer
{
    internal class Part
    {
        public string? TargetFilename { get; private set; }

        public string? TargetPartNumber { get; private set; }

        public int Quantity { get; private set; } = -404;

        public string? TargetDescription { get; private set; }


        public string Revision { get; private set; } = "1";

        public double Mass { get; private set; } = -404;

        public string? Material { get; private set; }

        public string? MatchedFile { get; private set; }

        public bool IsFileMatched { get => MatchedFile != null; }

        public string PartNumber { get; private set; } = "Error";

        public string OutputName { get; private set; }

        internal Part(Dictionary<string, string> header, Row r, SharedStringTable sharedStringTable, Dictionary<string, string> feedstock)
        {            
            foreach (Cell c in r.Elements<Cell>()) 
            {
                string col = c.CellReference?.Value?[..1] ?? "A";
                string? heading = header.GetValueOrDefault(col);
                if (heading == null) continue;
                string value = DeployerVM.GetStringCellValue(c, sharedStringTable);

                switch (heading)
                {
                    case "Filename":
                        TargetFilename = value;
                        break;
                    case "Part Number":
                        TargetPartNumber = value;
                        break;
                    case "QTY":
                        if (int.TryParse(c.InnerText, out int q)) Quantity = q;
                        break;
                    case "Description":
                        TargetDescription = value;
                        break;
                    case "REV":
                        Revision = value;
                        break;
                    case "Mass":
                        if (double.TryParse(value.Replace(" kg", ""), out double m)) Mass = m * 1000;
                        break;
                    case "Material":
                        Material = value;
                        break;
                }
            }

            if (TargetPartNumber != null)
            {
                PartNumber = TargetPartNumber.Insert(Math.Min(9, TargetPartNumber.Length - 1), Revision);
            }

            OutputName = $"{PartNumber} x{Quantity} {Mass}g";

            if (TargetFilename != null)
            {
                string memberName = TargetFilename.Split('.')[0];
                MatchedFile = feedstock.GetValueOrDefault(memberName);
            }
        }

        public override string ToString()
        {
            return OutputName;
        }
    }
}

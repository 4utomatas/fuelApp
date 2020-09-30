using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// for the methods
using System.IO;
using Microsoft.AspNetCore.Hosting;
using kuras.Models;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;

namespace kuras.Methods
{
    class GlobalMethods
    {
        // Creates a file(from the uploaded) in the specified folder on server
        public static Models.GasStationFile UploadFile (kuras.ViewModels.FileUploadViewModel model, string folderName, string webroot)
        {
            if (model.gsReport != null)
            {
                string uniqueFileName, filePath;
                string uploadsFolder = Path.Combine(webroot, folderName);
                string temp_string_date = GetDate();

                uniqueFileName = temp_string_date + "__" + model.gsReport.FileName;
                filePath = Path.Combine(uploadsFolder, uniqueFileName);
                //Create a file
                var fileCreator = new FileStream(filePath, FileMode.Create);
                model.gsReport.CopyTo(fileCreator);
                fileCreator.Close();
                //Put it on the database
                GasStationFile fileObject = new GasStationFile
                {
                    FileName = uniqueFileName,
                    Uploaded = false
                };
                return fileObject;
            }
            return null;
        }
        // Creates date for the file when uploading it
        private static string GetDate()
        {
            //Get todays date to make a unique file name
            DateTime a = DateTime.Now;

            char[] date_now = new char[a.ToString().Length];
            int ind = 0;
            foreach (char b in a.ToString())
            {
                if (b == ' ' || b == '/')
                {
                    date_now[ind] = '_';
                }
                else if (b == ':')
                {
                    date_now[ind] = ';';
                }
                else
                {
                    date_now[ind] = b;
                }
                ind++;
            }
            string string_date = new string(date_now);
            //end of unique date

            return string_date;
        }
        // Deletes a file in a selected folder
        public static bool DeleteFile(string filePath, string folderPath)
        {
            filePath = Path.Combine(folderPath, filePath);
            if(File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            else
            {
                return false;
            }
        }
        // Prints report
        public static string PrintAll(List<Report> reports, List<Car> cars, List<Card> cards, List<Employee> employees, string webroot)
        {
            var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Visi");

            //headers
            ws.Cell(1, 1).Value = "Kortelė";
            ws.Cell(1, 2).Value = "Kortele D";
            ws.Cell(1, 3).Value = "Vardas";
            ws.Cell(1, 4).Value = "Pavardė";
            ws.Cell(1, 5).Value = "Masina";
            ws.Cell(1, 6).Value = "Num.";
            ws.Cell(1, 7).Value = "Miestas";
            ws.Cell(1, 8).Value = "Ltr.";
            ws.Cell(1, 9).Value = "L/100km";
            ws.Cell(1, 10).Value = "km";
            ws.Cell(1, 11).Value = "km GPS";
            ws.Cell(1, 12).Value = "Suma";
            ws.Cell(1, 13).Value = "Skola";
            ws.Cell(1, 14).Value = "Limit";


            DateTime date1 = DateTime.Today;
            var reportsArray = reports.ToArray();
            string data = date1.Year + date1.Month.ToString("00");
            int i = 2;
            int max = -100;
            foreach (var el in reportsArray)
            {
                
                //ws.Cell(i, 1).Value = el.Card;
                var card = cards.FirstOrDefault(x => x.Id == el.Card);
                if(card != null)
                {
                    ws.Cell(i, 1).Value = card.Number;
                    ws.Cell(i, 2).Value = card.NumberD;
                    ws.Cell(i, 14).Value = card.Limit;
                    ws.Cell(i, 13).Value = (el.Suma - card.Limit > 0) ? Math.Round(el.Suma - card.Limit, 2) : 0;
                    var employee = employees.FirstOrDefault(x => x.Id == card.Emp);
                    if (employee != null)
                    {
                        ws.Cell(i, 3).Value = employee.Fname;
                        ws.Cell(i, 4).Value = employee.Lname;
                        ws.Cell(i, 7).Value = employee.City;
                        var car = cars.FirstOrDefault(x => x.Id == card.Car);
                        if (car != null)
                        {
                            var makeModel = car.Make + " " + car.Model;
                            if (makeModel.Length > max)
                                max = makeModel.Length;
                            ws.Cell(i, 5).Value = makeModel;
                            ws.Cell(i, 6).Value = car.NumberPlate;

                        }
                    }
                    
                }
                
                //ws.Cell(i, 6).Value = el.getCity();
                //var kuras = el.k.FirstOrDefault(x => x.data == data);
                ws.Cell(i, 8).Value = el.L;
                ws.Cell(i, 9).Value = el.Km > 0 ? (Math.Round((el.L / el.Km) * 100, 1)).ToString() : "N/A";
                ws.Cell(i, 10).Value = Math.Round(el.Km);
                ws.Cell(i, 11).Value = Math.Round(el.Km_gps);
                ws.Cell(i, 12).Value = Math.Round(el.Suma, 2);
                
                
                i++;
                
            }


            var firstCell = ws.FirstCellUsed();
            var lastCell = ws.LastCellUsed();
            var range = ws.Range(firstCell.Address, lastCell.Address);
            var table = range.CreateTable();

            //header style
            var rngHeaders = table.Range("A1:N1"); // The address is relative to rngTable (NOT the worksheet)
            rngHeaders.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;


            //atskiru stulpeliu style
            ws.Columns().AdjustToContents();
            ws.Column(1).Width = 8;
            ws.Column(1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            //ws.Column(4).Width = 8;
            ws.Column(5).Width = max + 4;
            ws.Column(6).Width = 8;
            ws.Column(6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Column(7).Width = 10;
            ws.Columns(8, 13).Width = 7;
            ws.Columns(8, 9).Style.NumberFormat.NumberFormatId = 2;
            ws.Columns(10, 11).Style.NumberFormat.NumberFormatId = 1;
            ws.Columns(12, 13).Style.NumberFormat.NumberFormatId = 2;
            ws.Column(13).Style.Font.Bold = true;
            ws.PageSetup.Margins.Left = 0.4;
            ws.PageSetup.Margins.Right = 0.4;
            ws.PageSetup.SetRowsToRepeatAtTop(1, 1);
            ws.PageSetup.PagesWide = 1;
            var fileName = reportsArray[0].Date + "-Report.xlsx";
            fileName = Path.Combine("Summary", fileName);
            // add to database record of file here
            //
            //

            var filePath = Path.Combine(webroot, fileName);
            workbook.SaveAs(@filePath);
            return fileName;
            //paleidzia faila
            //System.Diagnostics.Process.Start(@"Results.xlsx");
        }
        // Returns the last sevent characters of the string(cardNumber)
        public static string ConvertCardNumber(string cardNumber)
        {
            return (7 > cardNumber.Length ? cardNumber : cardNumber.Substring(cardNumber.Length - 7));
        }
        // Creates a Models.Car from AxaptaModels.DsAutoShort
        public static Car ConvertToCar(kuras.AxaptaModels.DsAutoShort model)
        {
            string _make = "";
            string _model = "";
            SeparateMakeModel(model.MakeModel, ref _make, ref _model);

            Car car = new Car
            {
                Model = _model,
                Make = _make,
                NumberPlate = model.NumberPlate
            };
            return car;
        }
        // Separates the makeModel into make and model retrieved from Axapta Ds Auto Table
        public static void SeparateMakeModel(string makeAndModel, ref string make, ref string model)
        {
            string[] makeModel = makeAndModel.Split(' ');
            int m_id = 0;
            make = "";
            model = "";
            foreach (string m in makeModel)
            {
                if (m_id == 0)
                {
                    make = m;
                }
                else
                {
                    model += m + " ";
                }
                m_id++;
            }
            model = model.Trim(' ');
        }

        // Creates a Models.Card card
        public static Card ConvertToCard(kuras.AxaptaModels.DsAutoKort model)
        {
            var card = new Card
            {
                Limit = 100,
                Number = ConvertCardNumber(model.CardNumber),
                NumberD = model.CardNumberD
            };
            return card;
        }
        // Creates a Models.Card card with a car.ID
        public static Card ConvertToCard(kuras.AxaptaModels.DsAutoKort model, int car)
        {
            var card = new Card
            {
                Limit = 100,
                Number = ConvertCardNumber(model.CardNumber),
                NumberD = model.CardNumberD,
                Car = car
            };
            return card;
        }
    }
}

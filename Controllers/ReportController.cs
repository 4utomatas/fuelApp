using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using kuras.Data;
using kuras.Models;
// excel
using ClosedXML.Excel;
// file
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;

namespace kuras.Controllers
{
    public class ReportController : Controller
    {
        private readonly kurasContext _context;
        private readonly IWebHostEnvironment webhostEnvironment;

        public ReportController(kurasContext context, IWebHostEnvironment webhostEnvironment)
        {
            _context = context;
            this.webhostEnvironment = webhostEnvironment;
        }
        #region auto generated
        // GET: Report
        public async Task<IActionResult> Index()
        {
            return View(await _context.Report.ToListAsync());
        }

        // GET: Report/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.Report
                .FirstOrDefaultAsync(m => m.Id == id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // GET: Report/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Report/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Card,Suma,Km,Km_gps,L,Start,End")] Report report)
        {
            if (ModelState.IsValid)
            {
                _context.Add(report);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(report);
        }

        // GET: Report/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.Report.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }
            return View(report);
        }

        // POST: Report/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Card,Suma,Km,Km_gps,L,Start,End")] Report report)
        {
            if (id != report.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(report);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReportExists(report.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(report);
        }

        // GET: Report/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.Report
                .FirstOrDefaultAsync(m => m.Id == id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // POST: Report/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var report = await _context.Report.FindAsync(id);
            _context.Report.Remove(report);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReportExists(int id)
        {
            return _context.Report.Any(e => e.Id == id);
        }
        #endregion

        // GET DATA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReport([Bind("Date, Odo, Gps, GasStation")]
        kuras.ViewModels.ReportCreateViewModel model)
        {
            if(!ModelState.IsValid)
            {
                return NotFound();
            }

            System.Diagnostics.Debug.WriteLine("Date is {0}, the odo id is: {1} and GPS is: {2}", model.Date, model.Odo, model.Gps);

            if (model.Odo == 0 || model.Gps == 0)
            {
                return NotFound();
            }

            var Reports = await ReturnFormattedReport(model);

            if (Reports.Any())
            {
                foreach (var el in Reports)
                    _context.Add(el);
                await _context.SaveChangesAsync();
                return View(Reports);
            }
                
            else return NotFound();
        }
        
        public async Task<IActionResult> SelectExcel()
        {
            return View(await _context.ReportFile.ToListAsync());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateExcel([Bind("Date")] string date)
        {
            var reports = await _context.Report.ToListAsync();
            var reportsByDate = reports.Where(x => x.Date == date).ToList();
            var employees = await _context.Employee.ToListAsync();
            var cards = await _context.Card.ToListAsync();
            var cars = await _context.Car.ToListAsync();

            string webroot = webhostEnvironment.WebRootPath;
            string fileName = Methods.GlobalMethods.PrintAll(reportsByDate, cars, cards, employees, webroot);
            //Download method
            await AddFile(fileName);

            byte[] fileData = new byte[0];
            string contentType = null;
            ConvertToDownloadable(fileName, ref fileData, ref contentType);
            return File(fileData, contentType, fileName);
        }
        public async Task<IActionResult> DownloadExcel(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reportFile = await _context.ReportFile
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reportFile == null)
            {
                return NotFound();
            }
            byte[] fileData = new byte[0];
            string contentType = null;
            ConvertToDownloadable(reportFile.FileName, ref fileData, ref contentType);
            return File(fileData, contentType, reportFile.FileName);
        }



        // private methods
        private async Task<List<Report>> ReturnFormattedReport(kuras.ViewModels.ReportCreateViewModel model)
        {
            var gpsfile = await _context.KilometersFile
                .FirstOrDefaultAsync(m => m.Id == model.Gps);
            var odofile = await _context.KilometersFile
               .FirstOrDefaultAsync(m => m.Id == model.Odo);
            var gasStationFile = await _context.GasStationFile
               .FirstOrDefaultAsync(m => m.Id == model.GasStation);

            var cards = await _context.Card.ToListAsync();
            var cars = await _context.Car.ToListAsync();
            //if no data in that id 
            if (gpsfile == null || odofile == null || gasStationFile == null)
            {
                return null;
            }

            List<Report> Reports = new List<Report>();

            if (gpsfile.FileName != null && odofile.FileName != null &&
                model.Date != null && gasStationFile.FileName != null)
            {
                var FileDataGps = ReturnFileDataGps(gpsfile.FileName);
                var FileDataOdo = new List<ViewModels.KilometersOdoFileViewModel>();
                var FileDataRF = ReturnFileDataOdoRF(odofile.FileName, ref FileDataOdo);
                var FileDataGS = ReturnFileDataGasStation(gasStationFile.FileName);
                if (FileDataGps.Any() && FileDataGS.Any() && FileDataOdo.Any())
                {
                    foreach (var odo in FileDataOdo)
                    {
                        if (odo.Period == model.Date)
                        {
                            // Find the Gps kilometers for the number plate
                            var gps = FileDataGps.FirstOrDefault(x => x.NumberPlate == odo.NumberPlate);
                            // finds the car object where the number plate is equal
                            var cardNumber = cards.FirstOrDefault(x => x.Number == odo.NumberPlate);
                            // calculates driven kilometers from the odometer
                            double km = odo.End - odo.Start;
                            // separates the date
                            var date1 = model.Date.Split('-');
                            if (date1.Length > 1)
                            {
                                if (Int32.TryParse(date1[0], out int year) &&
                                    Int32.TryParse(date1[1], out int month))
                                {
                                    var remfuel = FileDataRF.FirstOrDefault(x => x.NumberPlate == odo.NumberPlate
                                    && x.Month == month && x.Year == year);
                                    string numberplate = odo.NumberPlate;

                                    int carId = ReturnCarId(cars, numberplate);
                                    int cardId = -1;
                                    if (carId != -1)
                                        cardId = ReturnCardId(cards, carId);

                                    // finds the gas station data
                                    ViewModels.GasStationFileViewModel gsData = null;
                                    if (cardId != -1)
                                    {
                                        var card = cards.FirstOrDefault(x => x.Id == cardId);

                                        if (Int32.TryParse(card.Number, out int cardnumber))
                                            gsData = FileDataGS.FirstOrDefault(x => Int32.Parse(x.CardNumber) == cardnumber);
                                    }
                                    // creates a report about a single car/card
                                    if (gps == null)
                                    {
                                        gps = new ViewModels.KilometersGpsFileViewModel
                                        {
                                            DeviceKm = 0
                                        };
                                    }
                                    if (remfuel == null)
                                    {
                                        remfuel = new RemainingFuel
                                        {
                                            StartFuel = 0,
                                            EndFuel = 0
                                        };
                                    }
                                    if (gsData == null)
                                    {
                                        gsData = new ViewModels.GasStationFileViewModel
                                        {
                                            L = 0,
                                            Sum = 0
                                        };
                                    }
                                    Report rep = new Report
                                    {
                                        NumberPlate = odo.NumberPlate,
                                        Km = km,
                                        Km_gps = gps.DeviceKm,
                                        Date = model.Date,
                                        Start = remfuel.StartFuel,
                                        End = remfuel.EndFuel,
                                        Card = cardId,
                                        L = Math.Round(gsData.L, 2),
                                        Suma = Math.Round(gsData.Sum, 2)
                                    };
                                    Reports.Add(rep);
                                }
                            }
                        }
                    }
                }
            }
            return Reports;
        }
        private async Task AddFile(string fileName)
        {
            var fileObject = new ReportFile
            {
                FileName = fileName
            };
            _context.Add(fileObject);
            await _context.SaveChangesAsync();
        }
        private void ConvertToDownloadable(string fileName,
            ref byte[] fileData, ref string contentType)
        {
            string webroot = webhostEnvironment.WebRootPath;
            string filePath = Path.Combine(webroot, fileName);
            fileData = System.IO.File.ReadAllBytes(filePath);
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out contentType))
            {
                contentType = "application/octet-stream";
            }

        }
        private List<ViewModels.KilometersGpsFileViewModel> ReturnFileDataGps(string FileName)
        {
            List<ViewModels.KilometersGpsFileViewModel> FileData = new List<ViewModels.KilometersGpsFileViewModel>();

            if (FileName != null)
            {
                string uploadsFolder = Path.Combine(webhostEnvironment.WebRootPath, "kilometers");
                string filePath = Path.Combine(uploadsFolder, FileName);
                var wb = new XLWorkbook(filePath);
                var ws = wb.Worksheet(1);
                var device = 1;
                var kms = 2;
                var dataRow = ws.Row(6).RowUsed();

                List<ViewModels.KilometersGpsFileViewModel> Devices = new List<ViewModels.KilometersGpsFileViewModel>();

                while (!dataRow.Cell(device).IsEmpty())
                {
                    string dname = dataRow.Cell(device).GetString();
                    bool kmb = dataRow.Cell(kms).TryGetValue(out double tempkm);
                    dname = dname.Trim(' ');
                    var splitd = dname.Split(' ');
                    for (int i = 0; i < splitd.Length; i++)
                    {
                        splitd[i] = splitd[i].Trim(' ');
                    }
                    string np = "";
                    string name = "";
                    switch (splitd.Length)
                    {
                        case 1:
                            np = splitd[0];
                            break;
                        case 2:
                            if (splitd[0].Length == 3 && splitd[1].Length == 3)
                                np = splitd[0] + splitd[1];
                            else if (splitd[0].Length == 6)
                            {
                                np = splitd[0];
                                name = splitd[1];
                            }
                            break;
                        case 3:
                            if (splitd[0].Length == 3)
                            {
                                np = splitd[0] + splitd[1];
                                name = splitd[2];
                            }
                            else if (splitd[0].Length == 6)
                            {
                                np = splitd[0];
                                name = splitd[1] + "_" + splitd[2];
                            }
                            break;
                        case 4:
                            if (splitd[0].Length == 3 && splitd[1].Length == 3)
                            {
                                np = splitd[0] + splitd[1];
                                name = splitd[2] + "_" + splitd[3];
                            }
                            break;
                        default:
                            np = new System.Text.StringBuilder("").AppendFormat("length: {0} ", splitd.Length).ToString();
                            name = "n/a";
                            break;
                    }

                    if (kmb)
                    {
                        Devices.Add(
                            new ViewModels.KilometersGpsFileViewModel
                            {
                                DeviceName = dname,
                                DeviceKm = tempkm,
                                Name = name,
                                NumberPlate = np
                            });
                    }
                    dataRow = dataRow.RowBelow();
                }
                FileData = Devices;
            }
            //its view is returned  
            //if it is empty, empty list is returned
            return FileData;
        }
        private List<ViewModels.GasStationFileViewModel> ReturnFileDataGasStation(string FileName)
        {
            List<ViewModels.GasStationFileViewModel> FileData = new List<ViewModels.GasStationFileViewModel>();
            if (FileName != null)
            {
                string uploadsFolder = Path.Combine(webhostEnvironment.WebRootPath, "reports");
                string filePath = Path.Combine(uploadsFolder, FileName);
                string[] GsReportText = System.IO.File.ReadAllLines(filePath);
                foreach (string line in GsReportText)
                {
                    string[] cols = line.Split('\t');
                    for (int i = 0; i < cols.Length; i++)
                    {
                        cols[i] = cols[i].Trim(' ');
                    }
                    string cardNo = 7 > cols[0].Length ? cols[0] : cols[0].Substring(cols[0].Length - 7);
                    int indexOfCard = FileData.FindIndex(x => x.CardNumber == cardNo);

                    if (indexOfCard != -1)
                    {
                        //if the code of the item that was bought is a fuel type:
                        if (int.Parse(cols[6]) < 100)
                        {
                            FileData[indexOfCard].L += float.Parse(cols[9], System.Globalization.CultureInfo.InvariantCulture);
                            FileData[indexOfCard].Sum += float.Parse(cols[10], System.Globalization.CultureInfo.InvariantCulture);
                        }
                    }
                    else
                    {
                        FileData.Add(new ViewModels.GasStationFileViewModel
                        {

                            //CardNumber = cols[0].Substring(Math.Max(0, cols[0].Length - 4)),
                            //7 > cols[0].length ? cols[0] : cols[0].Substring(cols[0].length - 7)
                            //4 > mystring.length ? mystring : mystring.Substring(mystring.length -4);
                            //mystring.Substring(Math.Max(0, mystring.Length - 4))

                            CardNumber = 7 > cardNo.Length ? cardNo : cardNo.Substring(cardNo.Length - 7),
                            Sum = float.Parse(cols[10], System.Globalization.CultureInfo.InvariantCulture),
                            L = float.Parse(cols[9], System.Globalization.CultureInfo.InvariantCulture)
                        });
                    }
                }

            }
            //its view is returned  
            //if it is empty, empty list is returned
            return FileData;
        }
        private List<RemainingFuel> ReturnFileDataOdoRF(string FileName,
           ref List<ViewModels.KilometersOdoFileViewModel> Odo)
        {
            // List<ViewModels.KilometersOdoFileViewModel> FileData = new List<ViewModels.KilometersOdoFileViewModel>();
            var FileData = new List<RemainingFuel>();
            if (FileName != null)
            {
                #region excel
                string uploadsFolder = Path.Combine(webhostEnvironment.WebRootPath, "kilometers");
                string filePath = Path.Combine(uploadsFolder, FileName);
                var wb = new XLWorkbook(filePath);
                var ws = wb.Worksheet(1);
                #endregion

                #region worksheet cell numbers
                // where data starts (worksheet cell numbers)
                var numberplate = 5;
                var period = 3;
                var datastart = 6;
                var dataRow = ws.Row(datastart).RowUsed();
                // odo kilometers
                var start = 44;
                var end = 45;
                
                // Diesel 22 / 23
                // gasoline 30 / 31
                var dieselStart = 22;
                var dieselEnd = 23;
                var gasolineStart = 30;
                var gasolineEnd = 31;
                #endregion

                var CarList = _context.Car.ToList();

                List<RemainingFuel> fuelList = new List<RemainingFuel>();
                List<ViewModels.KilometersOdoFileViewModel> Devices = 
                    new List<ViewModels.KilometersOdoFileViewModel>();
                while (!dataRow.Cell(numberplate).IsEmpty())
                {

                    Car carFound = null;
                    string lastNP = null;
                    string tempnb = dataRow.Cell(numberplate).GetString();

                    if (lastNP != tempnb)
                    {
                        lastNP = tempnb;
                        carFound = CarList.Find(x => x.NumberPlate == tempnb);
                    }
                    if (carFound != null)
                    {
                        // benzoline and diesel
                        bool ds = dataRow.Cell(dieselStart).TryGetValue(out float dStart);
                        bool de = dataRow.Cell(dieselEnd).TryGetValue(out float dEnd);
                        bool bs = dataRow.Cell(gasolineStart).TryGetValue(out float bStart);
                        bool be = dataRow.Cell(gasolineEnd).TryGetValue(out float bEnd);
                        // date/period
                        string tempp = dataRow.Cell(period).GetString();
                        // remaining fuel
                        if (ds && de && bs && be)
                        {
                            string[] periodSplit = tempp.Split('-');
                            if (periodSplit.Length == 2)
                            {
                                for (int i = 0; i < periodSplit.Length; i++)
                                    periodSplit[i] = periodSplit[i].Trim(' ');

                                var yearb = Int32.TryParse(periodSplit[0], out int year);
                                var monthb = Int32.TryParse(periodSplit[1], out int month);

                                if (yearb && monthb)
                                {
                                    if (dStart != 0 || dEnd != 0)
                                        fuelList.Add(new RemainingFuel
                                        {
                                            Month = month,
                                            Year = year,
                                            NumberPlate = tempnb,
                                            StartFuel = dStart,
                                            EndFuel = dEnd,
                                            Car = carFound.Id
                                        });
                                    else if (bStart != 0 || bEnd != 0)
                                        fuelList.Add(new RemainingFuel
                                        {
                                            Month = month,
                                            Year = year,
                                            NumberPlate = tempnb,
                                            StartFuel = bStart,
                                            EndFuel = bEnd,
                                            Car = carFound.Id
                                        });
                                    else
                                    {
                                        fuelList.Add(new RemainingFuel
                                        {
                                            Month = month,
                                            Year = year,
                                            NumberPlate = tempnb,
                                            StartFuel = 0,
                                            EndFuel = 0,
                                            Car = carFound.Id
                                        });
                                    }
                                }
                            }
                        }
                        // odo kilometers
                        bool startb = dataRow.Cell(start).TryGetValue(out double temps);
                        bool endb = dataRow.Cell(end).TryGetValue(out double tempe);
                        if(startb && endb)
                        {
                            Devices.Add(new ViewModels.KilometersOdoFileViewModel
                            {
                                Period = tempp,
                                NumberPlate = tempnb,
                                Start = temps,
                                End = tempe
                            });
                        }
                    }
                    // next row
                    dataRow = dataRow.RowBelow();
                }
                // end of while
                FileData = fuelList;
                Odo = Devices;
            }
            //its view is returned  
            //if it is empty, empty list is returned
            
            return FileData;
        }
        private int ReturnCarId(List<Car> cars, string np)
        {
            //string np;
            for (int i = 0; i < cars.Count; i++)
            {
                if (cars[i].NumberPlate == np)
                {
                    return cars[i].Id;
                }
            }
            return -1;
        }
        private int ReturnCardId(List<Card> cards, int car)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].Car == car)
                {
                    return cards[i].Id;
                }
            }
            return -1;
        }
    }
}

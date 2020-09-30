using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using kuras.Data;
using kuras.Models;
//For the file upload
using System.IO;
using Microsoft.AspNetCore.Hosting;
//closedXML
using ClosedXML.Excel;
// diagnostics
using System.Diagnostics;
namespace kuras.Controllers
{
    public class RemainingFuelController : Controller
    {
        private readonly kurasContext _context;
        private readonly IWebHostEnvironment webhostEnvironment;
        public RemainingFuelController(kurasContext context,
         IWebHostEnvironment webhostEnvironment)
        {
            _context = context;
            this.webhostEnvironment = webhostEnvironment;
        }
        // GET: GasStation
        public async Task<IActionResult> Index()
        {
            return View(await _context.KilometersFile.Where(x => x.Gps == false).ToListAsync());
        }
        public IActionResult Create()
        {
            return View();
        }
        // GET DATA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id, gsReport")] kuras.ViewModels.FileUploadViewModel model)
        {
            if (ModelState.IsValid)
            {
                string webroot = webhostEnvironment.WebRootPath;
                GasStationFile fileObj = Methods.GlobalMethods.UploadFile(model, "remainingfuel", webroot);
                RemainingFuelFile rfObj = new RemainingFuelFile{
                    FileName = fileObj.FileName,
                    Uploaded = false
                };
                if (rfObj != null)
                {
                    _context.Add(rfObj);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Show", new { id = rfObj.Id });
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return NotFound();
            }
        }
        
        public async Task<IActionResult> Show(int? id)
        {
            //if no such id
            if (id == null)
            {
                return NotFound();
            }

            var kilometersOdoFile = await _context.KilometersFile
                .FirstOrDefaultAsync(m => m.Id == id);
            
            //if no data in that id 
            if (kilometersOdoFile == null)
            {
                return NotFound();
            }

            if (kilometersOdoFile.FileName != null)
            {
                var FileData = ReturnFileData(kilometersOdoFile.FileName);                
                ViewBag.FileName = kilometersOdoFile.FileName;
                //if file is retrieved, its view is returned
                return View(FileData);
            }                  
            else
            {
                return NotFound();
            }
        }
        private List<RemainingFuel> ReturnFileData(string FileName)
        {
            // List<ViewModels.KilometersOdoFileViewModel> FileData = new List<ViewModels.KilometersOdoFileViewModel>();
            var FileData = new List<RemainingFuel>();
            if (FileName != null)
            {
                Stopwatch wholeFunction = Stopwatch.StartNew();
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

                // Diesel 22 / 23
                // gasoline 30 / 31
                var dieselStart = 22;
                var dieselEnd = 23;
                var gasolineStart = 30;
                var gasolineEnd = 31;
                #endregion

                var CarList = _context.Car.ToList();

                List<RemainingFuel> fuelList = new List<RemainingFuel>();

                while (!dataRow.Cell(numberplate).IsEmpty())
                {
                    Stopwatch whileOne = Stopwatch.StartNew();

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
                    }
                    // next row
                    dataRow = dataRow.RowBelow();
                    whileOne.Stop();                  
                    Debug.WriteLine("{0} ticks - {1} ", whileOne.ElapsedTicks, tempnb);
                                     
                }
                // end of while
                FileData = fuelList;
                wholeFunction.Stop();
                var WholeFunctionTime = wholeFunction.ElapsedMilliseconds;
                Debug.WriteLine("Function took {0} millsecs to complete", WholeFunctionTime);
            }
            //its view is returned  
            //if it is empty, empty list is returned
            return FileData;           
        }
    
    }
}

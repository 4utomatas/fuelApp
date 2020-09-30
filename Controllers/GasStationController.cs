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

namespace kuras.Controllers
{
    public class GasStationController : Controller
    {
        private readonly kurasContext _context;
        private readonly IWebHostEnvironment webhostEnvironment;

        public GasStationController(kurasContext context, IWebHostEnvironment webhostEnvironment)
        {
            _context = context;
            this.webhostEnvironment = webhostEnvironment;
        }

        // INSERT DATA
        // GET: GasStation/Insert/5
        public async Task<IActionResult> Insert(int? id)
        {
            //if no such id
            if (id == null)
            {
                return NotFound();
            }

            var gasStationFile = await _context.GasStationFile
                .FirstOrDefaultAsync(m => m.Id == id);
            //if no data in that id 
            if (gasStationFile == null)
            {
                return NotFound();
            }

            if(gasStationFile.FileName != null)
            {
                //returns
                var Data = GasStationReport(gasStationFile.FileName);
                if (Data.Any())
                {
                    foreach(var el in Data)
                    {
                        _context.Add(el);
                    }
                    gasStationFile.Uploaded = true;
                    _context.Update(gasStationFile);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    return NotFound();
                }

            }
            return View();
        }


        // SHOW DATA
        // GET: GasStation/Show/5
        public async Task<IActionResult> Show(int? id)
        {
            //if no such id
            if (id == null)
            {
                return NotFound();
            }

            var gasStationFile = await _context.GasStationFile
                .FirstOrDefaultAsync(m => m.Id == id);
            
            //if no data in that id 
            if (gasStationFile == null)
            {
                return NotFound();
            }

            if (gasStationFile.FileName != null)
            {
                var FileData = ReturnFileData(gasStationFile.FileName);                
                ViewBag.FileName = gasStationFile.FileName;
                ViewBag.Id = gasStationFile.Id;
                //if file is retrieved, its view is returned
                return View(FileData);
            }                  
            else
            {
                return NotFound();
            }
        }
        public async Task<IActionResult> ShowReport(int? id)
        {
            //if no such id
            if (id == null)
            {
                return NotFound();
            }

            var gasStationFile = await _context.GasStationFile
                .FirstOrDefaultAsync(m => m.Id == id);

            //if no data in that id 
            if (gasStationFile == null)
            {
                return NotFound();
            }

            if (gasStationFile.FileName != null)
            {
                var FileData = GasStationReport(gasStationFile.FileName);
                ViewBag.FileName = gasStationFile.FileName;
                ViewBag.Id = gasStationFile.Id;
                //if file is retrieved, its view is returned
                return View(FileData);
            }
            else
            {
                return NotFound();
            }
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
                GasStationFile fileObj = Methods.GlobalMethods.UploadFile(model, "reports", webroot);
                if (fileObj != null)
                {
                    _context.Add(fileObj);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("ShowReport", new { id = fileObj.Id });
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
        // GET: GasStation
        public async Task<IActionResult> Index()
        {
            return View(await _context.GasStationFile.ToListAsync());
        }


        // METHODS
        private List<ViewModels.GasStationFileViewModel> ReturnFileData( string FileName )
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
        private List<GasStation> GasStationReport(string FileName)
        {
            List<GasStation> FileData = new List<GasStation>();
            var gasStation = _context.GasStation.ToList();
            if (FileName != null)
            {
                #region fetch data
                string uploadsFolder = Path.Combine(webhostEnvironment.WebRootPath, "reports");
                string filePath = Path.Combine(uploadsFolder, FileName);
                string[] GsReportText = System.IO.File.ReadAllLines(filePath);
                #endregion
                
                foreach (string line in GsReportText)
                {
                    #region split data into columns
                    string[] cols = line.Split('\t');
                    for (int i = 0; i < cols.Length; i++)
                    {
                        cols[i] = cols[i].Trim(' ');
                    }
                    #endregion

                    var tempRow = CreateGasStationRow(cols);
                    int indexOfRow = gasStation.FindIndex(x => x.Date == tempRow.Date && x.Location == tempRow.Location
                    && x.CardNumber == tempRow.CardNumber);
                    if(indexOfRow == -1)
                        FileData.Add(tempRow);
                    
                }

            }
            //its view is returned  
            //if it is empty, empty list is returned
            return FileData;
        }
        private GasStation CreateGasStationRow(string[] cols)
        {
            string cardNo = 7 > cols[0].Length ? cols[0] : cols[0].Substring(cols[0].Length - 7);
            string cardNoD = cols[1];
            string date = cols[2];
            // cols[3] skip
            string location = cols[4];
            // cols[5] skip
            bool result = Int32.TryParse(cols[6], out int itemId);
            if (!result)
                itemId = -1;

            string itemName = cols[7];

            result = double.TryParse(cols[8], out double itemPrice);
            if (!result)
                itemPrice = -1;

            result = double.TryParse(cols[9], out double itemAmount);
            if (!result)
                itemAmount = -1;

            result = double.TryParse(cols[10], out double totalPrice);
            if (!result)
                totalPrice = -1;
            return new GasStation
            {
                CardNumber = cardNo,
                CardNumberD = cardNoD,

                Date = date,
                Location = location,

                ItemId = itemId,
                ItemName = itemName,
                ItemPrice = itemPrice,
                ItemAmount = itemAmount,                
                ItemTotalPrice = totalPrice
            };
        }

    }
}

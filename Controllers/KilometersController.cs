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
//Excel
using ClosedXML.Excel;


namespace kuras.Controllers
{
    
    public class KilometersController : Controller
    {
        private readonly kurasContext _context;
        private readonly IWebHostEnvironment webhostEnvironment;

        public KilometersController(kurasContext context, IWebHostEnvironment webhostEnvironment)
        {
            _context = context;
            this.webhostEnvironment = webhostEnvironment;
        }
        // GET: Kilometers
        public async Task<IActionResult> Index()
        {
            return View(await _context.KilometersFile.ToListAsync());
        }
        public async Task<IActionResult> Delete(int? id)
        {
            //if no such id
            if (id == null)
            {
                return NotFound();
            }

            var kilometersFile = await _context.KilometersFile
                .FirstOrDefaultAsync(m => m.Id == id);

            //if no data in that id 
            if (kilometersFile == null)
            {
                return NotFound();
            }                  
            
            return View(kilometersFile);                                   
        }
        // POST: Kilometers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            
            var kilometersFile = await _context.KilometersFile.FindAsync(id);
            string webroot = webhostEnvironment.WebRootPath;
            string folderName = "kilometers";
            string folderPath = Path.Combine(webroot, folderName);
            bool wasDeleted = kuras.Methods.GlobalMethods.DeleteFile(kilometersFile.FileName, folderPath);
            if(wasDeleted)
            {
                _context.KilometersFile.Remove(kilometersFile);
                await _context.SaveChangesAsync();
            }               
            return RedirectToAction(nameof(Index));
        }

        public IActionResult UploadOdo()
        {

            return View();
        }
        public IActionResult UploadGps()
        {

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadGps([Bind("Id, gsReport")] kuras.ViewModels.FileUploadViewModel model)
        {
            if (ModelState.IsValid)
            {
                string webroot = webhostEnvironment.WebRootPath;
                GasStationFile fileObj = Methods.GlobalMethods.UploadFile(model, "kilometers", webroot);
                KilometersFile kmFile = new KilometersFile
                {
                    FileName = fileObj.FileName,
                    Uploaded = false,
                    Gps = true
                };
                if (kmFile != null)
                {
                    _context.Add(kmFile);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("ShowGps", new { id = kmFile.Id });
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadOdo([Bind("Id, gsReport")] kuras.ViewModels.FileUploadViewModel model)
        {
            if (ModelState.IsValid)
            {
                string webroot = webhostEnvironment.WebRootPath;
                GasStationFile fileObj = Methods.GlobalMethods.UploadFile(model, "kilometers", webroot);
                KilometersFile kmFile = new KilometersFile
                {
                    FileName = fileObj.FileName,
                    Uploaded = false,
                    Gps = false
                };
                if (kmFile != null)
                {
                    _context.Add(kmFile);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("ShowOdo", new { id = kmFile.Id });
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

        public async Task<IActionResult> ShowGps(int? id)
        {
            //if no such id
            if (id == null)
            {
                return NotFound();
            }

            var kilometersFile = await _context.KilometersFile
                .FirstOrDefaultAsync(m => m.Id == id);

            //if no data in that id 
            if (kilometersFile == null)
            {
                return NotFound();
            }

            if (kilometersFile.FileName != null)
            {
                var FileData = ReturnFileDataGps(kilometersFile.FileName);
                ViewBag.FileName = kilometersFile.FileName;
                //if file is retrieved, its view is returned
                return View(FileData);
            }
            else
            {
                return NotFound();
            }
        }
        public async Task<IActionResult> ShowOdo(int? id)
        {
            //if no such id
            if (id == null)
            {
                return NotFound();
            }

            var kilometersFile = await _context.KilometersFile
                .FirstOrDefaultAsync(m => m.Id == id);

            //if no data in that id 
            if (kilometersFile == null)
            {
                return NotFound();
            }

            if (kilometersFile.FileName != null)
            {
                var FileData = ReturnFileDataOdo(kilometersFile.FileName);
                ViewBag.FileName = kilometersFile.FileName;
                //if file is retrieved, its view is returned
                return View(FileData);
            }
            else
            {
                return NotFound();
            }
        }

        public List<ViewModels.KilometersOdoFileViewModel> ReturnFileDataOdo(string FileName)
        {
            List<ViewModels.KilometersOdoFileViewModel> FileData = new List<ViewModels.KilometersOdoFileViewModel>();

            if (FileName != null)
            {
                string uploadsFolder = Path.Combine(webhostEnvironment.WebRootPath, "kilometers");
                string filePath = Path.Combine(uploadsFolder, FileName);
                var wb = new XLWorkbook(filePath);
                var ws = wb.Worksheet(1);
                var numberplate = 5;
                var period = 3;
                var start = 44;
                var end = 45;
                var datastart = 6;
                var dataRow = ws.Row(datastart).RowUsed();

                List<ViewModels.KilometersOdoFileViewModel> Devices = new List<ViewModels.KilometersOdoFileViewModel>();

                while (!dataRow.Cell(numberplate).IsEmpty())
                {
                    string tempp = dataRow.Cell(period).GetString();
                    string tempnb = dataRow.Cell(numberplate).GetString();
                    double temps = dataRow.Cell(start).GetDouble();
                    double tempe = dataRow.Cell(end).GetDouble();

                    Devices.Add(new ViewModels.KilometersOdoFileViewModel
                    {
                        Period = tempp,
                        NumberPlate = tempnb,
                        Start = temps,
                        End = tempe
                    }); 
                   

                    dataRow = dataRow.RowBelow();

                }
                FileData = Devices;
            }
            //its view is returned  
            //if it is empty, empty list is returned
            return FileData;
        }
        public List<ViewModels.KilometersGpsFileViewModel> ReturnFileDataGps(string FileName)
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

                while(!dataRow.Cell(device).IsEmpty())
                {
                    string dname = dataRow.Cell(device).GetString();
                    bool kmb = dataRow.Cell(kms).TryGetValue(out double tempkm);
                    dname = dname.Trim(' ');
                    var splitd = dname.Split(' ');
                    for(int i = 0; i < splitd.Length; i++)
                    {
                        splitd[i] = splitd[i].Trim(' ');
                    }
                    string np = "";
                    string name = "";
                    switch(splitd.Length)
                    {
                        case 1:
                            np = splitd[0];
                            break;
                        case 2:
                            if (splitd[0].Length == 3 && splitd[1].Length == 3)
                                np = splitd[0] + splitd[1];
                            else if(splitd[0].Length == 6)
                            {
                                np = splitd[0];
                                name = splitd[1];
                            }
                            break;
                        case 3:
                            if(splitd[0].Length == 3)
                            {
                                np = splitd[0] + splitd[1];
                                name = splitd[2];
                            }
                            else if(splitd[0].Length == 6)
                            {
                                np = splitd[0];
                                name = splitd[1] + "_" + splitd[2];
                            }
                            break;
                        case 4:
                            if(splitd[0].Length == 3 && splitd[1].Length == 3)
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

                    if(kmb)
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
    }
}

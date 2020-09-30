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
    public class CardController : Controller
    {
        private readonly kurasContext _context;
        private readonly IWebHostEnvironment webhostEnvironment;
        public CardController(kurasContext context, IWebHostEnvironment webhostEnvironment)
        {
            _context = context;
            this.webhostEnvironment = webhostEnvironment;
        }
        // GET: CardFullViewModel
        public async Task<IActionResult> DetailedList()
        {
            kuras.ViewModels.CardFullViewModel cardFullViewModel = new ViewModels.CardFullViewModel
            {
                AllCards = await _context.Card.ToListAsync(),
                AllCars = await _context.Car.ToListAsync(),
                AllEmployees = await _context.Employee.ToListAsync()
            };

            return View(cardFullViewModel);
        }
        // GET: Card
        public async Task<IActionResult> Index()
        {
            return View(await _context.Card.ToListAsync());
        }

        // GET: Card/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var card = await _context.Card
                .FirstOrDefaultAsync(m => m.Id == id);
            if (card == null)
            {
                return NotFound();
            }

            return View(card);
        }

        // GET: Card/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Card/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Car,Emp,Number,Limit")] Card card)
        {
            if (ModelState.IsValid)
            {
                _context.Add(card);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(card);
        }

        // GET: Card/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var card = await _context.Card.FindAsync(id);
            if (card == null)
            {
                return NotFound();
            }
            return View(card);
        }

        // POST: Card/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Car,Emp,Number,Limit")] Card card)
        {
            if (id != card.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(card);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CardExists(card.Id))
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
            return View(card);
        }

        // GET: Card/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var card = await _context.Card
                .FirstOrDefaultAsync(m => m.Id == id);
            if (card == null)
            {
                return NotFound();
            }

            return View(card);
        }

        // POST: Card/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var card = await _context.Card.FindAsync(id);
            _context.Card.Remove(card);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CardExists(int id)
        {
            return _context.Card.Any(e => e.Id == id);
        }
        public IActionResult Upload()
        {
            return View();
        }
        // GET DATA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload([Bind("Id, gsReport")] kuras.ViewModels.FileUploadViewModel model)
        {
            if (ModelState.IsValid)
            {
                string webroot = webhostEnvironment.WebRootPath;
                GasStationFile fileObj = Methods.GlobalMethods.UploadFile( model, "cards", webroot);
                if(fileObj != null)
                {
                    _context.Add(fileObj);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("ShowFile", new { id = fileObj.Id });
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

        // INSERT INTO DATA BASE

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

            if (gasStationFile.FileName != null)
            {
                var FileData = ReturnFileData(gasStationFile.FileName);
                //InsertData(FileData);

                Card card;
                Employee emp;
                Car car;

                foreach (var el in FileData)
                {
                    string[] makeModel = el.MakeModel.Split(' ');
                    int m_id = 0;
                    string make = "";
                    string model = "";
                    //if(makeModel[0] == "MERCEDES" && )
                    foreach (string m in makeModel)
                    {
                        if (m_id == 0)
                        {
                            make = m;
                        }
                        else
                        {
                            model += m;
                        }
                        m_id++;
                    }
                    car = new Car
                    {
                        Make = make,
                        Model = model,
                        NumberPlate = el.NumberPlate
                    };

                    _context.Add(car);
                    await _context.SaveChangesAsync();

                    var carId = await _context.Car
                    .FirstOrDefaultAsync(m => m.NumberPlate == el.NumberPlate);

                    //carId.Id;
                    Employee empId;
                    string[] driver = el.Driver.Split(' ');
                    if (driver.Length == 2)
                    {
                        emp = new Employee
                        {
                            Fname = driver[0],
                            Lname = driver[1]
                        };
                        _context.Add(emp);
                        await _context.SaveChangesAsync();

                        empId = await _context.Employee
                        .FirstOrDefaultAsync(m => m.Fname == driver[0] && m.Lname == driver[1]);
                    }
                    else
                    {
                        emp = new Employee
                        {
                            Fname = driver[0],
                            Lname = ""
                        };
                        _context.Add(emp);
                        await _context.SaveChangesAsync();

                        empId = await _context.Employee
                        .FirstOrDefaultAsync(m => m.Fname == driver[0] && m.Lname == driver[0]);
                    }
                    if (empId != null && carId != null)
                    {
                        card = new Card
                        {
                            Emp = empId.Id,
                            Car = carId.Id,
                            Number = el.CardNumber,
                            NumberD = el.CardNumberD
                        };
                        _context.Add(card);
                        await _context.SaveChangesAsync();
                    }


                }

                ViewBag.FileName = gasStationFile.FileName;
                ViewBag.Id = id;
                //if file is retrieved, its view is returned
                return RedirectToAction("Index");
            }
            else
            {
                return NotFound();
            }
        }





        // SHOW DATA
        // GET: GasStation/ShowCard/5
        public async Task<IActionResult> ShowFile(int? id)
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
                ViewBag.Id = id;
                //if file is retrieved, its view is returned
                return View(FileData);
            }
            else
            {
                return NotFound();
            }
        }
        private List<CardData> ReturnFileData(string FileName)
        {
            List<CardData> FileData = new List<CardData>();

            if (FileName != null)
            {
                string uploadsFolder = Path.Combine(webhostEnvironment.WebRootPath, "cards");
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
                    FileData.Add(new CardData
                    {

                        //CardNumber = cols[0].Substring(Math.Max(0, cols[0].Length - 4)),
                        //7 > cols[0].length ? cols[0] : cols[0].Substring(cols[0].length - 7)
                        //4 > mystring.length ? mystring : mystring.Substring(mystring.length -4);
                        //mystring.Substring(Math.Max(0, mystring.Length - 4))

                        CardNumber = 7 > cardNo.Length ? cardNo : cardNo.Substring(cardNo.Length - 7),
                        CardNumberD = cols[1],
                        NumberPlate = cols[2],
                        MakeModel = cols[3],
                        Driver = cols[4]                        
                    });
                    
                }

            }
            //its view is returned  
            //if it is empty, empty list is returned
            return FileData;
        }

    }
}

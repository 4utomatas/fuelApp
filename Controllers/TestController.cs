using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using kuras.Data;
using kuras.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace kuras.Controllers
{
    public class TestController : Controller
    {
        private readonly kurasContext _context;
        private readonly IWebHostEnvironment webhostEnvironment;
        public async Task<IActionResult> ShowFile(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Test
                .FirstOrDefaultAsync(m => m.Id == id);
            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }
        public TestController(kurasContext context,
                              IWebHostEnvironment webhostEnvironment)
        {
            _context = context;
            this.webhostEnvironment = webhostEnvironment;
        }

        // GET: Test
        public async Task<IActionResult> Index()
        {
            return View(await _context.Test.ToListAsync());
        }

        // GET: Test/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Test
                .FirstOrDefaultAsync(m => m.Id == id);
            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

        // GET: Test/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Test/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id, gsReport")] kuras.ViewModels.FileUploadViewModel upload)
        {
            if (ModelState.IsValid)
            {

                string uniqueFileName = null;
                if (upload.gsReport != null)
                {
                    string uploadsFolder = Path.Combine(webhostEnvironment.WebRootPath, "reports");
                    DateTime a = DateTime.Now;

                    char[] date_now = new char[a.ToString().Length];
                    int i = 0;
                    foreach (char b in a.ToString())
                    {
                        if (b == ' ' || b == '/')
                        {
                            date_now[i] = '_';
                        }
                        else if(b == ':')
                        {
                            date_now[i] = ';';
                        } 
                        else
                        {
                            date_now[i] = b;
                        }
                        i++;
                    }
                    string temp_string_date = new string(date_now);

                    uniqueFileName = "date-" + temp_string_date + "__" + upload.gsReport.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    upload.gsReport.CopyTo(new FileStream(filePath, FileMode.Create));
                }

                Test test = new Test
                {
                    FileName = uniqueFileName
                };

                _context.Add(test);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            
            return View(upload);
        }

        // GET: Test/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Test.FindAsync(id);
            if (test == null)
            {
                return NotFound();
            }
            return View(test);
        }

        // POST: Test/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FileName")] Test test)
        {
            if (id != test.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(test);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TestExists(test.Id))
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
            return View(test);
        }

        // GET: Test/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Test
                .FirstOrDefaultAsync(m => m.Id == id);
            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

        // POST: Test/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var test = await _context.Test.FindAsync(id);
            _context.Test.Remove(test);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TestExists(int id)
        {
            return _context.Test.Any(e => e.Id == id);
        }
    }
}

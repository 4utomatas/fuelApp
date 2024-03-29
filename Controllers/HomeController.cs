﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
//data selection
using kuras.Data;
using kuras.Models;

namespace kuras.Controllers
{
    public class HomeController : Controller
    {
        private readonly kurasContext _context;
        private readonly ILogger<HomeController> _logger;
        

        public HomeController(kurasContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var gasStationfiles = _context.GasStationFile.ToList();
            var kilometerfiles = _context.KilometersFile.ToList();
            kuras.ViewModels.AllFileViewModel allfiles = new ViewModels.AllFileViewModel{ 
                GasStationFileAll = gasStationfiles,
                KilometersFileAll = kilometerfiles
            };
            return View(allfiles);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

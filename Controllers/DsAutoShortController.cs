using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using kuras.AxaptaModels;
using kuras.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace kuras.Controllers
{
    public class DsAutoShortController : Controller
    {
        private readonly TestContext _context;
        private readonly kurasContext _kuras_context;

        public DsAutoShortController(TestContext context, kurasContext kuras_context)
        {
            _context = context;
            _kuras_context = kuras_context;
        }
        // GET: DsAutoShort
        public IActionResult Index()
        {
            //var dsAutoShort =  await _context.DsAutoShort.ToListAsync();
            var found = FindNewCars();
            return View(found);
        }
        public IActionResult Found()
        {
            var found = FindNewCarsWithEmps();
            return View(found);
        }
        private ICollection<DsAutoShort> FindNewCars()
        {
            var fetched = new List<AxaptaViewModels.FetchedCarEmp>();
            var found = new List<DsAutoShort>();
            var dsAuto =  _context.DsAutoShort.ToList();
            var cars = _kuras_context.Car.ToList();
            //Employees
            var empAx = _context.Empltable.ToList();
            Models.Employee employee = new Models.Employee();
            foreach(var auto in dsAuto)
            {
                if( auto.Blocked == 0
                    && !String.IsNullOrWhiteSpace(auto.EmployeeId)
                    && auto.Type != 2
                    && !auto.EmployeeId.Contains("ZZZZZZZZZ") 
                    && FindCar(auto, cars))
                {
                    found.Add(auto);
                    var found_emp = empAx.FirstOrDefault(x => x.Emplid == auto.EmployeeId);
                    fetched.Add(new AxaptaViewModels.FetchedCarEmp
                    {
                        AxEmployee = found_emp,
                        DsAuto = auto
                    });
                }
            }
            return found;
        }
        private ICollection<AxaptaViewModels.FetchedCarEmp> FindNewCarsWithEmps()
        {
            var fetched = new List<AxaptaViewModels.FetchedCarEmp>();
            var dsAuto = _context.DsAutoShort.ToList();
            var cars = _kuras_context.Car.ToList();
            // Employees
            var empAx = _context.Empltable.ToList();
            foreach (var auto in dsAuto)
            {
                if (auto.Blocked == 0
                    && !String.IsNullOrWhiteSpace(auto.EmployeeId)
                    && auto.Type != 2
                    && !auto.EmployeeId.Contains("ZZZZZZZZZ")
                    && FindCar(auto, cars))
                {
                    var found_emp = empAx.FirstOrDefault(x => x.Emplid == auto.EmployeeId);
                    fetched.Add(new AxaptaViewModels.FetchedCarEmp
                    {
                        AxEmployee = found_emp,
                        DsAuto = auto
                    });
                }
            }
            return fetched;
        }
        private bool FindCar(DsAutoShort auto, ICollection<Models.Car> cars)
        {
            foreach (var car in cars)
            {
                if (auto.NumberPlate == car.NumberPlate)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
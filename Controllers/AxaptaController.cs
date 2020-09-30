using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
//Added
using Microsoft.AspNetCore.Http;
using kuras.AxaptaModels;
using kuras.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
// diagnostics
using System.Diagnostics;

namespace kuras.Controllers
{
	public class AxaptaController : Controller
	{
		private readonly TestContext _context;
		private readonly kurasContext _kuras_context;
		public AxaptaController(TestContext context, kurasContext kuras_context)
		{
			_context = context;
			_kuras_context = kuras_context;
		}
		// Returns a list of new entries in Axapta.
		public async Task<IActionResult> Index()
		{
			// ViewModel contains from the 3 table linked data ( by employee ID and Number Plate)
			var found = await FindNewAxaptaData();
			return View(found);
		}
		// Returns the list of Axapta table data
		public async Task<IActionResult> Refine()
		{
			await CommitChanges();
			return View(await _kuras_context.Axapta.ToListAsync());
		}
		// POST: Card/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete([Bind("Id")] int id)
		{
			var card = await _kuras_context.Axapta.FindAsync(id);
			if(card != null)
			{
				_kuras_context.Axapta.Remove(card);
				await _kuras_context.SaveChangesAsync();
				return RedirectToAction(nameof(Refine));
			}
			else
			{
				return NotFound();
			}
			
		}
		// Hidden function, not available via button
		// Adds all Axapta table records to Car, Card and Employee tables
		public async Task<IActionResult> AddAll()
		{
			var axaptaData = await _kuras_context.Axapta.ToListAsync();
			var addViewModel = new AxaptaViewModels.AddViewModel();
			foreach(var el in axaptaData)
			{
				addViewModel = await GenerateAddViewModel(el);
				if(addViewModel != null)
				{
					// !!!Check if the car exists
					_kuras_context.Add(addViewModel.Car);
					// There can be an employee that drives more than one car
					if(await _kuras_context.Employee.FirstOrDefaultAsync(x => x.Id == addViewModel.Employee.Id) == null)
					{
						_kuras_context.Add(addViewModel.Employee);
					}
					// Saving changes to retrieve ids from tables
					await _kuras_context.SaveChangesAsync();

					var newcarId = await _kuras_context.Car.FirstOrDefaultAsync(x => x.NumberPlate == addViewModel.Car.NumberPlate);
					var newempId = await _kuras_context.Employee.FirstOrDefaultAsync(x => x.EmplId == addViewModel.Employee.EmplId);

					addViewModel.Card.Car = newcarId.Id;
					addViewModel.Card.Emp = newempId.Id;

					_kuras_context.Add(addViewModel.Card);
					await _kuras_context.SaveChangesAsync();
				}   
			}
			// removes all records from axapta table
			DeleteTable();
			return RedirectToAction(nameof(Index));
		}
		public IActionResult Success(string np)
		{
			ViewBag.NP = np;
			return View();
		}
		// POST: Card/Add/5
		[HttpPost, ActionName("Add")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Add(IFormCollection collection)
		{
			// Getting data from the form
			string numberplate = collection["Car.Numberplate"];
			string make = collection["Car.Make"];
			string model = collection["Car.Model"];
			string limit = collection["Card.Limit"];

			string cardnumber = collection["Card.Number"];
			string cardnumberD = collection["Card.NumberD"];
			
			string fname = collection["Employee.Fname"];
			string lname = collection["Employee.Lname"];
			string city = collection["Employee.City"];
			string group = collection["Employee.Group"];
			string emplid = collection["Employee.EmplId"];

			Models.Car newcar = new Models.Car
			{
				NumberPlate = numberplate,
				Make = make,
				Model = model
			};
			// If the car already exists, update it
			var fCar = await _kuras_context.Car.FirstOrDefaultAsync(x => x.NumberPlate == newcar.NumberPlate);
			if (fCar != null)
			{
				fCar.UpdateValues(newcar);
				_kuras_context.Update(fCar);
			}
			else _kuras_context.Add(newcar);


			Models.Employee newemp = new Models.Employee
			{
				Fname = fname,
				Lname = lname,
				City = city,
				Group = group,
				EmplId = emplid
			};

			var fEmp = await _kuras_context.Employee.FirstOrDefaultAsync(x => x.EmplId == newemp.EmplId);
			if(fEmp != null)
			{
				fEmp.UpdateValues(newemp);
				_kuras_context.Update(fEmp);
			}
			else _kuras_context.Add(newemp);

			await _kuras_context.SaveChangesAsync();
			// Find the newly added or updated employess' and car's IDs
			var newcarId = await _kuras_context.Car.FirstOrDefaultAsync(x => x.NumberPlate == newcar.NumberPlate);
			var newempId = await _kuras_context.Employee.FirstOrDefaultAsync(x => x.EmplId == newemp.EmplId);
			
			Models.Card newcard = new Models.Card
			{
				Car = newcarId.Id,
				Emp = newempId.Id,
				Number = cardnumber,
				NumberD = cardnumberD,
				Limit = ((Int32.TryParse(limit, out int limitInt)) ? limitInt : 99)
			};

			_kuras_context.Add(newcard);
			await _kuras_context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
		// Adding one line at a time
		public async Task<IActionResult> Add(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}
			var model = await _kuras_context.Axapta.FindAsync(id);
			if (model != null)
			{
				var addViewModel = GenerateAddViewModel(model);
				//_kuras_context.Axapta.Remove(model);
				//await _kuras_context.SaveChangesAsync();
				//return RedirectToAction(nameof(Refine));
				return View(addViewModel);
			}
			else
			{
				return NotFound();
			}
		}
		
		// Generates AddViewModel => Card, Car, Employee
		private async Task<AxaptaViewModels.AddViewModel> GenerateAddViewModel( Models.Axapta model )
		{
			if (model != null)
			{



				string _make = "";
				string _model = "";
				SeparateMakeModel(model.MakeModel, ref _make, ref _model);

				Models.Car car = new Models.Car
				{
					Model = _model,
					Make = _make,
					NumberPlate = model.NumberPlate
				};
				string[] fullname = model.EmpName.Trim(' ').Split(' ');
				string fname = "", sname = "";
				for (int i = fullname.Length - 1; i >= 0; i--)
				{
					if (i == fullname.Length - 1)
						sname = fullname[i];
					else fname += fullname[i];
				}
				//No duplicates
				var emp = await _kuras_context.Employee.FirstOrDefaultAsync(x => x.EmplId == model.EmpIdEmplTable);
				if (emp == null)
				{
					emp = new Models.Employee
					{
						Fname = fname,
						Lname = sname,
						City = model.CityCode,
						Group = model.EmpGroup,
						EmplId = model.EmpIdEmplTable
					};
				}

				Models.Card card = new Models.Card
				{
					Number = model.CardNumber,
					NumberD = model.CardNumberD,
					Limit = 999
				};

				AxaptaViewModels.AddViewModel addViewModel = new AxaptaViewModels.AddViewModel
				{
					Card = card,
					Employee = emp,
					Car = car
				};
				return addViewModel;
			}
			return null;
		}
		// Searches for new data in axapta tables
		private async Task<ICollection<AxaptaViewModels.AxaptaViewModel>> FindNewAxaptaData()
		{
			var fetched = new List<AxaptaViewModels.AxaptaViewModel>();

			var upload = new List<Models.Axapta>();
			// Cars from kuras database
			var cars = await _kuras_context.Car.ToListAsync();
			// Car details from axapta
			var dsAuto = await _context.DsAutoShort.ToListAsync();
			// Card details from axapta
			var autoKort = await _context.DsAutoKort.ToListAsync();
			//Employees in Axapta
			var empAx = await _context.Empltable.ToListAsync();
			foreach (var auto in dsAuto)
			{            
				// if car is not blocked
				// if employee id is not blank
				// if car is not found in the kuras.Car table 
				if ( auto.Blocked == 0 &&
					 !String.IsNullOrWhiteSpace(auto.EmployeeId) &&
					 auto.Type != 2 &&
					 !auto.EmployeeId.Contains("ZZZZZZZZZ")
					)
				{
					if(!FindCar(auto, cars))
					{
						// Searching for the same employee in empltable
						var found_emp = empAx.FirstOrDefault(x => x.Emplid == auto.EmployeeId);
						// Searching for the same numberPlate in autokort
						var found_kort = autoKort.FirstOrDefault(x => x.NumberPlate == auto.NumberPlate);
						
						// If none were found, create empty objects with no values
						if (found_emp == null)
							found_emp = new Empltable();

						if (found_kort == null)
							found_kort = new DsAutoKort();

						// For the index page (of Axapta) View
						fetched.Add(new AxaptaViewModels.AxaptaViewModel
						{
							AxEmployee = found_emp,
							DsAuto = auto,
							DsAutoKort = found_kort
						});
						// Create an object which will be uploaded to Axapta table
						upload.Add(new Models.Axapta
						{
							EmpIdAutoTable = auto.EmployeeId,
							MakeModel = auto.MakeModel,
							Type = auto.Type,
							Blocked = auto.Blocked,
							NumberPlate = auto.NumberPlate,

							CardNumber = found_kort.CardNumberShort,
							CardNumberD = found_kort.CardNumberD,

							EmpIdEmplTable = found_emp.Emplid,
							EmpName = found_emp.Name,
							EmpGroup = found_emp.Dimension, //group                    
							CityCode = found_emp.StateId,
							Occupation = found_emp.Title

						});
					}
					else
					{
						// if the car was found, we need to check if anything changed for it
						Models.Car fCar = null;
						FindCar(auto, cars, ref fCar);
						if(fCar != null)
						{
							string _model = null, _make = null;
							SeparateMakeModel(auto.MakeModel, ref _model, ref _make);
							// Change the existing Car
							if(_model.Trim(' ') != fCar.Model.Trim(' '))
							{
								fCar.Model = _model.Trim(' ');
							}
							if(_make.Trim(' ') != fCar.Make.Trim(' '))
							{
								fCar.Make = _make.Trim(' ');
							}
							// Save changes to the existing car
							_kuras_context.Update(fCar);
							await _kuras_context.SaveChangesAsync();
						}
					}
					
				}
			}
			if(upload.Any())
			{
				await UpdateTable(upload);
			}
			return fetched;
		}
		// Copy
		private async Task<ICollection<Models.Axapta>> FindAxaptaData()
		{
			var fetched = new List<AxaptaViewModels.AxaptaViewModel>();

			var upload = new List<Models.Axapta>();
			// Cars from kuras database
			var cars = await _kuras_context.Car.ToListAsync();
			// Car details from axapta
			var dsAuto = await _context.DsAutoShort.ToListAsync();
			// Card details from axapta
			var autoKort = await _context.DsAutoKort.ToListAsync();
			//Employees in Axapta
			var empAx = await _context.Empltable.ToListAsync();
			foreach (var auto in dsAuto)
			{
				// if car is not blocked
				// if employee id is not blank
				// if car is found in the kuras.Car table 
				if (auto.Blocked == 0
					&& !String.IsNullOrWhiteSpace(auto.EmployeeId)
					&& auto.Type != 2
					&& !auto.EmployeeId.Contains("ZZZZZZZZZ"))
					//&& FindCar(auto, cars))
				{
					var found_emp = empAx.FirstOrDefault(x => x.Emplid == auto.EmployeeId);
					var found_kort = autoKort.FirstOrDefault(x => x.NumberPlate == auto.NumberPlate);

					fetched.Add(new AxaptaViewModels.AxaptaViewModel
					{
						AxEmployee = found_emp,
						DsAuto = auto,
						DsAutoKort = found_kort
					});

					if (found_emp == null)
						found_emp = new Empltable();

					if (found_kort == null)
						found_kort = new DsAutoKort();

					upload.Add(new Models.Axapta
					{
						EmpIdAutoTable = auto.EmployeeId,
						MakeModel = auto.MakeModel,
						Type = auto.Type,
						Blocked = auto.Blocked,
						NumberPlate = auto.NumberPlate,

						CardNumber = found_kort.CardNumberShort,
						CardNumberD = found_kort.CardNumberD,

						EmpIdEmplTable = found_emp.Emplid,
						EmpName = found_emp.Name,
						EmpGroup = found_emp.Dimension, //group                    
						CityCode = found_emp.StateId,
						Occupation = found_emp.Title

					});
				}
			}
			return upload;
		}
		// Checks if there have been made any changes to cards, car, employee 
		// If changes are made it updates the table row data
		private async Task FindChanges1(AxaptaViewModels.AddViewModel model)
		{
			// f stands for fetched
			var fCard = await _kuras_context.Card.
				FirstOrDefaultAsync(x => x.Number == model.Card.Number && x.NumberD == model.Card.NumberD);
			if(fCard != null)
			{
				var fCar = await _kuras_context.Car.FirstOrDefaultAsync(x => x.Id == fCard.Car);
				var fEmp = await _kuras_context.Employee.FirstOrDefaultAsync(x => x.Id == fCard.Emp);
				if(fCar != null && fEmp != null)
				{
					// if cars are different
					if(fCar.NumberPlate != model.Car.NumberPlate)
					{
						// updated car
						var uCar = await _kuras_context.Car.
							FirstOrDefaultAsync(x => x.NumberPlate == model.Car.NumberPlate);
						if (uCar != null)
							fCard.Car = uCar.Id;
						else Debug.WriteLine("Car with number plate: {0} does not exist", model.Car.NumberPlate);
					}
					if(fEmp.EmplId != model.Employee.EmplId)
					{
						var uEmp = await _kuras_context.Employee.
							FirstOrDefaultAsync(x => x.EmplId == model.Employee.EmplId);
						if (uEmp != null)
							fCard.Emp = uEmp.Id;
						else Debug.WriteLine("Employee with {0} ID does not exist", model.Employee.EmplId);
					}
					//fCar.UpdateValues(model.Car);
					//fEmp.UpdateValues(model.Employee);
				}
			}
			


			//var fetchedCar = await _kuras_context.Car.FirstOrDefaultAsync(x => x.NumberPlate == model.Car.NumberPlate);
			//if(fetchedCar.Model != model.Car.Model || fetchedCar.Make != model.Car.Make)
			//{
			//    fetchedCar.Model = model.Car.Model;
			//    fetchedCar.Make = model.Car.Make;
			//    _kuras_context.Update(fetchedCar);
			//}
			
			//var containsCar = await _kuras_context.Car.ContainsAsync(model.Car);
			//var containsCard = await _kuras_context.Card.ContainsAsync(model.Card);
			//var containsEmployee = await _kuras_context.Employee.ContainsAsync(model.Employee);
			//if (!containsCar)
			//    _kuras_context.Update(model.Car);
			//if (!containsCard)
			//    _kuras_context.Update(model.Card);
			//if (!containsEmployee)
			//    _kuras_context.Update(model.Employee);
			//await _kuras_context.SaveChangesAsync();

		}
		// Go through the whole list of axapta data and find changes
		public async Task<ActionResult> Changes()
		{
			#region
			/*
			// Go through the list
			// Gets the data from the axapta database
			var axaptaList = await FindAxaptaData();
			// gets car and emp lists for further use
			var cars = await _kuras_context.Car.ToListAsync();
			var employess = await _kuras_context.Employee.ToListAsync();
			var cards = await _kuras_context.Card.ToListAsync();
			// a list for all the changes
			var changedList = new List<Models.Axapta>();


			foreach (var axaptaItem in axaptaList)
			{
				// if the card's number or numberd are empty it wont check that card
				Models.Card fCard = null;
				if (!String.IsNullOrWhiteSpace(axaptaItem.CardNumber) ||
					!String.IsNullOrWhiteSpace(axaptaItem.CardNumberD))
				{
					fCard = cards.FirstOrDefault
						(x => x.Number == axaptaItem.CardNumber && x.NumberD == axaptaItem.CardNumberD);
				}
				
				if(fCard != null)
				{
					// fetch the car and employee that are associated with the card number
					var fCar = cars.FirstOrDefault( x => x.Id == fCard.Car);
					var fEmp = employess.FirstOrDefault( x => x.Id == fCard.Emp);
					// if fCar and fEmp exist, if they did not, that would be a bad error!
					if(fCar != null && fEmp != null)
					{
						// checking if the cars and emps the same as in axapta
						// if cars are different
						if(fCar.NumberPlate != axaptaItem.NumberPlate)
						{
							changedList.Add(axaptaItem);
							// updated car
							var uCar = cars.FirstOrDefault(x => x.NumberPlate == axaptaItem.NumberPlate);
							if (uCar != null){
								Debug.WriteLine($"NumberPlates {0} and {1} do not match", axaptaItem.NumberPlate, uCar.NumberPlate);
								//fCard.Car = uCar.Id;
							}
							else Debug.WriteLine($"Car with number plate: {0} does not exist", axaptaItem.NumberPlate);
						} else Debug.WriteLine($"NumberPlates {0} match", axaptaItem.NumberPlate);

						if(fEmp.EmplId != axaptaItem.EmpIdEmplTable)
						{
							if(!changedList.Contains(axaptaItem))
							{
								changedList.Add(axaptaItem);
							}
							
							var uEmp = employess.FirstOrDefault(x => x.EmplId == axaptaItem.EmpIdEmplTable);
							if (uEmp != null) {
								
								Debug.WriteLine($"Employee with {0} and {1} ID dont match", axaptaItem.EmpIdEmplTable, uEmp.EmplId);
								fCard.Emp = uEmp.Id;
							}    
							else Debug.WriteLine($"Employee with {0} ID does not exist", axaptaItem.EmpIdEmplTable);
						} 
						else Debug.WriteLine($"Employee with {0} ID match", axaptaItem.EmpIdEmplTable);
						//fCar.UpdateValues(model.Car);
						//fEmp.UpdateValues(model.Employee);
					}
					else Debug.WriteLine($"Car and emp not fetched card: {0} {1}",
						axaptaItem.CardNumber, axaptaItem.CardNumberD);
				}
			}
			*/
			#endregion

			return View(await GenerateChanges());
		}

		// Find all changes between Axapta and Kuras Databases
		private async Task<List<ViewModels.AxaptaChangesViewModel>> GenerateChanges()
		{

			// Cars from kuras database
			var cars = await _kuras_context.Car.ToListAsync();
			// Cards from kuras database
			var cards = await _kuras_context.Card.ToListAsync();
			// Employees from kuras database
			var employee = await _kuras_context.Employee.ToListAsync();
			
			// Car details from axapta
			var dsAuto = await _context.DsAutoShort.ToListAsync();
			// Card details from axapta
			var autoKort = await _context.DsAutoKort.ToListAsync();
			//Employees in Axapta
			var empl = await _context.Empltable.ToListAsync();

			var list = new List<ViewModels.AxaptaChangesViewModel>();
			// They are null to check later
			// a - axapta data
			Models.Card aCard = null;
			Models.Car aCar = null;
			Models.Employee aEmp = null;
			// fetched (from database)
			Models.Car fCar = null;
			Models.Employee fEmp = null;


			foreach (var auto in dsAuto)
			{
				// Create car model from dsauto
				aCar = Methods.GlobalMethods.ConvertToCar(auto);
				// Find if the car has a card in autokort table
				var fAutoKort = autoKort.FirstOrDefault(x => x.NumberPlate == auto.NumberPlate);
				if (fAutoKort != null)
				{
					var convertedNumber = Methods.GlobalMethods.ConvertCardNumber(fAutoKort.CardNumber);
					// Car check
					if(auto.Blocked == 0 && auto.Type != 2 && !auto.EmployeeId.Contains("ZZZZZZZZZ") && !string.IsNullOrWhiteSpace(convertedNumber) &&
						!string.IsNullOrWhiteSpace(fAutoKort.CardNumberD) && !String.IsNullOrWhiteSpace(auto.EmployeeId) )
					{
						// Check for employee
						// axapta empl table obj
						var aempl = empl.FirstOrDefault(x => x.Emplid == auto.EmployeeId);
						aEmp = null;
						if (aempl != null)
						{
							// Convert to Models.Employee
							aEmp = aempl.ReturnEmployee();
							// Check if the employee already exists
							fEmp = employee.FirstOrDefault(x => x.EmplId == aEmp.EmplId);
						}

						// If the car is found in the database
						if (FindCar(auto, cars, ref fCar))
						{
							var fCard = cards.FirstOrDefault(x => x.Car == fCar.Id);
							var fCardEmp = employee.FirstOrDefault(x => x.Id == fCard.Emp);
							if (fCard != null)
							{
								// autokort ? card
								if (convertedNumber != fCard.Number || fAutoKort.CardNumberD != fCard.NumberD)
								{
									// Seach for a card with the same number as in autokort
									var changedCard = cards.FirstOrDefault(x => x.Number == convertedNumber ||
										x.NumberD == fAutoKort.CardNumberD);
									if (changedCard != null && changedCard.Car != fCar.Id)
									{
										
										aCard = Methods.GlobalMethods.ConvertToCard(fAutoKort, fCar.Id);
										if (fEmp != null)
											aCard.Emp = fEmp.Id;
										list.Add(new ViewModels.AxaptaChangesViewModel(fCar, aCar, fCard, aCard, fEmp, aEmp));
									}
									// No card was found with such a cardNumber
									// Create a new card for the car
									else if (changedCard == null)
									{
										aCard = Methods.GlobalMethods.ConvertToCard(fAutoKort, fCar.Id);
										if (fEmp != null)
											aCard.Emp = fEmp.Id;
										list.Add(new ViewModels.AxaptaChangesViewModel(fCar, aCar, fCard, aCard, fEmp, aEmp));
									}
									
								}
								// if Employee is different OR Employee is new 
								if (fCardEmp != null && !aEmp.CompareEmplId(fCardEmp))
								{
									// if the employee is created but has a different Id, we need to check him
									fEmp = fCardEmp;
									aCard = Methods.GlobalMethods.ConvertToCard(fAutoKort, fCar.Id);
									if (fEmp != null)
										aCard.Emp = fEmp.Id;
									list.Add(new ViewModels.AxaptaChangesViewModel(fCar, aCar, fCard, aCard, fEmp, aEmp));
								}
								// no changes, everything is up-to-date (all data is identical)
							}
							// no card, new card
							else
							{
								aCard = Methods.GlobalMethods.ConvertToCard(fAutoKort, fCar.Id);
								if (fEmp != null)
									aCard.Emp = fEmp.Id;
								list.Add(new ViewModels.AxaptaChangesViewModel(fCar, aCar, null, aCard, fEmp, aEmp));
							}
						}
						// If there is no such car
						else
						{
							// Seach for a card with the same number as in autokort
							var changedCard = cards.FirstOrDefault(x => x.Number == convertedNumber ||
								x.NumberD == fAutoKort.CardNumberD);
							aCard = Methods.GlobalMethods.ConvertToCard(fAutoKort);
							if (fEmp != null)
								aCard.Emp = fEmp.Id;
							if (changedCard != null)
								list.Add(new ViewModels.AxaptaChangesViewModel(null, aCar, changedCard, aCard, fEmp, aEmp));
							// No card was found with such a cardNumber
							// Create a new card for the car
							else list.Add(new ViewModels.AxaptaChangesViewModel(null, aCar, null, aCard, fEmp, aEmp));

						}	
					}
				}
				// car is not used          
			}
			return list;
		}       

		private async Task CommitChanges()
		{
			var list = await GenerateChanges();
			var cars = await _kuras_context.Car.ToListAsync();
			var cards = await _kuras_context.Card.ToListAsync();
			var employees = await _kuras_context.Employee.ToListAsync();
			foreach (var el in list)
			{
				// used to insert/update into database
				Models.Car uCar = null;
				Models.Card uCard = null;
				Models.Employee uEmp = null;
				bool CarChanged = false;
				bool FEmpChanged = false;
				bool FCardChanged = false;
				if (el.FCar != null)
				{
					// if not the same
					if (el.ACar != null && el.FCar.CompareNP(el.ACar) && !el.FCar.Compare(el.ACar))
					{
						el.FCar.UpdateValues(el.ACar);
						// Update the car in the database
						_kuras_context.Update(el.FCar);
						CarChanged = true;
					}
					// Do not know if this works
					else if(el.ACar != null && !el.FCar.CompareNP(el.ACar))
					{
						// Not a new car but a different one
						uCar = cars.FirstOrDefault(x => x.NumberPlate == el.ACar.NumberPlate);
					}
				}
				else
				{
					uCar = el.ACar;
					_kuras_context.Add(uCar);
					CarChanged = true;
				}

				if ( el.FEmp != null )
				{
					if (el.AEmp != null && el.FEmp.CompareEmplId(el.AEmp) && !el.FEmp.Compare(el.AEmp))
					{
						el.FEmp.UpdateValues(el.AEmp);
						_kuras_context.Update(el.FEmp);
						FEmpChanged = true;
					}
					else if (el.AEmp != null && !el.FEmp.CompareEmplId(el.AEmp))
						uEmp = employees.FirstOrDefault(x => x.EmplId == el.AEmp.EmplId);
				}
				else
				{
					uEmp = el.AEmp;
					_kuras_context.Add(uEmp);
				}
					
				// Save changes if anything was changed
				if (uEmp != null || FEmpChanged || CarChanged)
					await _kuras_context.SaveChangesAsync();

				int uCarId = await GetCarId(uCar, el.FCar);
				int uEmpId = await GetEmployeeId(uEmp, el.FEmp);

				if (el.FCard != null)
				{
					if(el.ACard != null && !el.FCard.Compare(el.ACard) && el.FCard.CompareNumber(el.ACard))
					{
						el.FCard.UpdateValues(el.ACard);
						_kuras_context.Update(el.FCard);
						FCardChanged = true;
					}
					else if (el.FCard != null && !el.FCard.CompareNumber(el.ACard) && uCarId != 0 && uEmpId != 0)
					{
						// if it is a different card from Axapta(ACard) you need to find it and update it
						uCard = cards.FirstOrDefault(x => x.Number == el.ACard.Number && x.NumberD == el.ACard.NumberD);
						uCard.Car = uCarId;
						uCard.Emp = uEmpId;
						_kuras_context.Update(uCard);
						FCardChanged = true;
					}
				}
				else
				{
					uCard = el.ACard;
					if (uCarId != 0)
						uCard.Car = uCarId;
					if (uEmpId != 0)
						uCard.Emp = uEmpId;
					_kuras_context.Add(uCard);
					FCardChanged = true;
				}
				if (FCardChanged)
					await _kuras_context.SaveChangesAsync();

			}
		}
		async Task<int> GetCarId(Models.Car uCar, Models.Car fCar)
		{
			Models.Car newCarId = null;
			if (uCar != null)
				newCarId = await _kuras_context.Car.FirstOrDefaultAsync(x => x.NumberPlate == uCar.NumberPlate);
			else if (fCar != null)
				newCarId = await _kuras_context.Car.FirstOrDefaultAsync(x => x.NumberPlate == fCar.NumberPlate);
			return newCarId != null ? newCarId.Id : 0;
			

		}
		async Task<int> GetEmployeeId(Models.Employee uEmp, Models.Employee fEmp)
		{
			Models.Employee newEmpId = null;
			if (uEmp != null)
				newEmpId = await _kuras_context.Employee.FirstOrDefaultAsync(x => x.EmplId == uEmp.EmplId);
			else if (fEmp != null)
				newEmpId = await _kuras_context.Employee.FirstOrDefaultAsync(x => x.EmplId == fEmp.EmplId);
			return newEmpId != null ? newEmpId.Id : 0;
		}
		// Deletes all records in Axapta table and inserts new list
		private async Task UpdateTable(ICollection<Models.Axapta> axapta)
		{
			DeleteTable();

			foreach(var el in axapta)
			{
				_kuras_context.Add(el);
			}
			await _kuras_context.SaveChangesAsync();
		}
		// deletes all records in the table
		private void DeleteTable()
		{
			_kuras_context.Database.ExecuteSqlRaw("TRUNCATE TABLE Axapta");
		}
		// Separates the makeModel into make and model retrieved from Axapta Ds Auto Table
		private void SeparateMakeModel(string makeAndModel, ref string make, ref string model)
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
		}
		// if a car is found in kuras.Car table it returns true, if not - false
		private bool FindCar(DsAutoShort auto, ICollection<Models.Car> cars)
		{
			foreach (var car in cars)
			{
				if (auto.NumberPlate == car.NumberPlate)
				{
					return true;
				}
			}
			return false;
		}
		// if a car is found in kuras.Car table it returns true, if not - false, returns the car which had the same numberplate
		private bool FindCar(DsAutoShort auto, ICollection<Models.Car> cars, ref Models.Car fetchedCar)
		{
			foreach (var car in cars)
			{
				if (auto.NumberPlate == car.NumberPlate)
				{
					fetchedCar = car;
					return true;
					
				}
			}
			return false;
		}
	}
}
using ContactsManager.Core.DTO;
using CRUD.Filters.ActionFilters;
using CRUD.Filters.ExceptionFilters;
using CRUD.Filters.ResourceFilters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceContracts;
using ServiceContracts.Enums;
using ServiceContracts.IPersonsServices;

namespace CRUD.Controllers
{
    [TypeFilter<HanldeExceptionFilters>]
    [Route("[controller]/[action]")]

    public class HomeController : Controller
    {
        private readonly IPersonsGetterService _personsGetterService;
        private readonly IPersonsSorterService _personsSorterService;
        private readonly IPersonsUpdaterService _personsUpdateService;
        private readonly IPersonsAdderService _personsAdderService;
        private readonly IPersonsDeleterService _personsDeleterService;
        private readonly ICountriesService _countriesService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IPersonsGetterService personsGetterService, IPersonsSorterService personsSorterService, IPersonsUpdaterService personsUpdateService, IPersonsAdderService personsAdderService, IPersonsDeleterService personsDeleterService, ICountriesService countriesService, ILogger<HomeController> logger)
        {
            _personsGetterService = personsGetterService;
            _personsSorterService = personsSorterService;
            _personsUpdateService = personsUpdateService;
            _personsAdderService = personsAdderService;
            _personsDeleterService = personsDeleterService;
            _countriesService = countriesService;
            _logger = logger;
        }

        [TypeFilter(typeof(PersonsListActionFilter))]
        [TypeFilter(typeof(ResponseHeaderActionFilter),
            Arguments = new object[] { "I-CustomkEy", "I-CustomValue" })]
        [Route("/")]
        public async Task<IActionResult> Index(string searchBy, string? searchString, string sortBy = nameof(PersonReponse.PersonName),
            SortOrderOptions sortOrder = SortOrderOptions.ASC)
        {

            _logger.LogDebug("searchBy " + searchBy + " searchString" + sortBy + " sortBy" + sortOrder + " sortOrder");

            //Search
            List<PersonReponse> list_person = await _personsGetterService.GetFilterPerson(searchBy, searchString);

            //Sort
            List<PersonReponse> list_person_get_Sorted = await _personsSorterService.GetSortedPerson(list_person, sortBy, sortOrder);

            return View(list_person_get_Sorted);
        }


        public async Task<IActionResult> Create()
        {
            List<CountryReponse> countries = await _countriesService.GetAll();

            ViewBag.Coutries = countries.Select(x => new SelectListItem()
            {
                Value = x.CountryId.ToString(),
                Text = x.CountryName,
            });

            return View();
        }

        [HttpPost]
        [TypeFilter(typeof(FeatureDisableResourceFilters), Arguments = new object[] { false })]
        [TypeFilter<PersonCreateAndEditPostActionFilter>]

        public async Task<IActionResult> Create(PersonAddRequest personRequest)
        {

            //call service method
            PersonReponse personReponse = await _personsAdderService.AddPerson(personRequest);

            //navigate to Index() action method 
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid personID)
        {
            //call method 
            PersonReponse? person_reponse = await _personsGetterService.GetPersonById(personID);
            if (person_reponse == null) return new NotFoundResult();

            //convert to personUpdate
            PersonUpdateRequest personUpdate = person_reponse.ToPersonUpdateRequest();
            List<CountryReponse> countries = await _countriesService.GetAll();
            ViewBag.Countries = countries.Select(x => new SelectListItem()
            {
                Value = x.CountryId.ToString(),
                Text = x.CountryName,
            });

            return View(personUpdate);
        }

        [HttpPost]
        [TypeFilter<PersonCreateAndEditPostActionFilter>]
        public async Task<IActionResult> Edit(PersonUpdateRequest personRequest)
        {
            //callmethod

            PersonReponse personReponse = await _personsUpdateService.UpdatePerson(personRequest);

            //navigate to Index
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Delete(Guid personID)
        {
            //call method 
            await _personsDeleterService.DeletePerosn(personID);

            return RedirectToAction("Index");
        }

        [Route("/Error")]
        [AllowAnonymous]
        public IActionResult Error()
        {
            IExceptionHandlerPathFeature? exceptionHandlePath = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionHandlePath != null && exceptionHandlePath.Error != null)
            {
                ViewBag.ErrorMessage = exceptionHandlePath.Error.Message;
            }
            return View();
        }

    }




}

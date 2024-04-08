using AutoFixture;
using ContactsManager.Core.DTO;
using CRUD.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ServiceContracts;
using ServiceContracts.Enums;
using ServiceContracts.IPersonsServices;
namespace CRUDTest
{
    public class HomeControllerTest
    {

        private readonly IPersonsGetterService _personsGetterService;
        private readonly IPersonsAdderService _personsAdderService;
        private readonly IPersonsDeleterService _personsDeleterService;
        private readonly IPersonsUpdaterService _personsUpdaterService;
        private readonly IPersonsSorterService _personsSorterService;

        private readonly ICountriesService _countriesService;
        private readonly ILogger<HomeController> _logger;

        private readonly Mock<ICountriesService> _countriesServiceMock;
        private readonly Mock<IPersonsGetterService> _personsGetterServiceMock;
        private readonly Mock<IPersonsAdderService> _personsAdderServiceMock;
        private readonly Mock<IPersonsUpdaterService> _personsUpdaterServiceMock;
        private readonly Mock<IPersonsSorterService> _personsSorterServiceMock;
        private readonly Mock<IPersonsDeleterService> _personsDeleterServiceMock;


        private readonly Mock<ILogger<HomeController>> _loggerMock;

        private readonly Fixture _fixture;


        public HomeControllerTest()
        {
            _fixture = new Fixture();

            _countriesServiceMock = new Mock<ICountriesService>();
            _personsGetterServiceMock = new Mock<IPersonsGetterService>();
            _personsAdderServiceMock = new Mock<IPersonsAdderService>();
            _personsDeleterServiceMock = new Mock<IPersonsDeleterService>();
            _personsUpdaterServiceMock = new Mock<IPersonsUpdaterService>();
            _personsSorterServiceMock = new Mock<IPersonsSorterService>();

            _loggerMock = new Mock<ILogger<HomeController>>();

            _countriesService = _countriesServiceMock.Object;
            _personsGetterService = _personsGetterServiceMock.Object;
            _personsAdderService = _personsAdderServiceMock.Object;
            _personsUpdaterService = _personsUpdaterServiceMock.Object;
            _personsDeleterService = _personsDeleterServiceMock.Object;
            _personsSorterService = _personsSorterServiceMock.Object;

            _logger = _loggerMock.Object;
        }

        [Fact]
        public async Task Index_ShouldReturnIndexViewWithPersonList()
        {
            //Arrange
            HomeController homeController = new HomeController(_personsGetterService, _personsSorterService, _personsUpdaterService, _personsAdderService, _personsDeleterService, _countriesService, _logger);

            List<PersonReponse> person_reponse_list = _fixture.Create<List<PersonReponse>>();


            _personsGetterServiceMock.Setup(temp => temp.GetFilterPerson(It.IsAny<string>(), It.IsAny<String>()))
            .ReturnsAsync(person_reponse_list);

            _personsSorterServiceMock.Setup(temp => temp.GetSortedPerson(It.IsAny<List<PersonReponse>>(), It.IsAny<String>(),
                It.IsAny<SortOrderOptions>())).ReturnsAsync(person_reponse_list);

            //Acts

            IActionResult result = await homeController.Index(
                _fixture.Create<string>(), _fixture.Create<string>(),
                _fixture.Create<string>(), _fixture.Create<SortOrderOptions>());

            //Assert
            ViewResult viewResult = Assert.IsType<ViewResult>(result);

            viewResult.ViewData.Model.Should().BeAssignableTo<IEnumerable<PersonReponse>>();
            viewResult.ViewData.Model.Should().BeEquivalentTo(person_reponse_list);
        }


        [Fact]
        public async Task Create_IfNoErros_RedirectToIndex()
        {
            //Arrange
            HomeController homeController = new HomeController(_personsGetterService, _personsSorterService, _personsUpdaterService, _personsAdderService, _personsDeleterService, _countriesService, _logger);

            PersonAddRequest person_add_request = _fixture.Create<PersonAddRequest>();

            PersonReponse person_response = _fixture.Create<PersonReponse>();

            List<CountryReponse> list_country = _fixture.Create<List<CountryReponse>>();


            _countriesServiceMock.Setup(temp => temp.GetAll())
            .ReturnsAsync(list_country);

            _personsAdderServiceMock.Setup(temp => temp.AddPerson(It.IsAny<PersonAddRequest>())).ReturnsAsync(person_response);

            //Acts
            IActionResult result = await homeController.Create(person_add_request);



            //Assert
            RedirectToActionResult viewResult = Assert.IsType<RedirectToActionResult>(result);

            viewResult.ActionName.Should().Be("Index");
        }
    }
}
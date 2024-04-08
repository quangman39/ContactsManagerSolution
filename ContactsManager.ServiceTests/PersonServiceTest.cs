using AutoFixture;
using ContactsManager.Core.DTO;
using Enities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using RepositoriesContracts;
using ServiceContracts.Enums;
using ServiceContracts.IPersonsServices;
using Services;
using Services.PersonsServices;
using System.Linq.Expressions;
using Xunit.Abstractions;
namespace CRUDTest
{
    public class PersonServiceTest
    {
        private readonly IPersonsGetterService _personsGetterService;
        private readonly IPersonsAdderService _personsAdderService;
        private readonly IPersonsUpdaterService _personsUpdaterService;
        private readonly IPersonsDeleterService _personsDeleterService;
        private readonly IPersonsSorterService _personsSorterService;

        private readonly Mock<IPersonsRepository> _personRepositoryMock;
        private readonly IPersonsRepository _personsRepository;

        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFixture fixture;

        //constructor
        public PersonServiceTest(ITestOutputHelper testOutputHelper)
        {
            fixture = new Fixture();
            _personRepositoryMock = new Mock<IPersonsRepository>();
            _personsRepository = _personRepositoryMock.Object;

            var loggerMock = new Mock<ILogger<PersonsGetterService>>();

            _personsGetterService = new PersonsGetterService(_personsRepository, new Mock<ILogger<PersonsGetterService>>().Object);

            _personsAdderService = new PersonsAdderService(_personsRepository, new Mock<ILogger<PersonsAdderService>>().Object);

            _personsDeleterService = new PersonsDeleterService(_personsRepository, new Mock<ILogger<PersonsDeleterService>>().Object);

            _personsSorterService = new PersonsSorterService(_personsRepository, new Mock<ILogger<PersonsSorterService>>().Object);

            _personsUpdaterService = new PersonsUpdaterService(_personsRepository, new Mock<ILogger<PersonsUpdaterService>>().Object);

            _testOutputHelper = testOutputHelper;
        }



        #region AddPerson

        //it should throw ArgumentNull when personAddReques is assign null
        [Fact]
        public async Task AddPerson_NullArg_ToBeArgumentNullException()
        {
            // Arrange
            PersonAddRequest? personAddRequest = null;

            //Acts 
            Func<Task> action = async () => await _personsAdderService.AddPerson(personAddRequest);

            // Assert

            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        //when Name is null, it should throw ArgumentException
        public async Task AddPerson_NullName_ToBeArgumentException()
        {
            //Arrange
            PersonAddRequest? personAddRequest = fixture.Build<PersonAddRequest>().With(temp => temp.Email, "someone@example.com")
                                                        .With(temp => temp.PersonName, null as string).Create();

            Person person = personAddRequest.ToPerson();
            //When PersonRepository.AddPerson is called, it has to return the same object
            _personRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>()))
                                 .ReturnsAsync(person);


            //Acts 
            Func<Task> action = async () => await _personsAdderService.AddPerson(personAddRequest);

            // Assert

            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        //when supplies proper arg, it should return obj of PersonReponse which include the newly generated Id 
        public async Task Add_Person_ProperArg_ToBeSuccessFull()
        {
            //Arrange
            PersonAddRequest personAddRequest = fixture.Build<PersonAddRequest>()
                                                        .With(temp => temp.Email, "someone@example.com")
                                                        .Create();
            Person person = personAddRequest.ToPerson();

            PersonReponse person_reponse_expected = person.ToPersonReoponse();

            //IF we supply any agrument method to the AddPerson method, it should return same return value    
            _personRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>())).ReturnsAsync(person);

            //Act
            PersonReponse personReponse_fromAdd = await _personsAdderService.AddPerson(personAddRequest);
            person_reponse_expected.PersonID = personReponse_fromAdd.PersonID;

            //Assert
            personReponse_fromAdd.PersonID.Should().NotBe(Guid.Empty);
            personReponse_fromAdd.Should().Be(person_reponse_expected);



        }

        #endregion

        #region GetPersonId

        [Fact]
        //When PersonId is null, it should return null
        public async Task GetPersonById_NullId_ToBeNull()
        {
            //Arrange
            Guid? personId = null;

            PersonReponse? personReponse = await _personsGetterService.GetPersonById(personId);

            //Assert
            personReponse.Should().BeNull();

        }

        [Fact]
        //When supplies wrong id , it should return null obj
        public async Task GetPersonById_WrongId()
        {
            //Arrange
            Guid? personId = Guid.NewGuid();

            PersonReponse? personReponse = await _personsGetterService.GetPersonById(personId);

            //Assert

            personReponse.Should().BeNull();



        }


        [Fact]
        //When supplies proper PersonId , it should return null obj
        public async Task GetPersonById_ProperId_ToBeSuccessFull()
        {
            //Arrange
            Person person = fixture.Build<Person>()
                                    .With(temp => temp.Country, null as Country)
                                         .With(temp => temp.Email, "someone@example.com")
                                         .Create();
            PersonReponse person_reponse_expected = person.ToPersonReoponse();

            _personRepositoryMock.Setup(temp => temp.GetPersonById(It.IsAny<Guid>())).ReturnsAsync(person);

            //Act
            PersonReponse? personReponse = await _personsGetterService.GetPersonById(person.PersonID);

            //Assert

            personReponse.Should().Be(person_reponse_expected);



        }

        #endregion

        #region GetAllPerson
        [Fact]
        // List of person shoulde emty before add person
        public async Task GetAllPerson_BeforeAdd_ToBeEmty()
        {
            var person = new List<Person>();
            _personRepositoryMock.Setup(temp => temp.GetAllPersons())
                                .ReturnsAsync(person);
            //Arrange 
            List<PersonReponse>? list_person_from_getAll = await _personsGetterService.GetAllPerson();


            //Assert
            list_person_from_getAll.Should().BeEmpty();

        }

        [Fact]
        // when applies proper persons should return proper list PersonReponse
        public async Task GetAllPerson_ProperArgs_ToBeSuccessFul()
        {
            //Arrange 
            Person Person1 = fixture.Build<Person>()
                                    .With(temp => temp.Country, null as Country)
                                      .With(temp => temp.Email, "someone@example.com")
                                      .Create();
            Person Person2 = fixture.Build<Person>()
                                    .With(temp => temp.Country, null as Country)
                                     .With(temp => temp.Email, "someone@example.com")
                                     .Create();
            Person Person3 = fixture.Build<Person>()
                                    .With(temp => temp.Email, "someone@example.com")
                                    .With(temp => temp.Country, null as Country)
                                     .Create();

            List<Person> PersonList = new List<Person>
            {Person1,Person2,Person3};
            List<PersonReponse> list_person_reponse_expected = PersonList.Select(temp => temp.ToPersonReoponse()).ToList();

            _personRepositoryMock.Setup(temp => temp.GetAllPersons()).ReturnsAsync(PersonList);

            //Acts
            List<PersonReponse> list_from_getAll = await
                _personsGetterService.GetAllPerson();

            //Assert

            list_from_getAll.Should().BeEquivalentTo(list_person_reponse_expected);
        }
        #endregion

        #region GetFilterPerson
        [Fact]
        ///If the search text is emty and search by "PersonName", it should return all person
        public async Task GetFilterPerson_EmtySearch_ToBeSuccessful()
        {
            //Arrange
            List<Person> persons = new()
            {
                fixture.Build<Person>().With(temp => temp.Email, "someone.@gmail.com")
                .With(temp => temp.PersonName, "ma")
                .With(temp => temp.Country, null as Country)
                .Create(),

                fixture.Build<Person>().With(temp => temp.Email, "someone.@gmail.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                fixture.Build<Person>().With(temp => temp.Email , "someone.@gmail.com")
                .With(temp => temp.Country, null as Country)
                .Create()
            };


            var person_reponse_list = persons.Select(e => e.ToPersonReoponse()).ToList();
            _personRepositoryMock.Setup(temp => temp.GetFilterPersons(It.IsAny<Expression<Func<Person, bool>>>())).ReturnsAsync(persons);

            //Acts
            List<PersonReponse> list_from_filter = await _personsGetterService.GetFilterPerson(nameof(Person.PersonName), " ");

            //Asserts
            list_from_filter.Should().BeEquivalentTo(person_reponse_list);
        }


        // Searching based on person name with some search string
        //it should return matching person
        [Fact]
        public async Task GetFilterPerson_SearchByPersonName_ToBeSuccesful()
        {
            //Arrange
            List<Person> persons = new()
            {
                fixture.Build<Person>().With(temp => temp.Email, "someone.@gmail.com")
                .With(temp => temp.Country, null as Country)
                .With(temp => temp.PersonName,"man")
                .Create(),

                fixture.Build<Person>().With(temp => temp.Email, "someone.@gmail.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                fixture.Build<Person>().With(temp => temp.Email, "someone.@gmail.com")
                .With(temp => temp.Country, null as Country)
                .Create()
            };


            var person_reponse_list = persons.Select(e => e.ToPersonReoponse()).ToList();
            _personRepositoryMock.Setup(temp => temp.GetFilterPersons(It.IsAny<Expression<Func<Person, bool>>>())).ReturnsAsync(persons);


            //Acts
            List<PersonReponse> list_to_from_filted = await _personsGetterService.GetFilterPerson(nameof(Person.PersonName), "sa");

            //Asserts
            list_to_from_filted.Should().BeEquivalentTo(person_reponse_list);

        }


        #endregion

        #region GetSortedPerson
        [Fact]
        //when person based on PersonName in DESC, it should return persons list in descending on PersonName
        public async Task GetSortedPerson_ToBeSuccessful()
        {

            //Arrange 
            List<Person> persons = new()
            {
                fixture.Build<Person>().With(temp => temp.Email, "someone.@gmail.com")
                .With(temp => temp.Country, null as Country)
                .With(temp => temp.PersonName,"man")
                .Create(),

                fixture.Build<Person>().With(temp => temp.Email, "someone.@gmail.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

                fixture.Build<Person>().With(temp => temp.Email, "someone.@gmail.com")
                .With(temp => temp.Country, null as Country)
                .Create()
            };

            _personRepositoryMock.Setup(temp => temp.GetAllPersons()).ReturnsAsync(persons);
            List<PersonReponse> list_from_getAll = await _personsGetterService.GetAllPerson();

            //Act 
            List<PersonReponse> persons_list_from_search = await _personsSorterService.GetSortedPerson(list_from_getAll, nameof(PersonReponse.DateOfBirth), SortOrderOptions.DESC);

            //Assert
            persons_list_from_search.Should().BeInDescendingOrder(temp => temp.DateOfBirth);
        }



        #endregion

        #region UpdatePerson

        //when we supply null as PersonUpdateRequest, we should throw ArgNullException
        [Fact]
        public async Task UpdatePerson_nullArg_ToBeArgumentNullException()
        {
            //Arrange
            PersonUpdateRequest personUpdateRequest = null;

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _personsUpdaterService.UpdatePerson(personUpdateRequest);
            });
        }
        //when we supply a invalid id,it  should throw ArgException
        [Fact]
        public async Task UpdatePerson_InvalidId_ToBeArgumentNullException()
        {
            //Arrange
            PersonUpdateRequest personUpdateRequest = new()
            {
                PersonId = Guid.NewGuid(),
            };

            //Act
            Func<Task> action = async () =>
            {
                await _personsUpdaterService.UpdatePerson(personUpdateRequest);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        //when PersonName is null,it  should throw ArgException
        [Fact]
        public async Task UpdatePerson_PersonNameisNull_ToBeArgumentException()
        {
            //Arrange
            PersonUpdateRequest person_update = fixture.Build<PersonUpdateRequest>()
                                                .Create();

            person_update.PersonName = null;


            //Acts
            Func<Task> action = async () =>
            {
                await _personsUpdaterService.UpdatePerson(person_update);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        // Add new person in obj, and update Name and Email
        [Fact]
        public async Task UpdatePerson_UpdateNameandEmail_ToBeSuccesful()
        {
            //Arrange
            Person person = fixture.Build<Person>().With(temp => temp.Country, null as Country)
                                                    .With(temp => temp.Gender, "Male")
                                                      .With(temp => temp.Email, "a@gmail.com")
                                                      .Create();
            PersonReponse person_expected = person.ToPersonReoponse();
            PersonUpdateRequest person_update_request = person_expected.ToPersonUpdateRequest();

            _personRepositoryMock.Setup(temp => temp.GetPersonById(It.IsAny<Guid>())).ReturnsAsync(person);
            _personRepositoryMock.Setup(temp => temp.UpdatePerson(It.IsAny<Person>())).ReturnsAsync(person);

            //Act
            PersonReponse person_reponse_from_update = await _personsUpdaterService.UpdatePerson(person_update_request);

            //Assert
            person_reponse_from_update.Should().Be(person_expected);



        }


        #endregion

        #region Delete Person

        //when supply null id, it should return ArgumentNull
        [Fact]
        public async Task DeletePerson_nullArg()
        {
            //Arrange
            Guid? personId = null;

            //Acts
            Func<Task> action = async () =>
            {
                await _personsDeleterService.DeletePerosn(personId);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }




        // when supplies valid value, it should delete obj in person list and return true
        [Fact]
        public async Task DeletePerson_ValidPerson_ToBeSuccessful()
        {
            //Arrange
            Person person = fixture.Build<Person>().With(temp => temp.Country, null as Country)
                                                    .With(temp => temp.Gender, "Male")
                                                      .With(temp => temp.Email, "a@gmail.com")
                                                      .Create();

            _personRepositoryMock.Setup(temp => temp.GetPersonById(It.IsAny<Guid>())).ReturnsAsync(person);
            _personRepositoryMock.Setup(temp => temp.DeletePersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(true);

            //Act
            Boolean isDeleted = await _personsDeleterService.DeletePerosn(person.PersonID);

            //Assert
            isDeleted.Should().BeTrue();
        }


        // when supplies invalid value, it should  return failse
        [Fact]
        public async Task DeletePerson_InValidPerson_ToBeAgrumentException()
        {
            //Arrange
            Guid personId = Guid.NewGuid();

            //Act
            Boolean isDeleted = await _personsDeleterService.DeletePerosn(personId);

            //Assert
            isDeleted.Should().BeFalse();
        }

        #endregion
    }
}

using ContactsManager.Core.Domain.IndentityEntites;
using Enities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Repositories;
using RepositoriesContracts;
using ServiceContracts;
using ServiceContracts.IPersonsServices;

using Services;
using Services.PersonsServices;

namespace CRUD.StartupExtensions
{
    public static class ConfigureServicesExtension
    {

        public static IServiceCollection ConfigureService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllersWithViews();
            //add services in ioc container
            services.AddScoped<ICountriesRepository, CountriesRepository>();
            services.AddScoped<IPersonsRepository, PersonsRepository>();

            services.AddScoped<ICountriesService, CountriesService>();
            services.AddScoped<IPersonsAdderService, PersonsAdderService>();
            services.AddScoped<IPersonsDeleterService, PersonsDeleterService>();
            services.AddScoped<IPersonsGetterService, PersonsGetterService>();
            services.AddScoped<IPersonsUpdaterService, PersonsUpdaterService>();
            services.AddScoped<IPersonsSorterService, PersonsSorterService>();



            services.AddDbContext<ApplicationDbContext>(
               options =>
               {
                   options.UseSqlServer(configuration.GetConnectionString("DefaultConection"));
               });
            //add identity in this project 
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 5;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;

            })
                    .AddEntityFrameworkStores<ApplicationDbContext>()

                    .AddDefaultTokenProviders()

                    .AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()

                    .AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>();


            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                //enforce authoriation policy ( user must be authencated) fot all the action method
                options.AddPolicy("NotAuthencated", policy =>
                {
                    policy.RequireAssertion(context =>
                    {
                        return !context.User.Identity.IsAuthenticated;
                    });
                });
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
            });



            return services;
        }



    }
}

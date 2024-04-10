using CRUD.StartupExtensions;
using Serilog;
var builder = WebApplication.CreateBuilder(args);

//config serilog
builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(context.Configuration)//read configuration setting from buil-in configuration

    .ReadFrom.Services(services);
    // read our current app's service  and make them avaiable to serilog

});

//using configure file to add services
builder.Services.ConfigureService(builder.Configuration);


var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseExceptionHandlingMiddleware();
}

app.UseHsts();
app.UseHttpsRedirection();// using https 


app.UseStaticFiles();
app.UseRouting(); //Indetity action method based on rule
app.UseAuthentication();// read Identity cookies 
app.UseAuthorization();//add Authorization
app.MapControllers();


#pragma warning disable ASP0014 // Suggest using top level route registrations
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
      name: "areas",
      pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );
    /*app.MapControllerRoute(
       name: "default",
      pattern: "{controller=Home}/{action=Index}/{id?}")*/
    ; //Execute the filter pineline (action + filter)
});
#pragma warning restore ASP0014 // Suggest using top level route registrations



app.Run();


public partial class Program { } //make the auto generated program accessible programmatically
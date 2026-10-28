using System.Text;
using CadetTest.Entities;
using CadetTest.Helpers;
using CadetTest.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// in memory database used for simplicity, change to a real db for production applications
builder.Services.AddDbContext<DataContext>(x => x.UseInMemoryDatabase("CadetTestDb"));

builder.Services.AddControllersWithViews()
    .AddJsonOptions(x => x.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull);

// configure strongly typed settings objects
var appSettingsSection = builder.Configuration.GetSection("AppSettings");
builder.Services.Configure<AppSettings>(appSettingsSection);

// configure jwt authentication
var appSettings = appSettingsSection.Get<AppSettings>();
var key = Encoding.ASCII.GetBytes(appSettings.Secret);
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
            ClockSkew = TimeSpan.Zero
        };
    });

// configure DI for application services
builder.Services.AddScoped<IDataService, DataService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddSwaggerGen();

var app = builder.Build();

// seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DataContext>();
    var dataService = scope.ServiceProvider.GetRequiredService<IDataService>();

    context.Users.Add(new User { Username = "User-5E638711-2D64-47B0-A8F5-1C5A9EADA966", Password = "Pass-E4A679C7-2F4B-40AE-9DC8-C967EF7215AE" });
    context.Users.Add(new User { Username = "User-1A30567E-C121-44DF-8008-BCEBEC01BF0A", Password = "Pass-095584F8-0EFB-4F21-B3FB-7DB0899AE4A7" });
    context.Users.Add(new User { Username = "User-58076950-C311-4DD1-88DE-E682C164E3BE", Password = "Pass-15D6617D-721B-46D6-AC9C-B50E8EA207E0" });
    context.Users.Add(new User { Username = "User-DB42737F-6AAA-453C-A5E8-72EB5229C18C", Password = "Pass-5DD8829F-0DCA-4140-A022-048CB5964D36" });

    for (int i = 1; i <= appSettings.ConsentCount; i++)
    {
        var consent = new Consent
        {
            Recipient = $"{dataService.GetRandomString(10)}_{i}@ornek.com",
            RecipientType = "BIREYSEL",
            Status = "ONAY",
            Type = "EPOSTA"
        };

        context.Consents.Add(consent);
    }

    context.SaveChanges();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Enable middleware to serve generated Swagger as a JSON endpoint.
app.UseSwagger();

// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
// specifying the Swagger JSON endpoint.
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bilmed Cadet Test API");
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

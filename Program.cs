using HillsModelManager;
using HillsModelManager.Database;
using HillsModelManager.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//builder.Services.AddDbContext<TrainDBContext>(ServiceLifetime.Singleton);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<TrainService>();

var app = builder.Build();

// // Migrate latest database changes during startup
// using (var scope = app.Services.CreateScope())
// {
//     var dbContext = scope.ServiceProvider.GetRequiredService<TrainDBContext>();
//     dbContext.Database.Migrate();
// }

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles(new TrainerFileOptions(app.Configuration));
app.UseStaticFiles(new StaticFileOptions()
{
	ContentTypeProvider = new FileExtensionContentTypeProvider(
		new Dictionary<string, string>{
			{ ".pkl","application/octet-stream"},
			{ ".png", "image/png" },
			{ ".csv", "application/octet-stream" },
		}
	)
});
app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
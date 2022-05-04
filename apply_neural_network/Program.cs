using apply_neural_network.RabbitMq;
using apply_neural_network.databases;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IRabbitMqService, RabbitMqService>();
builder.Services.AddMvc();
// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapGet("/", () => "Hello World!");

// app.MapGet("/api/upload",);
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.MapRazorPages();

app.Run();

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
    {
        // Geliştirme sırasında tarayıcı eski dosyayı önbellekten vermesin
        if (app.Environment.IsDevelopment())
        {
            context.Context.Response.Headers["Cache-Control"] = "no-store";
        }
    }
});

app.Run();
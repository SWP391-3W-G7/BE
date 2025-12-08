namespace LostFoundApi
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Required for Swashbuckle CLI to generate OpenAPI
            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Required for Swashbuckle CLI — even if no actual app is run
            app.UseSwagger();
        }
    }

}

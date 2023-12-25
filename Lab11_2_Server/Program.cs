using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Server
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }
    }

    public class NorthwindContext : DbContext
    {
        public NorthwindContext(DbContextOptions<NorthwindContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly NorthwindContext _context;

        public EmployeesController(NorthwindContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public ActionResult<Employee> GetEmployee(int id)
        {
            var employee = _context.Employees.Find(id);
            if (employee == null)
            {
                return NotFound();
            }
            return employee;
        }

        [HttpPost]
        public ActionResult<Employee> CreateEmployee(Employee employee)
        {
            _context.Employees.Add(employee);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetEmployee), new { id = employee.EmployeeId }, employee);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateEmployee(int id, Employee employee)
        {
            if (id != employee.EmployeeId)
            {
                return BadRequest();
            }

            _context.Entry(employee).State = EntityState.Modified;
            _context.SaveChanges();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteEmployee(int id)
        {
            var employee = _context.Employees.Find(id);
            if (employee == null)
            {
                return NotFound();
            }

            _context.Employees.Remove(employee);
            _context.SaveChanges();
            return NoContent();
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            //var host = CreateHostBuilder(args).Build();
            //await host.RunAsync();

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddDbContext<NorthwindContext>(opt =>
                opt.UseSqlServer("Data Source=localhost;Initial Catalog=Northwind;Persist Security Info=True;User ID=sa;Password=HelloWorld10", providerOptions => { providerOptions.EnableRetryOnFailure(); }));
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "TodoApi", Version = "v1" });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (builder.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TodoApi v1"));
            }

            app.UseHttpsRedirection();

            app.MapControllers();

            app.Run();


        }

        static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder.Configure(app =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGet("/api/employees/{id}", async context =>
                            {
                                var id = int.Parse(context.Request.RouteValues["id"] as string);
                                var dbContext = context.RequestServices.GetRequiredService<NorthwindContext>();
                                var employee = await dbContext.Employees.FindAsync(id);
                                if (employee != null)
                                {
                                    await context.Response.WriteAsJsonAsync(employee);
                                }
                                else
                                {
                                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                                }
                            });
                        });
                    });
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddDbContext<NorthwindContext>(options =>
                        options.UseSqlServer(hostContext.Configuration.GetConnectionString("Persist Security Info=False;Integrated Security=true;Initial Catalog=Northwind;server=(local)")));
                });
        }
    }

}
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);

//Add DbContext 
builder.Services.AddDbContext<ExpenseDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("ExpenseDbContext")));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Expense API",
        Description = "Personal Expense Tracker",
        Version = "v1"
    });
});

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Expense  API V1");
    });
}


app.MapGet("/", () => "Hello World!");

//Get All expenses
app.MapGet("/expenses", async (ExpenseDbContext dbContext) => await dbContext.Expenses.ToListAsync());

//Get expense by id
app.MapGet("/expense/{id}", async (int id, ExpenseDbContext dbContext) => await dbContext.Expenses.FindAsync(id)
    is Expense exps ? Results.Ok(exps) : Results.NotFound());


//Add expense
app.MapPost("/expense", async (Expense exps, ExpenseDbContext dbContext) =>
{
    exps.CreatedDate = DateTime.Now;
    dbContext.Expenses.AddAsync(exps);
    await dbContext.SaveChangesAsync();
    return Results.Created($"/expense/{exps.Id}", exps);
});

//Update expense
app.MapPut("/expense/{id}", async (int id, Expense exps, ExpenseDbContext dbContext) =>
{
    var currentExps = await dbContext.Expenses.FindAsync(id);
    if (currentExps is null) return Results.NotFound();
    currentExps.Name = exps.Name;
    currentExps.Amount = exps.Amount;
    await dbContext.SaveChangesAsync();
    return Results.Ok(currentExps);
});

app.MapDelete("/expense/{id}", async (int id, ExpenseDbContext dbContext) =>
{
    var currentExps = await dbContext.Expenses.FindAsync(id);
    if (currentExps is null) return Results.NotFound();

    dbContext.Expenses.Remove(currentExps);
    await dbContext.SaveChangesAsync();
    return Results.Ok(currentExps);

});

app.Run();



class Expense
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedDate { get; set; }

}
class ExpenseDbContext : DbContext
{
    public ExpenseDbContext(DbContextOptions<ExpenseDbContext> options) : base(options) { }
    public DbSet<Expense> Expenses => Set<Expense>();
}


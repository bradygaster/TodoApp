using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TodoDbContext>(optionsBuilder => optionsBuilder.UseSqlServer(builder.Configuration.GetConnectionString("TodoDb")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddApplicationMapName();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
        db.Database.EnsureCreated();
    }
}

// Create a new todo
app.MapPost("/todos", async (TodoDbContext dbContext, Todo todo) =>
    {
        dbContext.Todos.Add(todo);
        await dbContext.SaveChangesAsync();

        return Results.Created($"/todo/{todo.Id}", todo);
    })
    .WithName("CreateTodo")
    .Produces(StatusCodes.Status409Conflict)
    .Produces<Todo>(StatusCodes.Status201Created);

// Retrieve all todos
app.MapGet("/todos", async (TodoDbContext dbContext) => await dbContext.Todos.ToListAsync())
   .WithName("GetAllTodos")
   .Produces<List<Todo>>();

// Retrieve a specific todo
app.MapGet("/todo/{id}", async (TodoDbContext dbContext, int id) =>
    await dbContext.Todos.FirstOrDefaultAsync(_ => _.Id == id)
    is Todo s
        ? Results.Ok(s)
        : Results.NotFound())
    .WithName("GetTodo")
    .Produces(StatusCodes.Status404NotFound)
    .Produces<Todo>(StatusCodes.Status200OK);

// Update an existing todo
app.MapPut("/todo/{id}", async (TodoDbContext dbContext, int id, Todo todo) =>
    {
        var existing = await dbContext.Todos.FirstOrDefaultAsync(_ => _.Id == id);
        if (existing == null) return Results.NotFound();

        existing.Title = todo.Title;
        existing.IsCompleted = todo.IsCompleted;

        dbContext.Update(existing);
        await dbContext.SaveChangesAsync();

        return Results.Accepted($"/todo/{id}", todo);
    })
    .WithName("UpdateTodo")
    .Produces(StatusCodes.Status404NotFound)
    .Produces<Todo>(StatusCodes.Status202Accepted);

// Delete a specific todo
app.MapDelete("/todo/{id}", async (TodoDbContext dbContext, int id) =>
    {
        var existing = await dbContext.Todos.FirstOrDefaultAsync(_ => _.Id == id);
        if (existing == null) return Results.NotFound();
        dbContext.Todos.Remove(existing);
        await dbContext.SaveChangesAsync();
        return Results.Ok();
    })
    .WithName("DeleteTodo")
    .Produces(StatusCodes.Status404NotFound)
    .Produces<Todo>(StatusCodes.Status200OK);

app.Run();

public class Todo
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; } = false;
}

public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) { }

    public DbSet<Todo> Todos => Set<Todo>();
}
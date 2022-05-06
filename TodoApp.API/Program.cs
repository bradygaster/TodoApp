using Microsoft.EntityFrameworkCore;
using TodoApp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TodoDbContext>(optionsBuilder => optionsBuilder.UseSqlServer(builder.Configuration.GetValue<string>("TodoDb")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddApplicationMapName();
builder.Services.AddSwaggerGen();
builder.Services.AddDirectoryBrowser();

var app = builder.Build();
app.UseFileServer();
app.UseStaticFiles();

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

// static UI
app.MapGet("/", (IWebHostEnvironment env) => env.FromStaticFile("index.html"));

app.Run();

public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) { }

    public DbSet<Todo> Todos => Set<Todo>();
}

static class ResultsExtensions
{
    public static IResult FromStaticFile(this IWebHostEnvironment env, string filename, string contentType = "text/html")
    {
        var memoryStream = new MemoryStream();
        using (var stream = File.OpenRead(Path.Combine(env.WebRootPath, filename)))
        {
            stream.CopyTo(memoryStream);
        }

        memoryStream.Position = 0;
        return Results.Stream(memoryStream, contentType: contentType);
    }
}
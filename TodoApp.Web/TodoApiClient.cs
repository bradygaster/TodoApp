using Refit;
using System.ComponentModel.DataAnnotations;

public record Todo(int Id, [Required] string Title)
{
    public bool IsCompleted { get; set; } = false;
}

public interface ITodoApiClient
{
    [Get("/todos")]
    Task<IEnumerable<Todo>> GetTodos();

    [Get("/todo/{id}")]
    Task<Todo> GetTodo(int id);

    [Post("/todos")]
    Task<Todo> CreateTodo(Todo todo);

    [Delete("/todo/{id}")]
    Task DeleteTodo(int id);

    [Put("/todo/{id}")]
    Task UpdateTodo(int id, Todo todo);
}

public class TodoApiClient : ITodoApiClient
{
    private readonly HttpClient _httpClient;

    private readonly IConfiguration _configuration;

    public TodoApiClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _httpClient.BaseAddress = new Uri(_configuration["ApiUrlBase"]);
    }

    public async Task<Todo> CreateTodo(Todo todo)
    {
        return await RestService.For<ITodoApiClient>(_httpClient).CreateTodo(todo);
    }

    public async Task DeleteTodo(int id)
    {
        await RestService.For<ITodoApiClient>(_httpClient).DeleteTodo(id);
    }

    public async Task<Todo> GetTodo(int id)
    {
        return await RestService.For<ITodoApiClient>(_httpClient).GetTodo(id);
    }

    public async Task<IEnumerable<Todo>> GetTodos()
    {
        return await RestService.For<ITodoApiClient>(_httpClient).GetTodos();
    }

    public async Task UpdateTodo(int id, Todo todo)
    {
        await RestService.For<ITodoApiClient>(_httpClient).UpdateTodo(id, todo);
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddTodoApiClient(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddSingleton<ITodoApiClient, TodoApiClient>();
        }
    }
}
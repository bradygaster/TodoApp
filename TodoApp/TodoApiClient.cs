namespace TodoApp
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Refit;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Net.Http;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    public class Todo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required]
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        
        [JsonPropertyName("isCompleted")]
        public bool IsCompleted { get; set; } = false;
    }

    public interface ITodoApiClient
    {
        [Get("/todos")]
        Task<IEnumerable<Todo>> GetTodos();

        [Get("/todo/{id}")]
        Task<Todo> GetTodo(int id);

        [Post("/todos")]
        Task CreateTodo([Body] Todo todo);

        [Delete("/todo/{id}")]
        Task DeleteTodo(int id);

        [Put("/todo/{id}")]
        Task UpdateTodo(int id, Todo todo);
    }

    public class TodoApiClient : ITodoApiClient
    {
        private readonly HttpClient _httpClient;

        private readonly IConfiguration _configuration;

        private readonly ILogger<TodoApiClient> _logger;

        public TodoApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<TodoApiClient> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri(_configuration["ApiUrlBase"]);
            _logger = logger;
        }

        public async Task CreateTodo([Body] Todo todo)
        {
            _logger.LogInformation($"ToDoApp: Client sending new Todo with title '{todo.Title}'.");
            await RestService.For<ITodoApiClient>(_httpClient).CreateTodo(todo);
            _logger.LogInformation($"ToDoApp: Client sent new Todo with title '{todo.Title}'.");
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
}

namespace Microsoft.Extensions.DependencyInjection
{
    using TodoApp;

    public static class TodoApiClientServiceCollectionExtensions
    {
        public static void AddTodoApiClient(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddSingleton<ITodoApiClient, TodoApiClient>();
        }
    }
}

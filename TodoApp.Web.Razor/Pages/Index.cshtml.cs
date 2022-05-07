using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TodoApp.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel>? _logger;
        private readonly ITodoApiClient _todoApiClient;

        [BindProperty]
        public Todo? NewTodo { get; set; }

        public IndexModel(ILogger<IndexModel> logger, ITodoApiClient todoApiClient)
        {
            _logger = logger;
            _todoApiClient = todoApiClient;
        }

        public IEnumerable<Todo>? Todos { get; private set; }

        public async Task OnGet()
        {
            Todos = await _todoApiClient.GetTodos();
        }

        public async Task<IActionResult> OnPostCreateNewTodoAsync()
        {
            _logger.LogInformation($"ToDoApp: Inside OnPostCreateNewTodoAsync in Razor client about to post. New todo item title is '{NewTodo.Title}' - Is Model State Valid: {ModelState.IsValid}");

            if (!ModelState.IsValid)
            {
                _logger.LogInformation("ToDoApp: Model state was invalid. Refreshing todos.");
                Todos = await _todoApiClient.GetTodos();
                return Page();
            }

            if (NewTodo != null)
            {
                _logger.LogInformation($"ToDoApp: Model state was valid. Logging new todo item with title of '{NewTodo.Title}'");
                await _todoApiClient.CreateTodo(NewTodo.Title);
                _logger.LogInformation("ToDoApp: Called API client without any errors.");
            }

            _logger.LogInformation("ToDoApp: Redirecting back to Razor index page.");
            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostDeleteTodoAsync(int todoId)
        {
            await _todoApiClient.DeleteTodo(todoId);
            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostMarkCompleteAsync(int todoId)
        {
            var todo = await _todoApiClient.GetTodo(todoId);
            todo.IsCompleted = !todo.IsCompleted;
            await _todoApiClient.UpdateTodo(todoId, todo);

            return RedirectToPage("./Index");
        }
    }
}
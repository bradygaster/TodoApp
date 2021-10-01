using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TodoApp.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ITodoApiClient _todoApiClient;

        [BindProperty]
        public Todo NewTodo { get; set; }

        public IndexModel(ILogger<IndexModel> logger, ITodoApiClient todoApiClient)
        {
            _logger = logger;
            _todoApiClient = todoApiClient;
        }

        public IEnumerable<Todo> Todos { get; private set; }

        public async Task OnGet()
        {
            Todos = await _todoApiClient.GetTodos();
        }

        public async Task<IActionResult> OnPostCreateNewTodoAsync()
        {
            if (!ModelState.IsValid)
            {
                Todos = await _todoApiClient.GetTodos();
                return Page();
            }

            await _todoApiClient.CreateTodo(NewTodo);

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
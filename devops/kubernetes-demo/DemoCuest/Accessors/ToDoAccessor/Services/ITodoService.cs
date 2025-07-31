using ToDoAccessor.Models;

namespace ToDoAccessor.Services
{
    public interface ITodoService
    {
        Task<List<Todo>> GetAllTodosAsync();
        Task<Todo?> GetTodoByIdAsync(string id);
        Task AddTodoAsync(Todo todo);
        Task<bool> UpdateTodoAsync(string id, Todo updatedTodo);
        Task<bool> DeleteTodoAsync(string id);
    }

}

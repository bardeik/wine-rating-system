using Microsoft.EntityFrameworkCore;
using WineApp.Models;

namespace WineApp.Data;

public class TodosRepository : ITodosRepository
{
    private readonly IDbContextFactory<WineAppDbContext> _contextFactory;

    public TodosRepository(IDbContextFactory<WineAppDbContext> contextFactory) =>
        _contextFactory = contextFactory;

    public IList<TodoItem> GetAllTodoItems()
    {
        using var context = _contextFactory.CreateDbContext();
        return context.TodoItems.ToList();
    }

    public TodoItem? GetTodoItemById(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        return context.TodoItems.Find(id);
    }

    public long AddTodoItem(TodoItem todoItem)
    {
        using var context = _contextFactory.CreateDbContext();
        context.TodoItems.Add(todoItem);
        context.SaveChanges();
        return todoItem.Id;
    }

    public void DeleteTodoItem(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var todo = context.TodoItems.Find(id);
        if (todo != null)
        {
            context.TodoItems.Remove(todo);
            context.SaveChanges();
        }
    }

    public void MarkTodoItemAsDone(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var todo = context.TodoItems.Find(id);
        if (todo != null)
        {
            todo.IsCompleted = true;
            context.SaveChanges();
        }
    }
}



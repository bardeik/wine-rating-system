using WineApp.Models;

namespace WineApp.Data;

public class TodosRepository : ITodosRepository
{
    private readonly List<TodoItem> todoItems = new()
    {
        new TodoItem
        {   Id = 1,
            Name = "Prepare wines for tasting event",
            CreatedAt =  new DateTime(2015, 12, 04),
            DueDate = new DateTime(2016, 12, 01),
        },
        new TodoItem
        {   Id = 2,
            Name = "Review judge ratings for consistency",
            CreatedAt =  new DateTime(2015, 12, 04),
            DueDate = new DateTime(2016, 08, 31),
        },
        new TodoItem
        {   Id = 3,
            Name = "Update wine producer information",
            CreatedAt =  new DateTime(2015, 12, 04),
            DueDate = new DateTime(2016,10, 15),
        }
    };

    public IList<TodoItem> GetAllTodoItems() => todoItems;

    public TodoItem? GetTodoItemById(int id) => 
        todoItems.Find(todoItem => todoItem.Id == id);

    public long AddTodoItem(TodoItem todoItem)
    {
        var newId = todoItems.Count > 0 ? todoItems.Max(x => x.Id) + 1 : 1;
        todoItem.Id = newId;
        todoItems.Add(todoItem);
        return newId;
    }

    public void DeleteTodoItem(int id)
    {
        var todo = todoItems.SingleOrDefault(x => x.Id == id);
        if (todo != null)
            todoItems.Remove(todo);
    }

    public void MarkTodoItemAsDone(int id)
    {
        var todo = todoItems.SingleOrDefault(x => x.Id == id);
        if (todo != null)
            todo.IsCompleted = true;
    }
}



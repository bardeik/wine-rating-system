using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WineApp.Models;

namespace WineApp.Data
{
    public interface ITodosRepository
    {
        IList<TodoItem> GetAllTodoItems();
        TodoItem GetTodoItemById(int id);
        long AddTodoItem(TodoItem todoItem);
        void DeleteTodoItem(int id);
        void MarkTodoItemAsDone(int id);

    }
}


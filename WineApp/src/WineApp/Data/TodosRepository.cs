using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WineApp.Models;

namespace WineApp.Data
{
    public class TodosRepository : ITodosRepository
    {
        private List<TodoItem> todoItems = new List<TodoItem>()
        {
            new TodoItem
            {   Id = 1,
                Name = "Learn Aurelia",
                CreatedAt =  new DateTime(2015, 12, 04),
                DueDate = new DateTime(2016, 12, 01),
            },
            new TodoItem
            {   Id = 2,
                Name = "Write Aurelia App",
                CreatedAt =  new DateTime(2015, 12, 04),
                DueDate = new DateTime(2016, 08, 31),
            }
            ,
            new TodoItem
            {   Id = 3,
                Name = "Sell Aurelia App",
                CreatedAt =  new DateTime(2015, 12, 04),
                DueDate = new DateTime(2016,10, 15),
            }
        };

        public IList<TodoItem> GetAllTodoItems()
        {
            return todoItems;
        }

        public TodoItem GetTodoItemById(int id)
        {
            return todoItems.Find(todoItem => todoItem.Id == id);
        }

        public long AddTodoItem(TodoItem todoItem)
        {
            var newId = todoItems.Max(x => x.Id) + 1;
            todoItem.Id = newId;
            todoItems.Add(todoItem);
            return newId;
        }

        public void DeleteTodoItem(int id)
        {
            todoItems.Remove(todoItems.Single(x => x.Id == id));
        }

        public void MarkTodoItemAsDone(int id)
        {
            todoItems.Single(x => x.Id == id).IsCompleted = true;
        }
    }

}



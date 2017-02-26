using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WineApp.Models
{
   
        public class TodoItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsCompleted { get; set; }
            public DateTime? CreatedAt { get; set; }
            public DateTime? DueDate { get; set; }
        }
}

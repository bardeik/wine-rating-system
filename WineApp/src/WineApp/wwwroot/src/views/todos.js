System.register(["aurelia-framework", "aurelia-fetch-client"], function(exports_1, context_1) {
    "use strict";
    var __moduleName = context_1 && context_1.id;
    var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
        var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
        if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
        else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
        return c > 3 && r && Object.defineProperty(target, key, r), r;
    };
    var aurelia_framework_1, aurelia_fetch_client_1;
    var Todos;
    return {
        setters:[
            function (aurelia_framework_1_1) {
                aurelia_framework_1 = aurelia_framework_1_1;
            },
            function (aurelia_fetch_client_1_1) {
                aurelia_fetch_client_1 = aurelia_fetch_client_1_1;
            }],
        execute: function() {
            let Todos = class Todos {
                constructor(http) {
                    this.http = http;
                }
                activate() {
                    this.fetchAllTodoItems();
                }
                addNewTodoItem() {
                    const newTodoItem = {
                        DueDate: this.dueDateTodoItem,
                        Name: this.nameTodoItem
                    };
                    this.http.fetch("http://localhost:50468/api/todos/", {
                        method: "post",
                        body: aurelia_fetch_client_1.json(newTodoItem)
                    }).then(response => {
                        this.fetchAllTodoItems();
                        console.log("todo item added: ", response);
                    });
                }
                fetchAllTodoItems() {
                    return this.http.fetch("http://localhost:50468/api/todos").
                        then(response => response.json()).then(data => {
                        this.todoItems = data;
                    });
                }
                deleteTodoItem(todoItemId) {
                    this.http.fetch(`http://localhost:50468/api/todos/${todoItemId}`, { method: "delete" }).then(() => { this.fetchAllTodoItems(); });
                }
                markTodoItemAsDone(todoItem) {
                    if (todoItem.isCompleted)
                        return;
                    this.http.fetch(`http://localhost:50468/api/todos/${todoItem.id}`, { method: "put" }).then(() => { this.fetchAllTodoItems(); });
                }
            };
            Todos = __decorate([
                aurelia_framework_1.inject(aurelia_fetch_client_1.HttpClient, aurelia_fetch_client_1.json)
            ], Todos);
            exports_1("Todos", Todos);
        }
    }
});
//# sourceMappingURL=todos.js.map
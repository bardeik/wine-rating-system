import {inject} from "aurelia-framework";
import {HttpClient, json} from "aurelia-fetch-client";

@inject(HttpClient, json)
export class Todos {
    todoItems: Array<ITodoItem>;
    dueDateTodoItem: Date;
    nameTodoItem: string;

    constructor(private http: HttpClient) { }

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
            body: json(newTodoItem)

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
        this.http.fetch(`http://localhost:50468/api/todos/${todoItemId}`,
            { method: "delete" }).then(() => { this.fetchAllTodoItems(); });
    }

    markTodoItemAsDone(todoItem: ITodoItem) {
        if (todoItem.isCompleted) return;
        this.http.fetch(`http://localhost:50468/api/todos/${todoItem.id}`,
            { method: "put" }).then(() => { this.fetchAllTodoItems(); });
    }

}

export interface ITodoItem {
    id: number;
    name: string;
    isCompleted: boolean;
    createdAt: Date;
    dueDate: Date;
}
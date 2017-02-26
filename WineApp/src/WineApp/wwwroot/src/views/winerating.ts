import {inject} from "aurelia-framework";
import {HttpClient, json} from "aurelia-fetch-client";

@inject(HttpClient, json)
export class WineRating {
    todoItems: Array<IWineRating>;


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

}

export interface IWineRating {
    wineRatingId: number;
    visuality: number;
    nose: number;
    taste: number;
    judgeId: string;
    wineId: number
}


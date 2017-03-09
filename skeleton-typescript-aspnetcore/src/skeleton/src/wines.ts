import {inject} from "aurelia-framework";
import {HttpClient, json} from "aurelia-fetch-client";

@inject(HttpClient, json)
export class Wines {
    wines: Array<IWine>;
    wineId: number;
    ratingName: string;
    name: string;
    group: WineGroup;
    class: WineClass;
    category: WineCategory;
    wineProducerId: number;

    constructor(private http: HttpClient) { }

    activate() {
        this.fetchAllWines();
    }

    addNewWine() {
        const newWine = {
            RatingName: this.ratingName,
            Name: this.name,
            Group:  this.group,
            Class: this.class,
            Category: this.category,
            WineProducerId: this.wineProducerId
        };
        this.http.fetch("http://localhost:49862/api/wines/", {
            method: "post",
            body: json(newWine)
        }).then(response => {
            this.fetchAllWines();
            console.log("Wine Rating added: ", response);
        });
    }

    fetchAllWines() {
        return this.http.fetch("http://localhost:49862/api/wines").
            then(response => response.json()).then(data => {
				this.wines = JSON.parse(data['_body']);
            });
    }

    deleteWine(wineId) {
        this.http.fetch(`http://localhost:49862/api/wines/${wineId}`,
            { method: "delete" }).then(() => { this.fetchAllWines(); });
    }
}

export interface IWine {
    wineId: number;
    ratingName: string;
    name: string;
    group: WineGroup;
    class: WineClass;
    category: WineCategory;
    wineProducerId: number;
}

export enum WineGroup {
    A, B, C, D
}

export enum WineClass {
    Unge, Eldre
}

export enum WineCategory {
    Hvitvin, Rosevin, Dessertvin, Rodvin, Mousserendevin, Hetvin
}

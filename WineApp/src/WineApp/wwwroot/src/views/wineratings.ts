import {inject} from "aurelia-framework";
import {HttpClient, json} from "aurelia-fetch-client";

@inject(HttpClient, json)
export class WineRatings {
    wineRatings: Array<IWineRating>;
    visuality: number;
    nose: number;
    taste: number;
    judgeId: string;
    wineId: number

    constructor(private http: HttpClient) { }

    activate() {
        this.fetchAllWineRatings();
    }

    addNewWineRating() {
        const newWineRating = {
            Visuality: this.visuality,
            Nose: this.nose,
            Taste: this.taste,
            JudgeId: this.judgeId,
            WineId: this.wineId
        };
        this.http.fetch("http://localhost:50468/api/wineratings/", {
            method: "post",
            body: json(newWineRating)
        }).then(response => {
            this.fetchAllWineRatings();
            console.log("Wine Rating added: ", response);
        });
    }

    fetchAllWineRatings() {
        return this.http.fetch("http://localhost:50468/api/wineratings").
            then(response => response.json()).then(data => {
                this.wineRatings = data;
            });
    }

    deleteWineRating(wineRatingId) {
        this.http.fetch(`http://localhost:50468/api/wineratings/${wineRatingId}`,
            { method: "delete" }).then(() => { this.fetchAllWineRatings(); });
    }
}

export interface IWineRating {
    wineRatingId: number;
    visuality: number;
    nose: number;
    taste: number;
    judgeId: string;
    wineId: number;
}


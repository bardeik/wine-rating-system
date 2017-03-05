import {inject} from "aurelia-framework";
import {HttpClient, json} from "aurelia-fetch-client";

@inject(HttpClient, json)
export class WineRatings {
    wineRatings: Array<IWineRating>;
    wineRatingScores: Array<IWineRatingScore>;
    visuality: number;
    nose: number;
    taste: number;
    judgeId: string;
    wineId: number

    constructor(private http: HttpClient) { }

    activate() {
        this.fetchAllWineRatings();
        this.fetchAllWineRatingScores();
    }

    addNewWineRating() {
        const newWineRating = {
            Visuality: this.visuality,
            Nose: this.nose,
            Taste: this.taste,
            JudgeId: this.judgeId,
            WineId: this.wineId
        };
        this.http.fetch("http://localhost:49862/api/wineratings/", {
            method: "post",
            body: json(newWineRating)
        }).then(response => {
            this.fetchAllWineRatings();
            this.fetchAllWineRatingScores();
            console.log("Wine Rating added: ", response);
        });
    }

    fetchAllWineRatingScores() {
        return this.http.fetch("http://localhost:49862/api/wineratings/GetScoreForWines").
            then(response => response.json()).then(data => {
                this.wineRatingScores = data;
            });
    }

    fetchAllWineRatings() {
        return this.http.fetch("http://localhost:49862/api/wineratings").
            then(response => response.json()).then(data => {
                this.wineRatings = data;
            });
    }


    deleteWineRating(wineRatingId) {
        this.http.fetch(`http://localhost:49862/api/wineratings/${wineRatingId}`,
            { method: "delete" }).then(() => { this.fetchAllWineRatings(); this.fetchAllWineRatingScores(); });
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

export interface IWineRatingScore {
        wineId: number;
        visuality: number;
        nose: number;
        taste: number;
        numberOfRatings: number;
        wineLevel: number;
        overallScore: number;
}

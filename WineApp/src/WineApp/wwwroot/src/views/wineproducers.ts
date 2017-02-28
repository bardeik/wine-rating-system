import {inject} from "aurelia-framework";
import {HttpClient, json} from "aurelia-fetch-client";

@inject(HttpClient, json)
export class WineProducers {
    wineProducers: Array<IWineProducer>;
    wineyardName: string;
    organisationNumber: string;
    responsibleProducerName: string;
    address: string;
    city: string;
    country: string;
    zip: string;
    email: string;

    constructor(private http: HttpClient) { }

    activate() {
        this.fetchAllWineProducers();
    }

    addNewWineProducer() {
        const newWineProducer = {
            WineyardName: this.wineyardName,
            OrganisationNumber: this.organisationNumber,
            ResponsibleProducerName: this.responsibleProducerName,
            Address: this.address,
            City: this.city,
            Country: this.country,
            Zip: this.zip,
            Email: this.email
        };
        this.http.fetch("http://localhost:50468/api/wineproducerss/", {
            method: "post",
            body: json(newWineProducer)
        }).then(response => {
            this.fetchAllWineProducers();
            console.log("Wine Rating added: ", response);
        });
    }

    fetchAllWineProducers() {
        return this.http.fetch("http://localhost:50468/api/wineproducers").
            then(response => response.json()).then(data => {
                this.wineProducers = data;
            });
    }

    deleteWineProducer(wineProducerId) {
        this.http.fetch(`http://localhost:50468/api/wineproducers/${wineProducerId}`,
            { method: "delete" }).then(() => { this.fetchAllWineProducers(); });
    }
}

export interface IWineProducer {
     wineProducerId: number;
     wineyardName: string;
     organisationNumber: string;
     responsibleProducerName: string;
     address: string;
     city: string;
     country: string;
     zip: string;
     email: string;
}


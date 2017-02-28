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
    var WineProducers;
    return {
        setters:[
            function (aurelia_framework_1_1) {
                aurelia_framework_1 = aurelia_framework_1_1;
            },
            function (aurelia_fetch_client_1_1) {
                aurelia_fetch_client_1 = aurelia_fetch_client_1_1;
            }],
        execute: function() {
            let WineProducers = class WineProducers {
                constructor(http) {
                    this.http = http;
                }
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
                        body: aurelia_fetch_client_1.json(newWineProducer)
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
                    this.http.fetch(`http://localhost:50468/api/wineproducers/${wineProducerId}`, { method: "delete" }).then(() => { this.fetchAllWineProducers(); });
                }
            };
            WineProducers = __decorate([
                aurelia_framework_1.inject(aurelia_fetch_client_1.HttpClient, aurelia_fetch_client_1.json)
            ], WineProducers);
            exports_1("WineProducers", WineProducers);
        }
    }
});
//# sourceMappingURL=wineproducers.js.map
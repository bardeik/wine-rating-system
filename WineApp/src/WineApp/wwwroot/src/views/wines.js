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
    var Wines, WineGroup, WineClass, WineCategory;
    return {
        setters:[
            function (aurelia_framework_1_1) {
                aurelia_framework_1 = aurelia_framework_1_1;
            },
            function (aurelia_fetch_client_1_1) {
                aurelia_fetch_client_1 = aurelia_fetch_client_1_1;
            }],
        execute: function() {
            let Wines = class Wines {
                constructor(http) {
                    this.http = http;
                }
                activate() {
                    this.fetchAllWines();
                }
                addNewWine() {
                    const newWine = {
                        RatingName: this.ratingName,
                        Name: this.name,
                        Group: this.group,
                        Class: this.class,
                        Category: this.category,
                        WineProducerId: this.wineProducerId
                    };
                    this.http.fetch("http://localhost:50468/api/wines/", {
                        method: "post",
                        body: aurelia_fetch_client_1.json(newWine)
                    }).then(response => {
                        this.fetchAllWines();
                        console.log("Wine Rating added: ", response);
                    });
                }
                fetchAllWines() {
                    return this.http.fetch("http://localhost:50468/api/wines").
                        then(response => response.json()).then(data => {
                        this.wines = data;
                    });
                }
                deleteWine(wineId) {
                    this.http.fetch(`http://localhost:50468/api/wines/${wineId}`, { method: "delete" }).then(() => { this.fetchAllWines(); });
                }
            };
            Wines = __decorate([
                aurelia_framework_1.inject(aurelia_fetch_client_1.HttpClient, aurelia_fetch_client_1.json)
            ], Wines);
            exports_1("Wines", Wines);
            (function (WineGroup) {
                WineGroup[WineGroup["A"] = 0] = "A";
                WineGroup[WineGroup["B"] = 1] = "B";
                WineGroup[WineGroup["C"] = 2] = "C";
                WineGroup[WineGroup["D"] = 3] = "D";
            })(WineGroup || (WineGroup = {}));
            exports_1("WineGroup", WineGroup);
            (function (WineClass) {
                WineClass[WineClass["Unge"] = 0] = "Unge";
                WineClass[WineClass["Eldre"] = 1] = "Eldre";
            })(WineClass || (WineClass = {}));
            exports_1("WineClass", WineClass);
            (function (WineCategory) {
                WineCategory[WineCategory["Hvitvin"] = 0] = "Hvitvin";
                WineCategory[WineCategory["Rosevin"] = 1] = "Rosevin";
                WineCategory[WineCategory["Dessertvin"] = 2] = "Dessertvin";
                WineCategory[WineCategory["Rodvin"] = 3] = "Rodvin";
                WineCategory[WineCategory["Mousserendevin"] = 4] = "Mousserendevin";
                WineCategory[WineCategory["Hetvin"] = 5] = "Hetvin";
            })(WineCategory || (WineCategory = {}));
            exports_1("WineCategory", WineCategory);
        }
    }
});
//# sourceMappingURL=wines.js.map
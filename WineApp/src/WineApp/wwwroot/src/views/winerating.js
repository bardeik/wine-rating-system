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
    var WineRating;
    return {
        setters:[
            function (aurelia_framework_1_1) {
                aurelia_framework_1 = aurelia_framework_1_1;
            },
            function (aurelia_fetch_client_1_1) {
                aurelia_fetch_client_1 = aurelia_fetch_client_1_1;
            }],
        execute: function() {
            let WineRating = class WineRating {
                constructor(http) {
                    this.http = http;
                }
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
                        body: aurelia_fetch_client_1.json(newWineRating)
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
                    this.http.fetch(`http://localhost:50468/api/wineratings/${wineRatingId}`, { method: "delete" }).then(() => { this.fetchAllWineRatings(); });
                }
            };
            WineRating = __decorate([
                aurelia_framework_1.inject(aurelia_fetch_client_1.HttpClient, aurelia_fetch_client_1.json)
            ], WineRating);
            exports_1("WineRating", WineRating);
        }
    }
});
//# sourceMappingURL=winerating.js.map
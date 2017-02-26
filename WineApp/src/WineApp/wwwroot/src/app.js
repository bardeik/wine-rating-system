System.register(["aurelia-framework", "aurelia-router"], function(exports_1, context_1) {
    "use strict";
    var __moduleName = context_1 && context_1.id;
    var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
        var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
        if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
        else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
        return c > 3 && r && Object.defineProperty(target, key, r), r;
    };
    var aurelia_framework_1, aurelia_router_1;
    var App;
    return {
        setters:[
            function (aurelia_framework_1_1) {
                aurelia_framework_1 = aurelia_framework_1_1;
            },
            function (aurelia_router_1_1) {
                aurelia_router_1 = aurelia_router_1_1;
            }],
        execute: function() {
            let App = class App {
                constructor() {
                }
                configureRouter(config, router) {
                    this.router = router;
                    config.title = "Todo App";
                    config.map([
                        { route: ["", "todos"], moduleId: "./views/todos", nav: true, title: "Todo List" },
                    ]);
                }
            };
            App = __decorate([
                aurelia_framework_1.inject(aurelia_router_1.Router)
            ], App);
            exports_1("App", App);
        }
    }
});
//# sourceMappingURL=app.js.map
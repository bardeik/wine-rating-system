import {inject} from "aurelia-framework";
import {Router} from "aurelia-router";

@inject(Router)
export class App {
    router: Router;

    constructor() { }

    configureRouter(config, router: Router) {
        this.router = router;

        config.title = "Todo App";
        config.map([
            { route: ["", "wines"], moduleId: "./views/wines", nav: true, title: "Wines" },
            { route: ["wineproducers"], moduleId: "./views/wineproducers", nav: true, title: "Wine Producers" },
            { route: ["wineratings"], moduleId: "./views/wineratings", nav: true, title: "Wine Ratings" },
            { route: ["todos"], moduleId: "./views/todos", nav: true, title: "Todo List" },
        ]);
    }
}

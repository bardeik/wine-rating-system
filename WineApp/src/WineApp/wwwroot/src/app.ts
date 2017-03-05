import {inject} from "aurelia-framework";
import {Router} from "aurelia-router";

@inject(Router)
export class App {
    router: Router;

    constructor() { }

    configureRouter(config, router: Router) {
		config.title = 'Wine App'
        config.map([
            { route: ['', 'wines'], name: 'wines', moduleId: "./views/wines", nav: true, title: "Wines" },
            { route: 'wineproducers', name: 'wineproducers', moduleId: "./views/wineproducers", nav: true, title: "Wine Producers" },
            { route: 'wineratings', name: 'wineratings', moduleId: "./views/wineratings", nav: true, title: "Wine Ratings" }
//            { route: 'todos', name: 'todos', moduleId: "./views/todos", nav: true, title: "Todo List" }
        ]);

        this.router = router;

    }
}

import {Router, RouterConfiguration} from 'aurelia-router';

export class App {
  public router: Router;

  public configureRouter(config: RouterConfiguration, router: Router) {
    config.title = 'Wine App';
    config.map([
//      { route: ['', 'welcome'], name: 'welcome',      moduleId: 'welcome',      nav: true, title: 'Welcome' },

      { route: ['', 'wines'], name: 'wines', moduleId: "wines", nav: true, title: "Wines" },
      { route: 'wineproducers', name: 'wineproducers', moduleId: "wineproducers", nav: true, title: "Wine Producers" },
      { route: 'wineratings', name: 'wineratings', moduleId: "wineratings", nav: true, title: "Wine Ratings" }
    ]);

    this.router = router;
  }
}

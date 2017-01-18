import {inject, bindable} from 'aurelia-framework';
import {Redirect} from 'aurelia-router';
import {StencilSDK} from 'stencil-sdk';

@inject(StencilSDK, Element)
export class App {
  constructor(stencilSDK, element){
    this.sdk = stencilSDK;
    this.element = element;
  }
  configureRouter(config, router) {
    config.title = 'Aurelia';
    config.map([
      { route: '', name: 'login',      moduleId: 'login',      nav: true, title: 'Login' },
      { route: 'welcome', name: 'welcome',      moduleId: 'welcome',      nav: true, title: 'Welcome' },
      { route: 'users',         name: 'users',        moduleId: 'users',        nav: true, title: 'Github Users' },
      { route: 'child-router',  name: 'child-router', moduleId: 'child-router', nav: true, title: 'Child Router' }
    ]);

    this.router = router;
  }
}

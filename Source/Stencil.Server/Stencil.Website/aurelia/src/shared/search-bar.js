import {inject, customElement, bindable} from 'aurelia-framework';

@inject(Element)
@customElement('search-bar')
export class SearchBar {
    
    @bindable term;
    @bindable app;
    @bindable service;
    @bindable entity;
    
    constructor(element) {
      this.element = element;
      this.entity = "item";
   }
    
    attached(){
    }
    
    termChanged(){
        this.service.dataApplySearch(this.term);
    }
    
}

  
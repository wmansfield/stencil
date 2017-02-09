import { inject, customElement, bindable } from 'aurelia-framework';

@inject(Element)
@customElement('pager')
export class Pager {

    @bindable data;
    @bindable step;
    @bindable app;
    @bindable service;

    constructor(element) {
        this.element = element;
    }

    attached() {
    }

    loadPrev() {
        var skip = this.data.stepping.current - this.step; // stepping.step is the NEXT, so go back twice
        this.service.dataStepTo(skip);
    }
    loadNext() {
        var skip = this.data.stepping.skip; // stepping.step is the NEXT
        this.service.dataStepTo(skip);
    }
}


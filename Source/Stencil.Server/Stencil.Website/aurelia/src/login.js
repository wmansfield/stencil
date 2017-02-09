import { computedFrom, inject, bindable } from 'aurelia-framework';
import { App } from 'app';
import $ from 'jquery';

@inject(App)
export class Login {

    @bindable userName;
    @bindable password;
    @bindable persist;
    @bindable processing;

    constructor(app) {
        this.app = app;
    }

    activate() {
        this.persist = false;
    }
    attached() {
        var self = this;
        this.$loginForm = function () { return $('#loginForm'); }
    }

    showError(data) {
        let message = '';
        if (data.Message) {
            message = data.Message
        } else if (data.responseJSON && data.responseJSON.Message) {
            message = data.responseJSON.Message;
        } else if (data.responseText) {
            message = data.responseText;
        } else if (data.statusText) {
            message = "Server returned: " + data.statusText;
        } else {
            message = data;
        }
        if (!message) {
            message = 'Unknown error occurred.'
        }
        alert(message);
    }
    showNotice(message) {
        alert(message);
    }

    submit() {
        var self = this;
            var sdk = this.app.sdk.configure();
            var payload = { user: self.userName, password: self.password, persist: self.persist };

            this.processing = true;
            
            sdk.Auth.LoginAsync(payload)
                .done(function (data) {
                    self.processing = false;
                    self.app.updateUser(data.item);
                    self.showNotice(`Hello ${data.item.first_name}`);
                })
                .error(function (data) {
                    self.processing = false;
                    self.showError(data);
                });
        return false;

    }
}

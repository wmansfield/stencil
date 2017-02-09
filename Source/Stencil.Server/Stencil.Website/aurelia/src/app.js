import { inject, bindable } from 'aurelia-framework';
import { Redirect } from 'aurelia-router';
import { StencilSDK } from 'stencil-sdk';

@inject(StencilSDK, Element)
export class App {

  @bindable notice;
  @bindable error;
  @bindable user;

  constructor(stencilSDK, element) {
    this.sdk = stencilSDK;
    this.element = element;
    this.notice = { button_text: "Okay" };
    this.error = { button_text: "Okay", title: "Error!" };
    this.confirmnotice = { positive_text: "Okay", negative_text: "Cancel", title: "Confirm" };
  }

  configureRouter(config, router) {
    config.title = 'Aurelia';

    config.addPipelineStep('authorize', AuthorizeStep);

    config.map([
      { route: '', name: 'login', moduleId: 'login', nav: true, title: 'Login' },
      { route: 'welcome', name: 'welcome', moduleId: 'welcome', nav: true, title: 'Welcome' },
      { route: 'posts', name: 'posts', moduleId: 'posts', nav: true, title: 'Posts' },
      { route: 'users', name: 'users', moduleId: 'users', nav: true, title: 'Github Users' },
      { route: 'child-router', name: 'child-router', moduleId: 'child-router', nav: true, title: 'Child Router' }
    ]);

    this.router = router;
  }

  navigationFailed(event) {
    console.log(event);
  }
  showError(data, callback) {
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
    this.error.message = message;
    this.error.callback = callback;
    $("#appErrorModal").modal('show');
  }
  showNotice(title, message, callback) {
    this.notice.title = title;
    this.notice.message = message;
    this.notice.callback = callback;
    $("#appNoticeModal").modal('show');
  }
  showConfirm(title, message, callback, positiveText = "Okay", negativeText = "Cancel") {
    this.confirmnotice.title = title;
    this.confirmnotice.message = message;
    this.confirmnotice.callback = callback;
    this.confirmnotice.positive_text = positiveText;
    this.confirmnotice.negative_text = negativeText;
    $("#appConfirmModal").modal('show');
  }
  signOut() {
    sessionStorage.removeItem("account");

    var self = this;
    var sdk = self.sdk.configure("");

    sdk.Auth.LogoutAsync()
      .done(function (data) {
        self.user = null;
        self.router.navigate("");
      })
      .error(function (data) {
        self.user = null;
        self.router.navigate("");
      });

    return false;
  }

  activate() {
    var self = this;
    var sdk = self.sdk.configure("");

    sdk.Accounts.GetSelfAsync()
      .done(function (data) {
        self.updateUser(data.item);
      })
      .error(function (data) {
        if (data.status == 401) {
          if (sessionStorage.getItem("account")) {
            sessionStorage.removeItem("account");
            self.router.navigate("");
          }
        }
      });


  }

  updateUser(user, persist = true) {
    this.user = user;
    if (persist) {
      sessionStorage.setItem("account", JSON.stringify(this.user));
    }
  }
  onConfirmButtonClicked(button) {
    if (this.confirmnotice.callback) {
      this.confirmnotice.callback(button);
    }
  }
  onNoticeButtonClicked(button) {
    if (this.notice.callback) {
      this.notice.callback(button);
    }
  }
  onErrorButtonClicked(button) {
    if (this.error.callback) {
      this.error.callback(button);
    }
  }
}


@inject(App, StencilSDK)
class AuthorizeStep {
  constructor(appInstance, stencilSDK) {
    this.sdk = stencilSDK;
    this.app = appInstance;
  }

  run(navigationInstruction, next) {
    if (navigationInstruction.config.route == '' || navigationInstruction.config.route == 'login' || navigationInstruction.config.anonymous) {
      return next();
    } else {
      var self = this;
      if (this.isLoggedIn()) {
        return next();
      } else {
        return new Promise(function (resolve, reject) {
          var sdk = self.sdk.configure("");

          sdk.Accounts.GetSelfAsync()
            .done(function (data) {
              self.app.updateUser(data.item, false);
              resolve();
              next();
            })
            .error(function (data) {
              reject();
              self.app.router.navigate("");
            });
        });
      }
    }
  }
  isLoggedIn() {
    if (this.app.user) {
      return true;
    } else {
      var account = sessionStorage.getItem("account");
      if (account) {
        this.app.updateUser(JSON.parse(account), false);
        return true;
      }
      return false;
    }

  }
}


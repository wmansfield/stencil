define(['exports'], function (exports) {
  'use strict';
  var StencilSDK = (function () {
      function StencilSDK() {
      this.private = "parts";
    }

    StencilSDK.prototype.configure = function configure(baseUrl) {
      baseUrl = (baseUrl == null || baseUrl == "") ? "/api" : baseUrl;
      return Stencil.Client.Create(baseUrl);
    };
    
    return StencilSDK;
  })();
  
  exports.StencilSDK = StencilSDK;
  return exports;
});


StencilSDKBridge = {};

// executeAsync: Required
StencilSDKBridge.executeAsync = function (baseUrl, resourceUrl, method, body, parameters, headers, contentType) {

    var data = null;
    if (body) {
        data = JSON.stringify(body)
    } else {
        data = StencilSDKBridge.mapKeyValue(parameters);
    }

    contentType = contentType || "application/json";

    var $defer = $.ajax({
        type: method,
        url: baseUrl + "/" + resourceUrl,
        processData: (body == null),
        contentType: contentType,
        data: data,
        headers: StencilSDKBridge.mapKeyValue(headers)
    });

    return $defer;
    
};

// generateMD5Hash: Required
StencilSDKBridge.generateMD5Hash = function (content) {
    return md5(content);
};
// getTimeStamp: Required
StencilSDKBridge.getTimeStamp = function () {
    return Math.floor(new Date().getTime() / 1000);
};

//mapKeyValue: Internal
StencilSDKBridge.mapKeyValue = function (keyValuePairs) {
    var result = {};
    if (keyValuePairs) {
        for (var i = 0; i < keyValuePairs.storage.length; i++) {
            result[keyValuePairs.storage[i].key] = keyValuePairs.storage[i].value;
        }
    }
    return result;
};

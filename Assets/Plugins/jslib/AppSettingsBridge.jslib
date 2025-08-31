mergeInto(LibraryManager.library, {
    // 本物のデータ設定
    SetAppCode: function(appCode) {
        var appCodeStr = UTF8ToString(appCode).trim();
        window.unityAppCode = appCodeStr;
        console.log("AppCode set from Unity:", JSON.stringify(appCodeStr));
    },
    
    SetServiceToken: function(serviceToken) {
        var serviceTokenStr = UTF8ToString(serviceToken).trim();
        window.unityServiceToken = serviceTokenStr;
        console.log("ServiceToken set from Unity:", JSON.stringify(serviceTokenStr));
    },
    
    SetProjectCode: function(projectCode) {
        var projectCodeStr = UTF8ToString(projectCode);
        window.unityProjectCode = projectCodeStr;
        console.log("ProjectCode set from Unity");
    },
    
    SetAuthDomain: function(authDomain) {
        var authDomainStr = UTF8ToString(authDomain);
        window.unityAuthDomain = authDomainStr;
        console.log("AuthDomain set from Unity");
    },
    
    SetStorageBucket: function(storageBucket) {
        var storageBucketStr = UTF8ToString(storageBucket);
        window.unityStorageBucket = storageBucketStr;
        console.log("StorageBucket set from Unity");
    },
    
    SetMessagingSenderCode: function(messagingSenderCode) {
        var messagingSenderCodeStr = UTF8ToString(messagingSenderCode);
        window.unityMessagingSenderCode = messagingSenderCodeStr;
        console.log("MessagingSenderCode set from Unity");
    },
    
    SetDatabaseURL: function(databaseURL) {
        var databaseURLStr = UTF8ToString(databaseURL);
        window.unityDatabaseURL = databaseURLStr;
        console.log("DatabaseURL set from Unity");
    },
    
    SetProductionMode: function(isProduction) {
        window.unityIsProduction = isProduction;
        console.log("ProductionMode set from Unity:", isProduction);
    }
});

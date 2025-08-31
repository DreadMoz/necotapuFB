mergeInto(LibraryManager.library, {
    // Firebase関連の関数
    setFirebaseConfig: function(configPointer) {
        var config = UTF8ToString(configPointer);
        setFirebaseConfig(config);
    },
    
    savePlayerDataJS: function(playerIdPointer, jsonDataPointer) {
        var playerId = UTF8ToString(playerIdPointer);
        var jsonData = UTF8ToString(jsonDataPointer);
        savePlayerDataJS(playerId, jsonData);
    },
    
    loadPlayerDataJS: function(playerIdPointer) {
        var playerId = UTF8ToString(playerIdPointer);
        loadPlayerDataJS(playerId);
    },
    
    checkAuthStateJS: function() {
        checkAuthStateJS();
    },
    
    signInAnonymouslyJS: function() {
        signInAnonymouslyJS();
    },
    
    signInAsGuestJS: function() {
        signInAsGuestJS();
    },
    
    signOutJS: function() {
        signOutJS();
    },
    
    signInWithGoogleJS: function() {
        signInWithGoogleJS();
    },
    
    signInWithMicrosoftJS: function() {
        signInWithMicrosoftJS();
    },
    
    signInWithEmailJS: function(emailPointer, passwordPointer) {
        var email = UTF8ToString(emailPointer);
        var password = UTF8ToString(passwordPointer);
        signInWithEmailJS(email, password);
    },
    
    createUserWithEmailJS: function(emailPointer, passwordPointer) {
        var email = UTF8ToString(emailPointer);
        var password = UTF8ToString(passwordPointer);
        createUserWithEmailJS(email, password);
    },
    
    // 既存の関数
    GetOAuth: function() {
        getOAuth();
    },

    OAuthLogout: function() {
        oAuthLogout();
    },
    LoadDataFromLocal: function() {
        try {
            // LocalStorageからステータスデータを取得
            var storedStatusData = localStorage.getItem('statusData');
            if (!storedStatusData) {
                console.log("LocalStorageにステータスデータが見つかりません。");
                SendMessage('TitleScene', 'handleInitialData');
            }
            else{
                var statusData = JSON.parse(storedStatusData);
                console.log("LocalStorageからステータスデータを読み込みました:", statusData);
                SendMessage('TitleScene', 'finishDataLoadExtStatus', JSON.stringify(statusData));
            }
            getNecoRank();
            
            // LocalStorageからランキングデータを取得
            var storedRankingData = localStorage.getItem('rankingData'); // ランキングデータも取得
            if (!storedRankingData) {
                console.log("LocalStorageにランキングデータが見つかりません。");
            }
            else
            {
                var rankingData = JSON.parse(storedRankingData); // ランキングデータを解析
                console.log("LocalStorageからランキングデータを読み込みました:", rankingData);
                SendMessage('GameManager', 'finishDataLoadExtRanking', JSON.stringify(rankingData));
            }
        } catch (e) {
            console.error("データの解析中にエラーが発生しました:", e);
            SendMessage('TitleScene', 'handleDataError');
        }
    },
    SaveStatusToLocal: function(dataPointer) {
        // ポインタから実際の文字列を取得
        var data = UTF8ToString(dataPointer);

        // LocalStorageにデータを保存
        localStorage.setItem('statusData', data);
        console.log("データをLocalStorageに保存しました:", data);
        // タイムアウト監視などの追加のエラーハンドリングもここに実装可能
    },
    GetNecoRank: function() {
        getNecoRank();
    },
    LoadFromGss: function() {
        loadFromGss();
    },
    SaveToGss: function(dataPointer) {
        console.log("Received pointer:", dataPointer); // ポインタ受け取り時のデバッグ
        var data = UTF8ToString(dataPointer);
        console.log("Converted data:", data); // 文字列変換後のデバッグ
        saveToGss(data);
    },
    ThroughGemini: function(dataPointer) {
        console.log("Received pointer:", dataPointer); // ポインタ受け取り時のデバッグ
        var data = UTF8ToString(dataPointer);
        console.log("Converted data:", data); // 文字列変換後のデバッグ
        throughGemini(data);
    }
});

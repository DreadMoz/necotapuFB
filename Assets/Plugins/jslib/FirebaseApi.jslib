// ===== FirebaseApi.jslib - 統合完了のためコメントアウト =====
// 全ての機能は FirebaseBridge.jslib に統合されました
/*
mergeInto(LibraryManager.library, {
    // Firebase認証関連の関数
    signInWithGoogleJslib: function() {
        startGoogleAuthFromUnity();
    },
    
    signInWithMicrosoftJS: function() {
        startMicrosoftAuthFromUnity();
    },
    
    signInWithEmailJS: function(emailPointer, passwordPointer) {
        var email = UTF8ToString(emailPointer);
        var password = UTF8ToString(passwordPointer);
        // TODO: メール認証の実装
        console.log("メール認証:", email);
    },
    
    signInAsGuestJS: function() {
        startGuestAuthFromUnity();
    },
    
    signOutJS: function() {
        signOutJS();
    },
    
    // Firestore関連の関数
    LoadFromFirestoreJS: function() {
        loadFromFirestore();
    },
    
    SaveToFirestoreJS: function(dataPointer) {
        console.log("Received pointer:", dataPointer);
        var data = UTF8ToString(dataPointer);
        console.log("Converted data:", data);
        saveToFirestore(data);
    },
    
    // 認証関連の関数
    GetLastAuthMethod: function() {
        return localStorage.getItem('lastAuthMethod') || '';
    },
    
    IsChromebook: function() {
        return navigator.userAgent.indexOf('Windows') !== -1;
    },
    
    IsMicrosoftEnvironment: function() {
        return navigator.userAgent.indexOf('Windows') !== -1;
    },
    
    // Firebase設定
    setFirebaseConfig: function(configPointer) {
        var config = UTF8ToString(configPointer);
        setFirebaseConfig(config);
    }
});
*/ 
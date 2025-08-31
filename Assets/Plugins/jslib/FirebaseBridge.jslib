


mergeInto(LibraryManager.library, {
    // ===== 制限時間設定 =====
    firebaseAccessLimitHours: 23, // デフォルト値
    
    // Unity側から制限時間を設定
    SetFirebaseAccessLimitHoursJslib: function(hours) {
        this.firebaseAccessLimitHours = hours;
        console.log(`制限時間を${hours}時間に設定しました`);
    },
    
    // Firebase設定値の受け渡し
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
        var projectCodeStr = UTF8ToString(projectCode).trim();
        window.unityProjectCode = projectCodeStr;
        console.log("ProjectCode set from Unity:", JSON.stringify(projectCodeStr));
    },
    
    SetAuthDomain: function(authDomain) {
        var authDomainStr = UTF8ToString(authDomain).trim();
        window.unityAuthDomain = authDomainStr;
        console.log("AuthDomain set from Unity:", JSON.stringify(authDomainStr));
    },
    
    SetStorageBucket: function(storageBucket) {
        var storageBucketStr = UTF8ToString(storageBucket).trim();
        window.unityStorageBucket = storageBucketStr;
        console.log("StorageBucket set from Unity:", JSON.stringify(storageBucketStr));
    },
    
    SetMessagingSenderCode: function(messagingSenderCode) {
        var messagingSenderCodeStr = UTF8ToString(messagingSenderCode).trim();
        window.unityMessagingSenderCode = messagingSenderCodeStr;
        console.log("MessagingSenderCode set from Unity:", JSON.stringify(messagingSenderCodeStr));
    },
    
    SetDatabaseURL: function(databaseURL) {
        var databaseURLStr = UTF8ToString(databaseURL).trim();
        window.unityDatabaseURL = databaseURLStr;
        console.log("DatabaseURL set from Unity:", JSON.stringify(databaseURLStr));
    },
    
    SetProductionMode: function(isProduction) {
        var isProductionStr = UTF8ToString(isProduction).trim();
        window.unityIsProduction = isProductionStr;
        console.log("ProductionMode set from Unity:", JSON.stringify(isProductionStr));
    },
    
    // ===== FirebaseApi.jslib から統合 =====
    // Firebase認証関連の関数
    signInWithGoogleJslib: function() {
        startGoogleAuthFromUnity();
    },
    
    signInWithMicrosoftJslib: function() {
        startMicrosoftAuthFromUnity();
    },
    
    signInWithEmailJslib: function(emailPointer, passwordPointer) {
        var email = UTF8ToString(emailPointer);
        var password = UTF8ToString(passwordPointer);
        // TODO: メール認証の実装
        console.log("メール認証:", email);
    },
    
    signInAsGuestJslib: function() {
        startGuestAuthFromUnity();
    },
    
    signOutJslib: function() {
        console.log("signOutJslib: ログアウト処理開始");
        // HTML側のFirebase v9ログアウト関数を呼び出し
        if (typeof window.signOutFromUnity === 'function') {
            window.signOutFromUnity();
            console.log("signOutJslib: HTML側のログアウト関数を呼び出し");
        } else {
            console.error("❌ signOutFromUnity関数が見つかりません");
            if (window.unityInstance) {
                window.unityInstance.SendMessage('AuthManager', 'OnSignOutComplete', 'error');
            }
        }
    },
    
    // Firestoreデータ保存（橋渡し専用）
    SaveToFirestoreJslib: function(dataPointer) {
        var data = UTF8ToString(dataPointer);
        console.log("SaveToFirestoreJslib: Unityからデータを受信", data);
        
        // HTML側のFirestore処理関数を呼び出し
        if (typeof window.saveToFirestoreFromUnity === 'function') {
            window.saveToFirestoreFromUnity(data);
        } else {
            console.error("❌ saveToFirestoreFromUnity関数が見つかりません");
            if (window.unityInstance) {
                window.unityInstance.SendMessage('FirestoreConnection', 'OnSaveComplete', 'error');
            }
        }
    },

    // Firestoreデータ存在チェック（橋渡し専用）
    CheckFirestoreUserDataExistsJslib: function() {
        console.log("CheckFirestoreUserDataExistsJslib: Unityからデータ存在チェック要求");
        if (typeof window.checkFirestoreUserDataExists === 'function') {
            try {
                if (window.auth && window.auth.currentUser && window.db) {
                    const user = window.auth.currentUser;
                    console.log(`データ存在チェック: ユーザー認証済み (UID: ${user.uid})`);
                    // 非同期でFirestoreチェックを実行し、結果をコールバックで返す
                    window.checkFirestoreUserDataExists().then((exists) => {
                        console.log(`Firestoreデータ存在チェック結果: ${exists}`);
                        // Unityに結果を送信
                        if (window.unityInstance) {
                            window.unityInstance.SendMessage('FirestoreConnection', 'OnDataExistsCheckComplete', exists.toString());
                        }
                    }).catch((error) => {
                        console.error("Firestoreデータ存在チェックエラー:", error);
                        // エラーの場合はfalseを送信
                        if (window.unityInstance) {
                            window.unityInstance.SendMessage('FirestoreConnection', 'OnDataExistsCheckComplete', 'false');
                        }
                    });
                    return true; // 非同期処理開始を示す
                }
                console.log("データ存在チェック: ユーザー未認証");
                return false;
            } catch (error) {
                console.error("データ存在チェックエラー:", error);
                return false;
            }
        } else {
            console.error("❌ checkFirestoreUserDataExists関数が見つかりません");
            return false;
        }
    },

    // Firestoreデータロード（橋渡し専用）
    LoadFirestoreUserDataJslib: function() {
        console.log("LoadFirestoreUserDataJslib: Unityからデータロード要求");
        if (typeof window.loadFirestoreUserData === 'function') {
            try {
                window.loadFirestoreUserData();
                console.log("Firestoreデータロード開始");
            } catch (error) {
                console.error("Firestoreデータロードエラー:", error);
            }
        } else {
            console.error("❌ loadFirestoreUserData関数が見つかりません");
        }
    },
    
    // 認証状態管理
    GetLastAuthMethodJslib: function() {
        return localStorage.getItem('lastAuthMethod') || '';
    },
    
    SaveLastAuthMethodJslib: function(methodPointer) {
        var method = UTF8ToString(methodPointer);
        localStorage.setItem('lastAuthMethod', method);
        console.log("認証方法を保存:", method);
    },
    
    // ===== GoogleApi.jslib から統合（必要な環境検出のみ） =====
    // 環境検出
    IsChromebookJslib: function() {
        return navigator.userAgent.indexOf('CrOS') !== -1;
    },
    
    IsMicrosoftEnvironmentJslib: function() {
        return navigator.userAgent.indexOf('Windows') !== -1;
    },
    
    // ===== FirebaseApi.jslib から統合（重複関数の統合） =====
    // 古い関数名との互換性のため、両方の名前を提供
    GetLastAuthMethodJslib: function() {
        return localStorage.getItem('lastAuthMethod') || '';
    },
    
    // Firestoreからのデータ読み込み（橋渡し専用）
    LoadFromFirestoreJslib: function() {
        console.log("LoadFromFirestoreJslib: Unityからデータ読み込み要求");
        if (typeof window.loadFirestoreUserData === 'function') {
            window.loadFirestoreUserData();
        } else {
            console.error("❌ loadFirestoreUserData関数が見つかりません");
            if (window.unityInstance) {
                window.unityInstance.SendMessage('FirestoreConnection', 'OnLoadComplete', 'error');
            }
        }
    },
    
    // Firebase設定（設定ポインタ版 - 互換性のため）
    setFirebaseConfigPointerJslib: function(configPointer) {
        var config = UTF8ToString(configPointer);
        console.log("setFirebaseConfigPointerJslib: 設定ポインタ版", config);
        // JSON版の関数を呼び出し
        this.setFirebaseConfigJslib(config);
    },
    
    // ===== Connection.csで使用されている古い関数（ビルドエラー解決用） =====
    

    
    // ユーザデータの最新性を判定（アイテム数 → ゴールド数の順）
    compareUserDataVersion: function(localData, firebaseData) {
        try {
            // ローカルデータをパース
            var local = JSON.parse(localData);
            var firebase = firebaseData;
            
            // アイテム数をカウント
            var localItemCount = 0;
            var firebaseItemCount = 0;
            
            if (local.Items && Array.isArray(local.Items)) {
                for (var i = 0; i < local.Items.length; i++) {
                    if (local.Items[i] === true) localItemCount++;
                }
            }
            
            if (firebase.Items && Array.isArray(firebase.Items)) {
                for (var i = 0; i < firebase.Items.length; i++) {
                    if (firebase.Items[i] === true) firebaseItemCount++;
                }
            }
            
            console.log(`バージョン比較: ローカルアイテム数=${localItemCount}, Firebaseアイテム数=${firebaseItemCount}`);
            
            // アイテム数で比較
            if (localItemCount > firebaseItemCount) {
                console.log("ローカルデータの方が新しい（アイテム数が多い）");
                return "local";
            } else if (localItemCount < firebaseItemCount) {
                console.log("Firebaseデータの方が新しい（アイテム数が多い）");
                return "firebase";
            }
            
            // アイテム数が同じ場合、ゴールド数で比較
            var localGold = local.Status && local.Status[0] ? local.Status[0] : 0;
            var firebaseGold = firebase.Status && firebase.Status[0] ? firebase.Status[0] : 0;
            
            console.log(`ユーザデータバージョン比較: ローカルゴールド=${localGold}, Firebaseゴールド=${firebaseGold}`);
            
            if (localGold > firebaseGold) {
                console.log("ローカルデータの方が新しい（ゴールドが多い）");
                return "local";
            } else if (localGold < firebaseGold) {
                console.log("Firebaseデータの方が新しい（ゴールドが多い）");
                return "firebase";
            }
            
            // 両方同じ場合はFirebaseを優先
            console.log("ユーザデータが同じ - Firebaseを優先");
            return "firebase";
            
        } catch (e) {
            console.error("ユーザデータバージョン比較中にエラーが発生:", e);
            // エラーの場合はFirebaseを優先
            return "firebase";
        }
    },
    
    // Unityから呼び出されるユーザデータバージョン比較関数
    CompareUserDataVersion: function(localDataPointer, firebaseDataPointer) {
        var localData = UTF8ToString(localDataPointer);
        var firebaseData = UTF8ToString(firebaseDataPointer);
        
        console.log("CompareUserDataVersion: Unityからユーザデータバージョン比較要求");
        
        // ユーザデータバージョン比較を実行
        // ユーザデータバージョン比較ロジックを直接実装
        try {
            // ローカルデータをパース
            var local = JSON.parse(localData);
            var firebase = JSON.parse(firebaseData);
            
            // アイテム数をカウント
            var localItemCount = 0;
            var firebaseItemCount = 0;
            
            if (local.Items && Array.isArray(local.Items)) {
                for (var i = 0; i < local.Items.length; i++) {
                    if (local.Items[i] === true) localItemCount++;
                }
            }
            
            if (firebase.Items && Array.isArray(firebase.Items)) {
                for (var i = 0; i < firebase.Items.length; i++) {
                    if (firebase.Items[i] === true) firebaseItemCount++;
                }
            }
            
            console.log(`ユーザデータバージョン比較: ローカルアイテム数=${localItemCount}, Firebaseアイテム数=${firebaseItemCount}`);
            
            // アイテム数で比較
            var result = "firebase"; // デフォルト
            if (localItemCount > firebaseItemCount) {
                console.log("ローカルデータの方が新しい（アイテム数が多い）");
                result = "local";
            } else if (localItemCount < firebaseItemCount) {
                console.log("Firebaseデータの方が新しい（アイテム数が多い）");
                result = "firebase";
            } else {
                // アイテム数が同じ場合、ゴールド数で比較
                var localGold = local.Status && local.Status[0] ? local.Status[0] : 0;
                var firebaseGold = firebase.Status && local.Status[0] ? firebase.Status[0] : 0;
                
                console.log(`ユーザデータバージョン比較: ローカルゴールド=${localGold}, Firebaseゴールド=${firebaseGold}`);
                
                if (localGold > firebaseGold) {
                    console.log("ローカルデータの方が新しい（ゴールドが多い）");
                    result = "local";
                } else if (localGold < firebaseGold) {
                    console.log("Firebaseデータの方が新しい（ゴールドが多い）");
                    result = "firebase";
                } else {
                    console.log("ユーザデータが同じ - Firebaseを優先");
                    result = "firebase";
                }
            }
        } catch (e) {
            console.error("ユーザデータバージョン比較中にエラーが発生:", e);
            var result = "firebase"; // エラーの場合はFirebaseを優先
        }
        
        console.log(`ユーザデータバージョン比較結果: ${result}`);
        
        // Unityに結果を送信
        if (window.unityInstance) {
            if (result === "local") {
                // ローカルデータの方が新しい場合
                window.unityInstance.SendMessage('TitleScene', 'UseLocalData');
            } else {
                // Firebaseデータの方が新しい場合
                window.unityInstance.SendMessage('TitleScene', 'UseFirebaseData');
            }
        } else {
            console.error("Unityインスタンスが見つかりません");
        }
    },
    
    // データ管理
    SaveStatusToLocalJslib: function(dataPointer) {
        var data = UTF8ToString(dataPointer);
        var accountKey = window.getAccountSpecificKey('statusData');
        localStorage.setItem(accountKey, data);
        console.log(`データをLocalStorageに保存しました (${accountKey}):`, data);
    },
    
    GetNecoRank: function() {
        getNecoRank();
    },

    // アカウント固有のデータをクリア
    ClearAccountData: function() {
        try {
            var currentUser = null;
            if (typeof window.auth !== 'undefined' && window.auth.currentUser) {
                currentUser = window.auth.currentUser;
            }
            
            if (currentUser && currentUser.email) {
                var emailKey = currentUser.email.replace(/[^a-zA-Z0-9]/g, '_');
                
                // アカウント固有のキーでデータをクリア
                var statusKey = 'statusData_' + emailKey;
                var rankingKey = 'rankingData_' + emailKey;
                
                localStorage.removeItem(statusKey);
                localStorage.removeItem(rankingKey);
                
                console.log(`アカウント固有データをクリアしました: ${statusKey}, ${rankingKey}`);
            } else {
                console.log("認証ユーザーが見つからないため、データクリアをスキップ");
            }
        } catch (e) {
            console.error("アカウントデータクリア中にエラーが発生:", e);
        }
    },
    
    // Google Spreadsheet関連
    LoadFromGssJslib: function() {
        loadFromGss();
    },
    
    SaveToGssJslib: function(dataPointer) {
        console.log("Received pointer:", dataPointer);
        var data = UTF8ToString(dataPointer);
        console.log("Converted data:", data);
        saveToGss(data);
    },
    
    // Gemini API関連
    ThroughGeminiJslib: function(dataPointer) {
        console.log("Received pointer:", dataPointer);
        var data = UTF8ToString(dataPointer);
        console.log("Converted data:", data);
        throughGemini(data);
    },
    
    // ===== Firebase設定を一括で設定する関数 =====
    setFirebaseConfigJslib: function(configJson) {
        try {
            var config = JSON.parse(UTF8ToString(configJson));
            console.log("setFirebaseConfig: Firebase設定を受信", config);
            
            // 各設定値をwindow変数に設定
            window.unityAppCode = config.appId || '';
            window.unityServiceToken = config.apiKey || '';
            window.unityProjectCode = config.projectId || '';
            window.unityAuthDomain = config.authDomain || '';
            window.unityStorageBucket = config.storageBucket || '';
            window.unityMessagingSenderCode = config.messagingSenderId || '';
            window.unityDatabaseURL = config.databaseURL || '';
            window.unityIsProduction = config.isProduction || false;
            
            console.log("✅ Firebase設定をUnity側から設定完了");
            console.log("設定値:", {
                appId: window.unityAppCode,
                apiKey: window.unityServiceToken,
                projectId: window.unityProjectCode,
                authDomain: window.unityAuthDomain,
                storageBucket: window.unityStorageBucket,
                messagingSenderId: window.unityMessagingSenderCode,
                databaseURL: window.unityDatabaseURL,
                isProduction: window.unityIsProduction
            });
            
            // Firebase初期化を実行 (削除)
            // if (typeof window.initializeFirebaseFromUnity === 'function') {
            //     console.log("initializeFirebaseFromUnity関数を呼び出し");
            //     window.initializeFirebaseFromUnity();
            // } else {
            //     console.warn("⚠️ initializeFirebaseFromUnity関数が見つかりません");
            // }
            
        } catch (error) {
            console.error("❌ setFirebaseConfigでエラーが発生:", error);
            console.error("エラー詳細:", error.stack);
        }
    },
    

    // セーブ機能（23時間制限付き）
    SaveToFirestoreWithLimitJslib: function(dataPointer, limitHours) {
        var data = UTF8ToString(dataPointer);
        var hours = parseInt(UTF8ToString(limitHours));
        console.log("SaveToFirestoreWithLimitJslib: 制限付きセーブ開始");
        
        // 制限チェック（アカウント固有）
                        var lastAccessKey = "lastFirebaseAccess_save_" + window.getAccountSpecificKey('statusData').split('_')[1];
        var lastAccessTime = localStorage.getItem(lastAccessKey);
        var canAccess = "false";
        
        if (!lastAccessTime) {
            console.log("save: 初回アクセス - 制限なし");
            canAccess = "true";
        } else {
            var lastAccess = new Date(lastAccessTime);
            var now = new Date();
            var timeDiff = now.getTime() - lastAccess.getTime();
            var hoursDiff = timeDiff / (1000 * 60 * 60);
            
            if (hoursDiff >= hours) {
                console.log(`save: ${hours}時間経過 - アクセス可能`);
                canAccess = "true";
            } else {
                console.log(`save: 制限中 - あと${(hours - hoursDiff).toFixed(2)}時間待機必要`);
                canAccess = "false";
            }
        }
        
        if (canAccess === "true") {
            console.log("Firebaseアクセス可能 - Firebaseに保存");
            if (typeof window.saveToFirestoreFromUnity === 'function') {
                window.saveToFirestoreFromUnity(data);
                // アクセス時刻を記録
                var now = new Date();
                localStorage.setItem(lastAccessKey, now.toISOString());
            } else {
                console.error("❌ saveToFirestoreFromUnity関数が見つかりません");
            }
        } else {
            console.log("Firebaseアクセス制限中 - ブラウザに保存");
            this.SaveStatusToLocalJslib(dataPointer);
            console.log("データをブラウザに保存しました");
            
            if (window.unityInstance) {
                window.unityInstance.SendMessage('FirestoreConnection', 'OnSaveComplete', 'limited');
            }
        }
    },
    
    // アプリバージョン管理
    CheckAppVersionJslib: function() {
        console.log("CheckAppVersionJslib: -> LoadAllDataFromFirestoreWithLimitJslib を呼び出しますにゃん。");
        this.LoadAllDataFromFirestoreWithLimitJslib("23");
    },
    
    // ロード機能（23時間制限付き）
    LoadFromFirestoreWithLimitJslib: function(limitHours) {
        console.log("LoadFromFirestoreWithLimitJslib: -> LoadAllDataFromFirestoreWithLimitJslib を呼び出しますにゃん。");
        this.LoadAllDataFromFirestoreWithLimitJslib(limitHours);
    },
    
    // 一括ロード機能（23時間制限付き）- ユーザ情報、ランキング情報、バージョン情報を一気に取得
    LoadAllDataFromFirestoreWithLimitJslib: function(limitHours) {
        var hours = parseInt(UTF8ToString(limitHours));
        console.log("LoadAllDataFromFirestoreWithLimitJslib: 一括ロード開始（制限付き）");
        
        // まず、ブラウザにデータがあるかチェック
        var accountKey = window.getAccountSpecificKey('statusData');
        var storedStatusData = localStorage.getItem(accountKey);
        
        // === 共通のFirebaseアクセス制限ロジック ===
        var lastAccessKey = "lastFirebaseAccess_loadAll_" + window.getAccountSpecificKey('statusData').split('_')[1];
        var lastAccessTime = localStorage.getItem(lastAccessKey);
        var canAccess = "false";
        
        if (!lastAccessTime) {
            console.log("loadAll: 初回アクセス - 制限なし");
            canAccess = "true";
        } else {
            var lastAccess = new Date(lastAccessTime);
            var now = new Date();
            var timeDiff = now.getTime() - lastAccess.getTime();
            var hoursDiff = timeDiff / (1000 * 60 * 60);
            
            if (hoursDiff >= hours) {
                console.log(`loadAll: ${hours}時間経過 - アクセス可能`);
                canAccess = "true";
            } else {
                console.log(`loadAll: 制限中 - あと${(hours - hoursDiff).toFixed(2)}時間待機必要`);
                canAccess = "false";
            }
        }

        // --- データロード処理 --- 
        // LocalStorageにデータが「ない」場合、またはFirebaseアクセスが「可能」な場合
        if (!storedStatusData || canAccess === "true") {
            console.log("Firebaseから全データ（ユーザ、ランキング、バージョン）を一括読み込み");
            
            // ユーザデータ、ランキングデータ、アプリバージョンデータを全て取得するためのPromise.all
            Promise.all([
                typeof window.loadFirestoreUserData === 'function' ? window.loadFirestoreUserData() : Promise.resolve({ statusData: null, source: 'none' }),
                typeof window.getNecoRank === 'function' ? new Promise(resolve => {
                    window.getNecoRank();
                    resolve(true);
                }) : Promise.resolve(false),
                typeof window.checkAppVersion === 'function' ? window.checkAppVersion() : Promise.resolve(null)
            ])
            .then(([userDataResult, rankingDataLoaded, versionInfo]) => {
                console.log("一括データ読み込み完了:", { userDataResult, rankingDataLoaded, versionInfo });
    
                    // Unityに全データを送信
                    if (window.unityInstance) {
                        const allData = {
                            statusData: userDataResult.statusData,
                            rankingData: null, // 後でランキングデータをここに追加
                            appVersion: versionInfo ? versionInfo.version : '',
                            source: userDataResult.source // FirebaseからかLocalStorageからかを伝える
                        };
                        window.unityInstance.SendMessage('FirestoreConnection', 'OnAllDataLoadComplete', JSON.stringify(allData));
                    }
                    
                    // アクセス時刻を記録（成功時のみ）
                    var now = new Date();
                    localStorage.setItem(lastAccessKey, now.toISOString());
                })
                .catch(error => {
                    console.error("一括データ読み込み中にエラーが発生しました:", error);
                    // エラーの場合もUnityに通知
                    if (window.unityInstance) {
                        const allData = {
                            statusData: null,
                            rankingData: null,
                            appVersion: '',
                            source: 'error'
                        };
                        window.unityInstance.SendMessage('FirestoreConnection', 'OnAllDataLoadComplete', JSON.stringify(allData));
                    }
                });
    
            } else { // Firebaseアクセスが「不可」で、LocalStorageにデータが「ある」場合
                console.log("Firebaseアクセス制限中 - ブラウザから読み込み");
                // LocalStorageからデータを読み込み、Unityに送信
                window.LoadDataFromLocal().then((localDataResult) => {
                    if (window.unityInstance) {
                        const allData = {
                            statusData: localDataResult.statusData,
                            rankingData: localDataResult.rankingData,
                            appVersion: '', // ローカルストレージからの読み込み時はアプリバージョンを取得しない
                            source: localDataResult.source // LocalStorageからロードされたことを伝える
                        };
                        window.unityInstance.SendMessage('FirestoreConnection', 'OnAllDataLoadComplete', JSON.stringify(allData));
                    }
                }).catch(error => {
                    console.error("LocalStorageからの読み込み中にエラーが発生しました:", error);
                    if (window.unityInstance) {
                        const allData = {
                            statusData: null,
                            rankingData: null,
                            appVersion: '',
                            source: 'error'
                        };
                        window.unityInstance.SendMessage('FirestoreConnection', 'OnAllDataLoadComplete', JSON.stringify(allData));
                    }
                });
                // ローカルストレージからの読み込み時はアクセス時刻を更新しない (ねこ隊長のご指示通り)
            }
        },
});

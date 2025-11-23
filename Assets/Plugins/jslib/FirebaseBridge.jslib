mergeInto(LibraryManager.library, {
    // ===== 制限時間設定 =====
    firebaseAccessLimitHoursLoad: 23, // デフォルト値
    firebaseAccessLimitHoursSave: 23, // デフォルト値
    
    // Unity側から制限時間を設定
    SetFirebaseAccessLimitHoursJslib: function(loadHours, saveHours) {
        this.firebaseAccessLimitHoursLoad = loadHours;
        this.firebaseAccessLimitHoursSave = saveHours;
        console.log(`jslib内制限時間Loadを${loadHours}, Saveを${saveHours}時間に設定しました`);
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
    
    // Firestoreユーザーデータ保存（橋渡し専用）
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
    
    // 一括ロード機能（23時間制限付き）- ユーザ情報、ランキング情報、バージョン情報を一気に取得
    LoadAllDataFromFirestoreWithLimitJslib: function(limitHours) {
        var hours = parseInt(UTF8ToString(limitHours));
        console.log("LoadAllDataFromFirestoreWithLimitJslib: 一括ロード開始（制限付き）");
        
        // 最終的にUnityに送信するデータ
        let finalStatusData = null;
        let finalRankingList = [];
        let finalAppVersion = '';
        let finalSource = 'none';

        let localStatusData = null;
        let localRankingData = [];
        let firebaseStatusData = null;
        let firebaseRankingData = [];
        let firebaseAppVersionData = null;

        let isFirebaseAccessed = false; // Firebaseへのアクセスが成功したかどうかのフラグ

        // 1. 常にLocalStorageからのデータ読み込みを試行
        window.LoadDataFromLocal().then(async (localResult) => { // async を追加
            localStatusData = localResult.statusData;
            localRankingData = localResult.rankingData;
            
            // 2. Firebaseへのアクセス制限チェック
            var lastAccessKey = "lastFirebaseAccess_loadAll_" + window.getAccountSpecificKey('statusData').split('_')[1];
            var lastAccessTime = localStorage.getItem(lastAccessKey);
            var canAccessFirebase = false;
            
            if (!lastAccessTime) {
                console.log("loadAll: 初回Firebaseアクセス - 制限なし");
                canAccessFirebase = true;
            } else {
                var lastAccess = new Date(lastAccessTime);
                var now = new Date();
                var timeDiff = now.getTime() - lastAccess.getTime();
                var hoursDiff = timeDiff / (1000 * 60 * 60);
                
                if (hoursDiff >= hours) {
                    console.log(`loadAll: ${hours}時間経過 - Firebaseアクセス可能`);
                    canAccessFirebase = true;
                } else {
                    console.log(`loadAll: Firebase制限中 - あと${(hours - hoursDiff).toFixed(2)}時間待機必要`);
                }
            }

            // 3. Firebaseアクセスが可能な場合のみFirebaseからデータ読み込み
            if (canAccessFirebase) {
                console.log("Firebaseからデータ（ユーザ、ランキング、バージョン）を一括読み込み");
                try {
                    const [userDataResult, rankingList, versionInfo] = await Promise.all([
                        typeof window.loadFirestoreUserData === 'function' ? window.loadFirestoreUserData() : Promise.resolve({ statusData: null, source: 'none' }),
                        typeof window.getNecoRank === 'function' ? window.getNecoRank() : Promise.resolve([]),
                        typeof window.checkAppVersion === 'function' ? window.checkAppVersion() : Promise.resolve(null)
                    ]);
                    firebaseStatusData = userDataResult.statusData;
                    firebaseRankingData = rankingList;
                    firebaseAppVersionData = versionInfo;

                    // アクセス時刻を記録（成功時のみ）
                    var now = new Date();
                    localStorage.setItem(lastAccessKey, now.toISOString());
                    isFirebaseAccessed = true; // Firebaseにアクセス成功

                    // Firebaseからの読み込みに成功した場合、ランキングデータはLocalStorageに自動保存
                    if (firebaseRankingData && firebaseRankingData.length > 0) {
                        const rankingAccountKey = window.getAccountSpecificKey('rankingData');
                        localStorage.setItem(rankingAccountKey, JSON.stringify(firebaseRankingData));
                        console.log("💾 FirebaseランキングデータをLocalStorageに自動保存 (LoadAllData):", rankingAccountKey);
                    }
                    // アクセス時刻を記録（成功時のみ）
                    var now = new Date();
                    localStorage.setItem(lastAccessKey, now.toISOString());
                    isFirebaseAccessed = true; // Firebaseにアクセス成功

                } catch (error) {
                    console.error("Firebase一括データ読み込み中にエラーが発生しました:", error);
                    // エラー時もデータはnullとして続行
                }
            }

            // 4. 最強データを決定
            finalStatusData = localStatusData; // デフォルトはローカルデータ
            finalRankingList = localRankingData; // デフォルトはローカルランキング
            finalAppVersion = firebaseAppVersionData ? firebaseAppVersionData.version : '';// アプリバージョンはFirebase
            finalSource = 'local';

            // ユーザーデータ比較
            if (firebaseStatusData && localStatusData) {
                // HTML側のバージョン比較関数を呼び出す（window.compareUserDataVersionが存在する場合）
                if (typeof window.compareUserDataVersion === 'function') {
                    const compareResult = window.compareUserDataVersion(JSON.stringify(localStatusData), JSON.stringify(firebaseStatusData));
                    if (compareResult === 'firebase') {
                        finalStatusData = firebaseStatusData;
                        finalRankingList = firebaseRankingData;
                        finalSource = 'firebase';
                    } else {
                        // ローカルデータが最強の場合（現状維持）
                        console.log("JSLib: ローカルデータが最強と判断");
                    }
                } else {
                    // HTML側に比較関数がない場合、Firebaseデータを優先
                    console.warn("JSLib: window.compareUserDataVersion が見つかりません。Firebaseデータをデフォルトで優先します。");
                    finalStatusData = firebaseStatusData;
                    finalRankingList = firebaseRankingData;
                    finalSource = 'firebase';
                }
            } else if (firebaseStatusData && !localStatusData) {
                // ローカルデータがない場合はFirebaseデータを採用
                finalStatusData = firebaseStatusData;
                finalRankingList = firebaseRankingData;
                finalSource = 'firebase';
            } else if (!firebaseStatusData && !localStatusData) {
                // 両方ない場合はnullのまま（新規ユーザーと判断）
                finalStatusData = null;
                finalSource = 'none';
            }

            // 5. Unityに決定された最強データを送信
            if (window.unityInstance) {
                const allData = {
                    statusData: finalStatusData,
                    rankingData: finalRankingList,
                    appVersion: finalAppVersion,
                    source: finalSource,
                    isFirebaseAccessed: isFirebaseAccessed // Firebaseアクセス成功フラグを追加
                };
                window.unityInstance.SendMessage('FirestoreConnection', 'OnAllDataLoadComplete', JSON.stringify(allData));
            }
        })
        .catch(error => {
            console.error("LoadAllDataFromFirestoreWithLimitJslib全体でエラーが発生:", error);
            // 何らかの理由でPromiseチェーン全体が失敗した場合、Unityにエラーを通知
            if (window.unityInstance) {
                const allData = {
                    statusData: null,
                    rankingData: [],
                    appVersion: '',
                    source: 'error',
                    isFirebaseAccessed: false // エラー時はfalse
                };
                window.unityInstance.SendMessage('FirestoreConnection', 'OnAllDataLoadComplete', JSON.stringify(allData));
            }
        });
    },

    // 初期データセーブ機能（23時間制限付き）
    SaveToFirestoreWithLimitJslib: function(dataPointer, limitHours) {
        var data = UTF8ToString(dataPointer);
        var hours = parseInt(UTF8ToString(limitHours));
        
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
        }
    },

    // 追加：統合データ保存のためのJSLib関数
    SaveCombinedDataToFBJslib: function(combinedDataJsonPointer, limitHoursPointer) {
        console.log("SaveCombinedDataToFBJslib: 統合データを保存開始");
        const combinedDataJson = UTF8ToString(combinedDataJsonPointer);
        const hours = parseInt(UTF8ToString(limitHoursPointer));

        // 制限チェック（アカウント固有）
        var lastAccessKey = "lastFirebaseAccess_saveCombined_" + window.getAccountSpecificKey('statusData').split('_')[1];
        var lastAccessTime = localStorage.getItem(lastAccessKey);
        var canAccess = "false";
        
        if (!lastAccessTime) {
            console.log("saveCombined: 初回アクセス - 制限なし");
            canAccess = "true";
        } else {
            var lastAccess = new Date(lastAccessTime);
            console.log(`DEBUG: SaveCombined - lastAccessKey: ${lastAccessKey}`);
            console.log(`DEBUG: SaveCombined - lastAccessTime (Raw): "${lastAccessTime}"`);
            console.log(`DEBUG: SaveCombined - Parsed lastAccess (Date object): ${lastAccess}`);
            console.log(`DEBUG: SaveCombined - lastAccess.getTime(): ${lastAccess.getTime()}`);

            // lastAccess が有効な日付かどうかをチェック
            if (isNaN(lastAccess.getTime())) {
                console.log(`DEBUG: SaveCombined - Parsed lastAccess is Invalid Date.`);
                console.warn(`saveCombined: 記録された最終アクセス時刻が不正です (${lastAccessTime})。初回アクセスとして処理します。`);
                canAccess = "true"; // 初回アクセスとして扱う
                // 不正な値をクリアし、次回からは正しい形式で保存されるようにする
                localStorage.removeItem(lastAccessKey);
            } else {
                var now = new Date();
                var timeDiff = now.getTime() - lastAccess.getTime();
                var hoursDiff = timeDiff / (1000 * 60 * 60);
                console.log(`DEBUG: SaveCombined - now.getTime(): ${now.getTime()}`);
                console.log(`DEBUG: SaveCombined - timeDiff: ${timeDiff}`);
                console.log(`DEBUG: SaveCombined - hoursDiff: ${hoursDiff}`);
                
                if (hoursDiff >= hours) {
                    console.log(`saveCombined: ${hours}時間経過 - アクセス可能`);
                    canAccess = "true";
                } else {
                    console.log(`saveCombined: 制限中 - あと${(hours - hoursDiff).toFixed(2)}時間待機必要`);
                    canAccess = "false";
                    if (window.unityInstance) {
                        // アクセス制限中の場合はUnityに"limited"を通知
                        window.unityInstance.SendMessage('FirestoreConnection', 'OnSaveComplete', 'limited');
                    }
                }
            }
        }
        
        if (canAccess === "true") {
            console.log("Firebaseアクセス可能 - 統合データを保存");
            // HTML側の関数を呼び出す
            if (typeof window.saveCombinedDataToFBFromUnity === 'function') {
                window.saveCombinedDataToFBFromUnity(combinedDataJson);
                // アクセス時刻を記録
                var now = new Date();
                localStorage.setItem(lastAccessKey, now.toISOString());
            } else {
                console.error("❌ window.saveCombinedDataToFBFromUnity関数が見つかりません");
                if (window.unityInstance) {
                    window.unityInstance.SendMessage('FirestoreConnection', 'OnSaveComplete', 'error');
                }
            }
        }
    },

    // Unity側からFirebase設定完了の通知を受け取る
    FirebaseConfigLoadedJslib: function() {
        console.log("FirebaseConfigLoadedJslib: UnityからFirebase設定完了通知を受信しました。");
        if (typeof window.onFirebaseConfigLoaded === 'function') {
            window.onFirebaseConfigLoaded();
        } else {
            console.error("❌ window.onFirebaseConfigLoaded関数が見つかりません");
        }
    },
});
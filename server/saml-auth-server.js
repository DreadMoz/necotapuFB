const express = require('express');
const admin = require('firebase-admin');
const passport = require('passport');
const SamlStrategy = require('passport-saml').Strategy;
const cors = require('cors');

const app = express();
const PORT = process.env.PORT || 3000;

// CORS設定
app.use(cors({
    origin: ['http://localhost:8000', 'http://localhost:8001', 'https://necotapufb.firebaseapp.com'],
    credentials: true
}));

// Firebase Admin SDK初期化
const serviceAccount = require('./firebase-service-account.json');
admin.initializeApp({
    credential: admin.credential.cert(serviceAccount),
    databaseURL: 'https://necotapufb.firebaseio.com'
});

// SAML設定
const samlConfig = {
    entryPoint: 'https://your-saml-idp.com/sso', // 学校のSAML IdP
    issuer: 'https://necotapufb.firebaseapp.com',
    callbackUrl: 'https://your-server.com/auth/saml/callback',
    cert: 'your-saml-certificate.pem', // SAML証明書
    privateCert: 'your-private-key.pem' // 秘密鍵
};

// Passport SAML戦略
passport.use(new SamlStrategy(samlConfig, (profile, done) => {
    // SAML認証成功時の処理
    return done(null, {
        id: profile.nameID,
        email: profile.email,
        displayName: profile.displayName,
        schoolId: profile.schoolId,
        studentId: profile.studentId
    });
}));

// セッション設定
app.use(express.json());
app.use(passport.initialize());

// SAML認証開始
app.get('/auth/saml', passport.authenticate('saml', {
    failureRedirect: '/auth/failure',
    failureFlash: true
}));

// SAML認証コールバック
app.get('/auth/saml/callback', passport.authenticate('saml', {
    failureRedirect: '/auth/failure',
    failureFlash: true
}), async (req, res) => {
    try {
        const user = req.user;
        
        // Firebaseカスタムトークンを生成
        const customToken = await admin.auth().createCustomToken(user.id, {
            email: user.email,
            displayName: user.displayName,
            schoolId: user.schoolId,
            studentId: user.studentId
        });
        
        // トークンをクライアントに返す
        res.json({
            success: true,
            customToken: customToken,
            user: {
                id: user.id,
                email: user.email,
                displayName: user.displayName,
                schoolId: user.schoolId,
                studentId: user.studentId
            }
        });
    } catch (error) {
        console.error('SAML認証エラー:', error);
        res.status(500).json({
            success: false,
            error: error.message
        });
    }
});

// 認証失敗
app.get('/auth/failure', (req, res) => {
    res.json({
        success: false,
        error: 'SAML認証に失敗しました'
    });
});

// ヘルスチェック
app.get('/health', (req, res) => {
    res.json({ status: 'OK', timestamp: new Date().toISOString() });
});

// サーバー起動
app.listen(PORT, () => {
    console.log(`SAML認証サーバーが起動しました: http://localhost:${PORT}`);
});

module.exports = app; 
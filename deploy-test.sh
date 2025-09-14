#!/bin/bash

echo "🚀 Deploying to TEST environment..."

# テスト環境に切り替え
firebase use firetyping-ad101

# テスト環境にデプロイ
firebase deploy --only hosting --debug

echo "✅ Test deployment completed!"
echo "🌐 Test URL: https://firetyping-ad101.web.app"

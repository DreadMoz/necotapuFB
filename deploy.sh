#!/bin/bash

echo "🚀 Deploying to PRODUCTION environment..."

# 本番環境に切り替え
firebase use necotapufb

# 本番環境にデプロイ
firebase deploy

echo "✅ Production deployment completed!"
echo "🌐 Production URL: https://necotapufb.web.app"

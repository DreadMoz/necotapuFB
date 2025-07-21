using System;

/// <summary>
/// 認証の優先順位を定義
/// </summary>
public enum AuthPriority
{
    Google = 1,      // 最優先（自治体採用率高）
    Microsoft = 2,   // 次優先（全国展開対応）
    Anonymous = 3    // フォールバック（イベント用）
}

/// <summary>
/// 認証結果を表す
/// </summary>
public enum AuthResult
{
    Success,
    Failed,
    Cancelled,
    NotSupported
}

/// <summary>
/// 認証情報を表す
/// </summary>
[Serializable]
public class AuthInfo
{
    public string UserId { get; set; }
    public string Email { get; set; }
    public string DisplayName { get; set; }
    public AuthPriority AuthMethod { get; set; }
    public bool IsGuest { get; set; }
    public DateTime AuthenticatedAt { get; set; }
} 
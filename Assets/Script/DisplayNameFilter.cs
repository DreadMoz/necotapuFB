using System.Text;
using TMPro;

/// <summary>
/// ユーザー名を画面に表示する直前に通す変換。
/// フォントに入っていない文字を「？」に置き換える（保存データは変更しない）。
/// フォントが Dynamic の間は、アトラスにない文字でも表示できるため何も変換しない。
/// Static フォントに切り替えたときから置き換えが効く。
/// </summary>
public static class DisplayNameFilter
{
    private const string Replacement = "？";

    public static string ToDisplay(string name, TMP_FontAsset font)
    {
        if (string.IsNullOrEmpty(name) || font == null)
        {
            return name;
        }
        // Dynamic フォントは実行時に文字を追加できるため、従来どおり表示する。
        if (font.atlasPopulationMode == AtlasPopulationMode.Dynamic)
        {
            return name;
        }

        // 文字テーブルを確実に初期化してから、サロゲートペア用の int 版で判定する。
        font.HasCharacter(' ', true);

        var sb = new StringBuilder(name.Length);
        for (int i = 0; i < name.Length; i++)
        {
            int cp = name[i];
            if (char.IsHighSurrogate(name[i]) && i + 1 < name.Length && char.IsLowSurrogate(name[i + 1]))
            {
                cp = char.ConvertToUtf32(name[i], name[i + 1]);
                i++;
            }

            if (IsAvailable(font, cp))
            {
                sb.Append(char.ConvertFromUtf32(cp));
                continue;
            }

            // 異体字セレクタ・結合用の文字は、単独では表示しない。
            if (IsJoiner(cp))
            {
                continue;
            }

            // 絵文字などに続く修飾（異体字セレクタ、肌色、ZWJ でつながる文字）は 1 つの「？」にまとめる。
            sb.Append(Replacement);
            while (i + 1 < name.Length)
            {
                int next = name[i + 1];
                int nextLen = 1;
                if (char.IsHighSurrogate(name[i + 1]) && i + 2 < name.Length && char.IsLowSurrogate(name[i + 2]))
                {
                    next = char.ConvertToUtf32(name[i + 1], name[i + 2]);
                    nextLen = 2;
                }
                if (!IsJoiner(next) && !IsSkinTone(next))
                {
                    break;
                }
                i += nextLen;
                if (next == 0x200D && i + 1 < name.Length)
                {
                    // ZWJ の次の 1 文字も同じ絵文字の一部として読み飛ばす。
                    i += (char.IsHighSurrogate(name[i + 1]) && i + 2 < name.Length && char.IsLowSurrogate(name[i + 2])) ? 2 : 1;
                }
            }
        }
        return sb.ToString();
    }

    private static bool IsAvailable(TMP_FontAsset font, int cp)
    {
        if (cp <= 0xFFFF)
        {
            return font.HasCharacter((char)cp, true);
        }
        return font.HasCharacter(cp);
    }

    // 異体字セレクタ（FE00〜FE0F）、ZWJ、キーキャップ結合文字、タグ文字
    private static bool IsJoiner(int cp)
    {
        return (cp >= 0xFE00 && cp <= 0xFE0F) || cp == 0x200D || cp == 0x20E3 || (cp >= 0xE0020 && cp <= 0xE007F);
    }

    // 絵文字の肌色修飾（1F3FB〜1F3FF）
    private static bool IsSkinTone(int cp)
    {
        return cp >= 0x1F3FB && cp <= 0x1F3FF;
    }
}

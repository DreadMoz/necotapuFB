using System;
using System.Collections.Generic;
using Shapes2D;
using TMPro;
using UnityEngine;

public class AssistKeyboardJIS : MonoBehaviour
{
    [SerializeField] GameObject AKParent;

    [SerializeField]
    private GameObject lHand;        // Lハンドオブジェクト
    [SerializeField]
    private GameObject rHand;        // Rハンドオブジェクト
    private Animator rightHandAnim;  // rightHandのアニメーター
    private Animator leftHandAnim;   // leftHandのアニメーター

  // key_name -> GameObject のマップ
    private static Dictionary<string, GameObject> AKKeys = new Dictionary<string, GameObject>();
  // finger_name -> GameObject のマップ
  private static Dictionary<string, GameObject> AFingers = new Dictionary<string, GameObject>();
  // JIS 配列での文字とキーのマッピング
  private static readonly Dictionary<string, string[]> keyMappingJIS = new Dictionary<string, string[]>() {
    {"0", new string[1] {"Key_0"}},
    {"1", new string[1] {"Key_1"}},
    {"2", new string[1] {"Key_2"}},
    {"3", new string[1] {"Key_3"}},
    {"4", new string[1] {"Key_4"}},
    {"5", new string[1] {"Key_5"}},
    {"6", new string[1] {"Key_6"}},
    {"7", new string[1] {"Key_7"}},
    {"8", new string[1] {"Key_8"}},
    {"9", new string[1] {"Key_9"}},
    {"a", new string[1] {"Key_A"}},
    {"b", new string[1] {"Key_B"}},
    {"c", new string[1] {"Key_C"}},
    {"d", new string[1] {"Key_D"}},
    {"e", new string[1] {"Key_E"}},
    {"f", new string[1] {"Key_F"}},
    {"g", new string[1] {"Key_G"}},
    {"h", new string[1] {"Key_H"}},
    {"i", new string[1] {"Key_I"}},
    {"j", new string[1] {"Key_J"}},
    {"k", new string[1] {"Key_K"}},
    {"l", new string[1] {"Key_L"}},
    {"m", new string[1] {"Key_M"}},
    {"n", new string[1] {"Key_N"}},
    {"o", new string[1] {"Key_O"}},
    {"p", new string[1] {"Key_P"}},
    {"q", new string[1] {"Key_Q"}},
    {"r", new string[1] {"Key_R"}},
    {"s", new string[1] {"Key_S"}},
    {"t", new string[1] {"Key_T"}},
    {"u", new string[1] {"Key_U"}},
    {"v", new string[1] {"Key_V"}},
    {"w", new string[1] {"Key_W"}},
    {"x", new string[1] {"Key_X"}},
    {"y", new string[1] {"Key_Y"}},
    {"z", new string[1] {"Key_Z"}},
    {"A", new string[2] {"Key_A", "Key_RShift"}},
    {"B", new string[2] {"Key_B", "Key_RShift"}},
    {"C", new string[2] {"Key_C", "Key_RShift"}},
    {"D", new string[2] {"Key_D", "Key_RShift"}},
    {"E", new string[2] {"Key_E", "Key_RShift"}},
    {"F", new string[2] {"Key_F", "Key_RShift"}},
    {"G", new string[2] {"Key_G", "Key_RShift"}},
    {"H", new string[2] {"Key_H", "Key_LShift"}},
    {"I", new string[2] {"Key_I", "Key_LShift"}},
    {"J", new string[2] {"Key_J", "Key_LShift"}},
    {"K", new string[2] {"Key_K", "Key_LShift"}},
    {"L", new string[2] {"Key_L", "Key_LShift"}},
    {"M", new string[2] {"Key_M", "Key_LShift"}},
    {"N", new string[2] {"Key_N", "Key_LShift"}},
    {"O", new string[2] {"Key_O", "Key_LShift"}},
    {"P", new string[2] {"Key_P", "Key_LShift"}},
    {"Q", new string[2] {"Key_Q", "Key_RShift"}},
    {"R", new string[2] {"Key_R", "Key_RShift"}},
    {"S", new string[2] {"Key_S", "Key_RShift"}},
    {"T", new string[2] {"Key_T", "Key_RShift"}},
    {"U", new string[2] {"Key_U", "Key_LShift"}},
    {"V", new string[2] {"Key_V", "Key_RShift"}},
    {"W", new string[2] {"Key_W", "Key_RShift"}},
    {"X", new string[2] {"Key_X", "Key_RShift"}},
    {"Y", new string[2] {"Key_Y", "Key_LShift"}},
    {"Z", new string[2] {"Key_Z", "Key_RShift"}},
    {" ", new string[1] {"Key_Space"}},
    {"-", new string[1] {"Key_Hyphen"}},
    {",", new string[1] {"Key_Comma"}},
    {".", new string[1] {"Key_Period"}},
    {";", new string[1] {"Key_Semicolon"}},
    {":", new string[1] {"Key_Colon"}},
    {"[", new string[1] {"Key_LBracket"}},
    {"]", new string[1] {"Key_RBracket"}},
    {"@", new string[1] {"Key_At"}},
    {"/", new string[1] {"Key_Slash"}},
    {"!", new string[2] {"Key_1", "Key_RShift"}},
    {"?", new string[2] {"Key_Slash", "Key_LShift"}},
    {"\"", new string[2] {"Key_2", "Key_RShift"}},
    {"#", new string[2] {"Key_3", "Key_RShift"}},
    {"$", new string[2] {"Key_4", "Key_RShift"}},
    {"%", new string[2] {"Key_5", "Key_RShift"}},
    {"&", new string[2] {"Key_6", "Key_LShift"}},
    {"\'", new string[2] {"Key_7", "Key_LShift"}},
    {"(", new string[2] {"Key_8", "Key_LShift"}},
    {")", new string[2] {"Key_9", "Key_LShift"}},
    {"=", new string[2] {"Key_Hyphen", "Key_LShift"}},
    {"~", new string[2] {"Key_Caret", "Key_LShift"}},
    {"|", new string[2] {"Key_Yen", "Key_LShift"}},
    {"`", new string[2] {"Key_At", "Key_LShift"}},
    {"{", new string[2] {"Key_LBracket", "Key_LShift"}},
    {"}", new string[2] {"Key_RBracket", "Key_LShift"}},
    {"+", new string[2] {"Key_Semicolon", "Key_LShift"}},
    {"*", new string[2] {"Key_Colon", "Key_LShift"}},
    {"<", new string[2] {"Key_Comma", "Key_LShift"}},
    {">", new string[2] {"Key_Period", "Key_LShift"}},
    {"_", new string[2] {"Key_BackSlash", "Key_LShift"}},
    {"ぬ", new string[1]{"Key_1"}},
    {"た", new string[1]{"Key_Q"}},
    {"ち", new string[1]{"Key_A"}},
    {"つ", new string[1]{"Key_Z"}},
    {"っ", new string[2]{"Key_Z", "Key_RShift"}},
    {"ふ", new string[1]{"Key_2"}},
    {"て", new string[1]{"Key_W"}},
    {"と", new string[1]{"Key_S"}},
    {"さ", new string[1]{"Key_X"}},
    {"あ", new string[1]{"Key_3"}},
    {"ぁ", new string[2]{"Key_3", "Key_RShift"}},
    {"い", new string[1]{"Key_E"}},
    {"ぃ", new string[2]{"Key_E", "Key_RShift"}},
    {"し", new string[1]{"Key_D"}},
    {"そ", new string[1]{"Key_C"}},
    {"う", new string[1]{"Key_4"}},
    {"ぅ", new string[2]{"Key_4", "Key_RShift"}},
    {"す", new string[1]{"Key_R"}},
    {"は", new string[1]{"Key_F"}},
    {"ひ", new string[1]{"Key_V"}},
    {"え", new string[1]{"Key_5"}},
    {"ぇ", new string[2]{"Key_5", "Key_RShift"}},
    {"か", new string[1]{"Key_T"}},
    {"き", new string[1]{"Key_G"}},
    {"こ", new string[1]{"Key_B"}},
    {"お", new string[1]{"Key_6"}},
    {"ぉ", new string[2]{"Key_6", "Key_LShift"}},
    {"ん", new string[1]{"Key_Y"}},
    {"く", new string[1]{"Key_H"}},
    {"み", new string[1]{"Key_N"}},
    {"や", new string[1]{"Key_7"}},
    {"ゃ", new string[2]{"Key_7", "Key_LShift"}},
    {"な", new string[1]{"Key_U"}},
    {"ま", new string[1]{"Key_J"}},
    {"も", new string[1]{"Key_M"}},
    {"ゆ", new string[1]{"Key_8"}},
    {"ゅ", new string[2]{"Key_8", "Key_LShift"}},
    {"に", new string[1]{"Key_I"}},
    {"の", new string[1]{"Key_K"}},
    {"ね", new string[1]{"Key_Comma"}},
    {"、", new string[2]{"Key_Comma", "Key_LShift"}},
    {"よ", new string[1]{"Key_9"}},
    {"ょ", new string[2]{"Key_9", "Key_LShift"}},
    {"ら", new string[1]{"Key_O"}},
    {"り", new string[1]{"Key_L"}},
    {"る", new string[1]{"Key_Period"}},
    {"。", new string[2]{"Key_Period", "Key_LShift"}},
    {"わ", new string[1]{"Key_0"}},
    {"を", new string[2]{"Key_0", "Key_LShift"}},
    {"せ", new string[1]{"Key_P"}},
    {"れ", new string[1]{"Key_Semicolon"}},
    {"め", new string[1]{"Key_Slash"}},
    {"ほ", new string[1]{"Key_Hyphen"}},
    {"゛", new string[1]{"Key_At"}},
    {"け", new string[1]{"Key_Colon"}},
    {"ろ", new string[1]{"Key_BackSlash"}},
    {"へ", new string[1]{"Key_Caret"}},
    {"゜", new string[1]{"Key_LBracket"}},
    {"む", new string[1]{"Key_RBracket"}},
    {"ー", new string[1]{"Key_Yen"}},
    {"　", new string[1]{"Key_Space"}}
  };

  // US 配列での文字とキーのマッピング
  private static readonly Dictionary<string, string[]> keyMappingUS = new Dictionary<string, string[]>() {
    {"0", new string[1] {"Key_0"}},
    {"1", new string[1] {"Key_1"}},
    {"2", new string[1] {"Key_2"}},
    {"3", new string[1] {"Key_3"}},
    {"4", new string[1] {"Key_4"}},
    {"5", new string[1] {"Key_5"}},
    {"6", new string[1] {"Key_6"}},
    {"7", new string[1] {"Key_7"}},
    {"8", new string[1] {"Key_8"}},
    {"9", new string[1] {"Key_9"}},
    {"a", new string[1] {"Key_A"}},
    {"b", new string[1] {"Key_B"}},
    {"c", new string[1] {"Key_C"}},
    {"d", new string[1] {"Key_D"}},
    {"e", new string[1] {"Key_E"}},
    {"f", new string[1] {"Key_F"}},
    {"g", new string[1] {"Key_G"}},
    {"h", new string[1] {"Key_H"}},
    {"i", new string[1] {"Key_I"}},
    {"j", new string[1] {"Key_J"}},
    {"k", new string[1] {"Key_K"}},
    {"l", new string[1] {"Key_L"}},
    {"m", new string[1] {"Key_M"}},
    {"n", new string[1] {"Key_N"}},
    {"o", new string[1] {"Key_O"}},
    {"p", new string[1] {"Key_P"}},
    {"q", new string[1] {"Key_Q"}},
    {"r", new string[1] {"Key_R"}},
    {"s", new string[1] {"Key_S"}},
    {"t", new string[1] {"Key_T"}},
    {"u", new string[1] {"Key_U"}},
    {"v", new string[1] {"Key_V"}},
    {"w", new string[1] {"Key_W"}},
    {"x", new string[1] {"Key_X"}},
    {"y", new string[1] {"Key_Y"}},
    {"z", new string[1] {"Key_Z"}},
    {"A", new string[2] {"Key_A", "Key_RShift"}},
    {"B", new string[2] {"Key_B", "Key_RShift"}},
    {"C", new string[2] {"Key_C", "Key_RShift"}},
    {"D", new string[2] {"Key_D", "Key_RShift"}},
    {"E", new string[2] {"Key_E", "Key_RShift"}},
    {"F", new string[2] {"Key_F", "Key_RShift"}},
    {"G", new string[2] {"Key_G", "Key_RShift"}},
    {"H", new string[2] {"Key_H", "Key_LShift"}},
    {"I", new string[2] {"Key_I", "Key_LShift"}},
    {"J", new string[2] {"Key_J", "Key_LShift"}},
    {"K", new string[2] {"Key_K", "Key_LShift"}},
    {"L", new string[2] {"Key_L", "Key_LShift"}},
    {"M", new string[2] {"Key_M", "Key_LShift"}},
    {"N", new string[2] {"Key_N", "Key_LShift"}},
    {"O", new string[2] {"Key_O", "Key_LShift"}},
    {"P", new string[2] {"Key_P", "Key_LShift"}},
    {"Q", new string[2] {"Key_Q", "Key_RShift"}},
    {"R", new string[2] {"Key_R", "Key_RShift"}},
    {"S", new string[2] {"Key_S", "Key_RShift"}},
    {"T", new string[2] {"Key_T", "Key_RShift"}},
    {"U", new string[2] {"Key_U", "Key_LShift"}},
    {"V", new string[2] {"Key_V", "Key_RShift"}},
    {"W", new string[2] {"Key_W", "Key_RShift"}},
    {"X", new string[2] {"Key_X", "Key_RShift"}},
    {"Y", new string[2] {"Key_Y", "Key_LShift"}},
    {"Z", new string[2] {"Key_Z", "Key_RShift"}},
    {" ", new string[1] {"Key_Space"}},
    {"-", new string[1] {"Key_Hyphen"}},
    {",", new string[1] {"Key_Comma"}},
    {".", new string[1] {"Key_Period"}},
    {";", new string[1] {"Key_Semicolon"}},
    {":", new string[2] {"Key_Semicolon", "Key_LShift"}},
    {"[", new string[1] {"Key_At"}},
    {"]", new string[1] {"Key_LBracket"}},
    {"@", new string[2] {"Key_2", "Key_RShift"}},
    {"/", new string[1] {"Key_Slash"}},
    {"!", new string[2] {"Key_1", "Key_RShift"}},
    {"?", new string[2] {"Key_Slash", "Key_LShift"}},
    {"\"", new string[2] {"Key_Colon", "Key_LShift"}},
    {"#", new string[2] {"Key_3", "Key_RShift"}},
    {"$", new string[2] {"Key_4", "Key_RShift"}},
    {"%", new string[2] {"Key_5", "Key_RShift"}},
    {"&", new string[2] {"Key_7", "Key_LShift"}},
    {"\'", new string[1] {"Key_Colon"}},
    {"(", new string[2] {"Key_9", "Key_LShift"}},
    {")", new string[2] {"Key_0", "Key_LShift"}},
    {"=", new string[1] {"Key_Caret"}},
    {"{", new string[2] {"Key_At", "Key_LShift"}},
    {"}", new string[2] {"Key_LBracket", "Key_LShift"}},
    {"+", new string[2] {"Key_Semicolon", "Key_LShift"}},
    {"*", new string[2] {"Key_8", "Key_LShift"}},
    {"<", new string[2] {"Key_Comma", "Key_LShift"}},
    {">", new string[2] {"Key_Period", "Key_LShift"}},
    {"_", new string[2] {"Key_Hyphen", "Key_LShift"}},
    {"　", new string[1]{"Key_Space"}}
  };

  // キーとフィンガリングのマッピング
  private static Dictionary<string, (int, char)> keyFingering = new Dictionary<string, (int, char)>() {
    {"Key_1", (5, 'L')},
    {"Key_Q", (5, 'L')},
    {"Key_A", (5, 'L')},
    {"Key_Z", (5, 'L')},
    {"Key_2", (4, 'L')},
    {"Key_W", (4, 'L')},
    {"Key_S", (4, 'L')},
    {"Key_X", (4, 'L')},
    {"Key_3", (3, 'L')},
    {"Key_E", (3, 'L')},
    {"Key_D", (3, 'L')},
    {"Key_C", (3, 'L')},
    {"Key_4", (2, 'L')},
    {"Key_R", (2, 'L')},
    {"Key_F", (2, 'L')},
    {"Key_V", (2, 'L')},
    {"Key_5", (2, 'L')},
    {"Key_T", (2, 'L')},
    {"Key_G", (2, 'L')},
    {"Key_B", (2, 'L')},
    {"Key_6", (2, 'R')},
    {"Key_Y", (2, 'R')},
    {"Key_H", (2, 'R')},
    {"Key_N", (2, 'R')},
    {"Key_7", (2, 'R')},
    {"Key_U", (2, 'R')},
    {"Key_J", (2, 'R')},
    {"Key_M", (2, 'R')},
    {"Key_8", (3, 'R')},
    {"Key_I", (3, 'R')},
    {"Key_K", (3, 'R')},
    {"Key_Comma", (3, 'R')},
    {"Key_9", (4, 'R')},
    {"Key_O", (4, 'R')},
    {"Key_L", (4, 'R')},
    {"Key_Period", (4, 'R')},
    {"Key_0", (5, 'R')},
    {"Key_P", (5, 'R')},
    {"Key_Semicolon", (5, 'R')},
    {"Key_Slash", (5, 'R')},
    {"Key_Hyphen", (5, 'R')},
    {"Key_At", (5, 'R')},
    {"Key_Colon", (5, 'R')},
    {"Key_Caret", (5, 'R')},
    {"Key_Yen", (5, 'R')},
    {"Key_LBracket", (5, 'R')},
    {"Key_RBracket", (5, 'R')},
    {"Key_BackSlash", (5, 'R')},
    {"Key_Space", (1, 'B')},
    {"Key_RShift", (5, 'R')},
    {"Key_LShift", (5, 'L')}
  };

  // JIS かなでのキートップ名
  private static readonly Dictionary<string, string> jisKanaKeyNameMap = new Dictionary<string, string>() {
    {"Key_1", "ぬ"},
    {"Key_Q", "た"},
    {"Key_A", "ち"},
    {"Key_Z", "つ"},
    {"Key_2", "ふ"},
    {"Key_W", "て"},
    {"Key_S", "と"},
    {"Key_X", "さ"},
    {"Key_3", "あ"},
    {"Key_E", "い"},
    {"Key_D", "し"},
    {"Key_C", "そ"},
    {"Key_4", "う"},
    {"Key_R", "す"},
    {"Key_F", "は"},
    {"Key_V", "ひ"},
    {"Key_5", "え"},
    {"Key_T", "か"},
    {"Key_G", "き"},
    {"Key_B", "こ"},
    {"Key_6", "お"},
    {"Key_Y", "ん"},
    {"Key_H", "く"},
    {"Key_N", "み"},
    {"Key_7", "や"},
    {"Key_U", "な"},
    {"Key_J", "ま"},
    {"Key_M", "も"},
    {"Key_8", "ゆ"},
    {"Key_I", "に"},
    {"Key_K", "の"},
    {"Key_Comma", "ね"},
    {"Key_9", "よ"},
    {"Key_O", "ら"},
    {"Key_L", "り"},
    {"Key_Period", "る"},
    {"Key_0", "わ"},
    {"Key_P", "せ"},
    {"Key_Semicolon", "れ"},
    {"Key_Slash", "め"},
    {"Key_Hyphen", "ほ"},
    {"Key_At", "゛"},
    {"Key_Colon", "け"},
    {"Key_BackSlash", "ろ"},
    {"Key_Caret", "へ"},
    {"Key_LBracket", "゜"},
    {"Key_RBracket", "む"},
    {"Key_Yen", "ー"},
    {"Key_Space", ""},
    {"Key_RShift", "Shift"},
    {"Key_LShift", "Shift"}
  };

  // US 配列のキートップ名
  private static readonly Dictionary<string, string> USArrayKeyNameMap = new Dictionary<string, string>() {
    {"Key_1", "1"},
    {"Key_Q", "Q"},
    {"Key_A", "A"},
    {"Key_Z", "Z"},
    {"Key_2", "2"},
    {"Key_W", "W"},
    {"Key_S", "S"},
    {"Key_X", "X"},
    {"Key_3", "3"},
    {"Key_E", "E"},
    {"Key_D", "D"},
    {"Key_C", "C"},
    {"Key_4", "4"},
    {"Key_R", "R"},
    {"Key_F", "F"},
    {"Key_V", "V"},
    {"Key_5", "5"},
    {"Key_T", "T"},
    {"Key_G", "G"},
    {"Key_B", "B"},
    {"Key_6", "6"},
    {"Key_Y", "Y"},
    {"Key_H", "H"},
    {"Key_N", "N"},
    {"Key_7", "7"},
    {"Key_U", "U"},
    {"Key_J", "J"},
    {"Key_M", "M"},
    {"Key_8", "8"},
    {"Key_I", "I"},
    {"Key_K", "K"},
    {"Key_Comma", ","},
    {"Key_9", "9"},
    {"Key_O", "O"},
    {"Key_L", "L"},
    {"Key_Period", "."},
    {"Key_0", "0"},
    {"Key_P", "P"},
    {"Key_Semicolon", ";"},
    {"Key_Slash", "/"},
    {"Key_Hyphen", "-"},
    {"Key_At", "["},
    {"Key_Colon", "'"},
    {"Key_BackSlash", ""},
    {"Key_Caret", "="},
    {"Key_LBracket", "]"},
    {"Key_RBracket", ""},
    {"Key_Yen", ""},
    {"Key_Space", ""},
    {"Key_RShift", "Shift"},
    {"Key_LShift", "Shift"}
  };

  // JIS 配列のキートップ名
  private static readonly Dictionary<string, string> JISArrayKeyNameMap = new Dictionary<string, string>() {
    {"Key_1", "1"},
    {"Key_Q", "Q"},
    {"Key_A", "A"},
    {"Key_Z", "Z"},
    {"Key_2", "2"},
    {"Key_W", "W"},
    {"Key_S", "S"},
    {"Key_X", "X"},
    {"Key_3", "3"},
    {"Key_E", "E"},
    {"Key_D", "D"},
    {"Key_C", "C"},
    {"Key_4", "4"},
    {"Key_R", "R"},
    {"Key_F", "F"},
    {"Key_V", "V"},
    {"Key_5", "5"},
    {"Key_T", "T"},
    {"Key_G", "G"},
    {"Key_B", "B"},
    {"Key_6", "6"},
    {"Key_Y", "Y"},
    {"Key_H", "H"},
    {"Key_N", "N"},
    {"Key_7", "7"},
    {"Key_U", "U"},
    {"Key_J", "J"},
    {"Key_M", "M"},
    {"Key_8", "8"},
    {"Key_I", "I"},
    {"Key_K", "K"},
    {"Key_Comma", ","},
    {"Key_9", "9"},
    {"Key_O", "O"},
    {"Key_L", "L"},
    {"Key_Period", "."},
    {"Key_0", "0"},
    {"Key_P", "P"},
    {"Key_Semicolon", ";"},
    {"Key_Slash", "/"},
    {"Key_Hyphen", "-"},
    {"Key_At", "@"},
    {"Key_Colon", ":"},
    {"Key_BackSlash", "_"},
    {"Key_Caret", "^"},
    {"Key_LBracket", "["},
    {"Key_RBracket", "]"},
    {"Key_Yen", ""},
    {"Key_Space", ""},
    {"Key_RShift", "Shift"},
    {"Key_LShift", "Shift"}
  };

  // キーの色
    private static float darkenFactor = 0.8f;

    private static Color colorGray = new Color(180f / 255f, 180f / 255f, 180f / 255f, 1);
    private static Color colorWhite = new Color(1, 1, 1, 1);
    private static Color colorBlack = new Color(0, 0, 0, 1);
    private static Color colorBlackFill = new Color(24f / 255f * darkenFactor, 24f / 255f * darkenFactor, 24f / 255f * darkenFactor, 1);
    private static Color colorPink = new Color(1 * darkenFactor, 40f / 255f * darkenFactor, 70f / 255f * darkenFactor, 1);
    private static Color colorLightPink = new Color(1 * darkenFactor, 194f / 255f * darkenFactor, 217f / 255f * darkenFactor, 1);
    private static Color colorOrange = new Color(251f / 255f * darkenFactor, 83f / 255f * darkenFactor, 30f / 255f * darkenFactor, 1);
    private static Color colorLightOrange = new Color(1 * darkenFactor, 220f / 255f * darkenFactor, 160f / 255f * darkenFactor, 1);
    private static Color colorGreen = new Color(49f / 255f * darkenFactor, 83f / 255f * darkenFactor, 30f / 255f * darkenFactor, 1);
    private static Color colorLightGreen = new Color(180f / 255f * darkenFactor, 1 * darkenFactor, 190f / 255f * darkenFactor, 1);
    private static Color colorBlue = new Color(28f / 255f * darkenFactor, 95f / 255f * darkenFactor, 166f / 255f * darkenFactor, 1);
    private static Color colorLightBlue = new Color(141f / 255f * darkenFactor, 240f / 255f * darkenFactor, 1 * darkenFactor, 1);
    private static Color colorViolet = new Color(140f / 255f * darkenFactor, 64f / 255f * darkenFactor, 1 * darkenFactor, 1);
    private static Color colorLightViolet = new Color(234f / 255f * darkenFactor, 198f / 255f * darkenFactor, 1 * darkenFactor, 1);



    /// <summary>
    /// 初期化処理
    /// </summary>
    void Awake()
    {
        rightHandAnim = rHand.GetComponent<Animator>(); // rightHandのアニメーターを取得
        leftHandAnim = lHand.GetComponent<Animator>(); // leftHandのアニメーターを取得

        GetAllKeys(ConfigScript.InputMode, ConfigScript.InputArray);
        SetAllKeyColorWhite();
    }

  /// <summary>
  /// キーのオブジェクトを取得する
  /// </summary>
  public void GetAllKeys(int inputType, int arrayType)
  {
    AKKeys = new Dictionary<string, GameObject>();
    for (int i = 0; i < AKParent.transform.childCount; ++i)
    {
      var keyboardRows = AKParent.transform.GetChild(i);
      for (int j = 0; j < keyboardRows.transform.childCount; ++j)
      {
        var obj = keyboardRows.transform.GetChild(j).gameObject;
        var keyName = obj.name;
        // JIS かなのプリセット
        if (inputType == (int)ConfigScript.InputType.jisKana && jisKanaKeyNameMap.ContainsKey(keyName))
        {
          var keyTextObj = obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
          var kanaText = jisKanaKeyNameMap[keyName];
          keyTextObj.text = kanaText;
        }
        // QwertyJP のプリセット
        else if (arrayType == (int)ConfigScript.KeyArrayType.japanese && JISArrayKeyNameMap.ContainsKey(keyName))
        {
          var keyTextObj = obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
          var keyText = JISArrayKeyNameMap[keyName];
          keyTextObj.text = keyText;
        }
        // Qwerty US 配列プリセット
        else if (arrayType == (int)ConfigScript.KeyArrayType.us && USArrayKeyNameMap.ContainsKey(keyName))
        {
          var keyTextObj = obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
          var keyText = USArrayKeyNameMap[keyName];
          keyTextObj.text = keyText;
        }
        AKKeys.Add(keyName, obj);
      }
    }
  }

    /// <summary>
    /// 指定したキーにエフェクト表示
    /// <param name="keyName">キー名</param>
    /// </summary>
    public void pushKeyAction(string keyStr)
    {
        if (keyMappingJIS.TryGetValue(keyStr, out string[] value))
        {
            GameObject keyObject = AKKeys[value[0]];
            ParticleSystem particleSystem = keyObject.GetComponentInChildren<ParticleSystem>();
            particleSystem.Play();
        }
        else
        {
            Console.WriteLine("Key not found");
        }
    }

    /// <summary>
    /// 指定したキーの色を白に設定する
    /// <param name="keyName">キー名</param>
    /// </summary>
    private void SetKeyColorWhite(string keyName)
    {
        var shape = AKKeys[keyName].GetComponent<Shape>();
        //        shape.settings.outlineColor = colorGray;
        //        shape.settings.fillColor = colorWhite;
        shape.settings.outlineColor = colorBlack;
        shape.settings.fillColor = colorBlackFill;
    }

    /// <summary>
    /// 指定したキーの色を変更する
    /// <param name="keyName">キー名</param>
    /// </summary>
    private void SetKeyColorHighlight(string keyName)
  {
    var shape = AKKeys[keyName].GetComponent<Shape>();
    switch (keyFingering[keyName].Item1)
    {
      case 1:
        shape.settings.outlineColor = colorViolet;
        shape.settings.fillColor = colorLightViolet;
        break;
      case 2:
        shape.settings.outlineColor = colorBlue;
        shape.settings.fillColor = colorLightBlue;
        break;
      case 3:
        shape.settings.outlineColor = colorGreen;
        shape.settings.fillColor = colorLightGreen;
        break;
      case 4:
        shape.settings.outlineColor = colorOrange;
        shape.settings.fillColor = colorLightOrange;
        break;
      case 5:
        shape.settings.outlineColor = colorPink;
        shape.settings.fillColor = colorLightPink;
        break;
    }
  }

  /// <summary>
  /// 全てのキーの色を白にする
  /// </summary>
  public void SetAllKeyColorWhite()
  {
    foreach (var kvp in keyMappingJIS)
    {
      var keyList = new List<string>(kvp.Value);
      foreach (var keyName in keyList)
      {
        SetKeyColorWhite(keyName);
      }
    }
  }

    /// <summary>
    /// 手のアニメーション操作
    /// </summary>
    private void handAnimation(string word)
    {
        leftHandAnim.ResetTrigger("1");
        leftHandAnim.ResetTrigger("2");
        leftHandAnim.ResetTrigger("3");
        leftHandAnim.ResetTrigger("4");
        leftHandAnim.ResetTrigger("5");
        leftHandAnim.ResetTrigger("q");
        leftHandAnim.ResetTrigger("w");
        leftHandAnim.ResetTrigger("e");
        leftHandAnim.ResetTrigger("r");
        leftHandAnim.ResetTrigger("t");
        leftHandAnim.ResetTrigger("a");
        leftHandAnim.ResetTrigger("s");
        leftHandAnim.ResetTrigger("d");
        leftHandAnim.ResetTrigger("f");
        leftHandAnim.ResetTrigger("g");
        leftHandAnim.ResetTrigger("z");
        leftHandAnim.ResetTrigger("x");
        leftHandAnim.ResetTrigger("c");
        leftHandAnim.ResetTrigger("v");
        leftHandAnim.ResetTrigger("b");
        leftHandAnim.ResetTrigger("home");

        rightHandAnim.ResetTrigger("6");
        rightHandAnim.ResetTrigger("7");
        rightHandAnim.ResetTrigger("8");
        rightHandAnim.ResetTrigger("9");
        rightHandAnim.ResetTrigger("0");
        rightHandAnim.ResetTrigger("-");
        rightHandAnim.ResetTrigger("^");
        rightHandAnim.ResetTrigger("yen");
        rightHandAnim.ResetTrigger("y");
        rightHandAnim.ResetTrigger("u");
        rightHandAnim.ResetTrigger("i");
        rightHandAnim.ResetTrigger("o");
        rightHandAnim.ResetTrigger("p");
        rightHandAnim.ResetTrigger("at");
        rightHandAnim.ResetTrigger("[");
        rightHandAnim.ResetTrigger("h");
        rightHandAnim.ResetTrigger("j");
        rightHandAnim.ResetTrigger("k");
        rightHandAnim.ResetTrigger("l");
        rightHandAnim.ResetTrigger(";");
        rightHandAnim.ResetTrigger("colon");
        rightHandAnim.ResetTrigger("]");
        rightHandAnim.ResetTrigger("n");
        rightHandAnim.ResetTrigger("m");
        rightHandAnim.ResetTrigger("comma");
        rightHandAnim.ResetTrigger("dot");
        rightHandAnim.ResetTrigger("slash");
        rightHandAnim.ResetTrigger("_");
        rightHandAnim.ResetTrigger("home");

        switch (word)
        {
            case "1":
                leftHandAnim.SetTrigger("1");
                break;
            case "2":
                leftHandAnim.SetTrigger("2");
                break;
            case "3":
                leftHandAnim.SetTrigger("3");
                break;
            case "4":
                leftHandAnim.SetTrigger("4");
                break;
            case "5":
                leftHandAnim.SetTrigger("5");
                break;
            case "q":
                leftHandAnim.SetTrigger("q");
                break;
            case "w":
                leftHandAnim.SetTrigger("w");
                break;
            case "e":
                leftHandAnim.SetTrigger("e");
                break;
            case "r":
                leftHandAnim.SetTrigger("r");
                break;
            case "t":
                leftHandAnim.SetTrigger("t");
                break;
            case "a":
                leftHandAnim.SetTrigger("a");
                break;
            case "s":
                leftHandAnim.SetTrigger("s");
                break;
            case "d":
                leftHandAnim.SetTrigger("d");
                break;
            case "f":
                leftHandAnim.SetTrigger("f");
                break;
            case "g":
                leftHandAnim.SetTrigger("g");
                break;
            case "z":
                leftHandAnim.SetTrigger("z");
                break;
            case "x":
                leftHandAnim.SetTrigger("x");
                break;
            case "c":
                leftHandAnim.SetTrigger("c");
                break;
            case "v":
                leftHandAnim.SetTrigger("v");
                break;
            case "b":
                leftHandAnim.SetTrigger("b");
                break;

            default:
                leftHandAnim.SetTrigger("home");
                break;
        }

        switch (word)
        {
            case "6":
                rightHandAnim.SetTrigger("6");
                break;
            case "7":
                rightHandAnim.SetTrigger("7");
                break;
            case "8":
                rightHandAnim.SetTrigger("8");
                break;
            case "9":
                rightHandAnim.SetTrigger("9");
                break;
            case "0":
                rightHandAnim.SetTrigger("0");
                break;
            case "-":
                rightHandAnim.SetTrigger("-");
                break;
            case "^":
                rightHandAnim.SetTrigger("^");
                break;
            case "\\":
                rightHandAnim.SetTrigger("yen");
                break;
            case "y":
                rightHandAnim.SetTrigger("y");
                break;
            case "u":
                rightHandAnim.SetTrigger("u");
                break;
            case "i":
                rightHandAnim.SetTrigger("i");
                break;
            case "o":
                rightHandAnim.SetTrigger("o");
                break;
            case "p":
                rightHandAnim.SetTrigger("p");
                break;
            case "@":
                rightHandAnim.SetTrigger("at");
                break;
            case "[":
                rightHandAnim.SetTrigger("[");
                break;
            case "h":
                rightHandAnim.SetTrigger("h");
                break;
            case "j":
                rightHandAnim.SetTrigger("j");
                break;
            case "k":
                rightHandAnim.SetTrigger("k");
                break;
            case "l":
                rightHandAnim.SetTrigger("l");
                break;
            case ";":
                rightHandAnim.SetTrigger(";");
                break;
            case ":":
                rightHandAnim.SetTrigger("colon");
                break;
            case "]":
                rightHandAnim.SetTrigger("]");
                break;
            case "n":
                rightHandAnim.SetTrigger("n");
                break;
            case "m":
                rightHandAnim.SetTrigger("m");
                break;
            case ",":
                rightHandAnim.SetTrigger("comma");
                break;
            case ".":
                rightHandAnim.SetTrigger("dot");
                break;
            case "/":
                rightHandAnim.SetTrigger("slash");
                break;
            case "_":
                rightHandAnim.SetTrigger("_");
                break;

            default:
                rightHandAnim.SetTrigger("home");
                break;
        }
    }
    /// <summary>
    /// 次に打つべき文字と指をハイライトする
    /// <param name="nextHighlightChar">次に打つ文字</param>
    /// </summary>
    public void SetNextHighlight(string nextStr)
    {
        handAnimation(nextStr);

        // 一度指、キーの色をリセットする
        SetAllKeyColorWhite();
        var keyList = new List<string>();

        if (ConfigScript.InputArray == (int)ConfigScript.KeyArrayType.japanese)
        {
            keyList = new List<string>(keyMappingJIS[nextStr]);
        }
        else if (ConfigScript.InputArray == (int)ConfigScript.KeyArrayType.us)
        {
            keyList = new List<string>(keyMappingUS[nextStr]);
        }
        foreach (var keyName in keyList)
        {
            SetKeyColorHighlight(keyName);
        }
    }

  /// <summary>
  /// 複数のキーと指をハイライトする
  /// </summary>
  /// <param name="keyList"></param>
  public void SetHighlights(IReadOnlyCollection<string> keyList)
  {
    foreach (var keyName in keyList)
    {
      SetKeyColorHighlight(keyName);
    }
  }
}

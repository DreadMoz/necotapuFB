// using System;
// using System.Collections.Generic;
// using Cysharp.Threading.Tasks;
// using Google.Apis.Services;
// using Google.Apis.Sheets.v4;
// using Google.Apis.Sheets.v4.Data;

// public static class GSheet
// {
//     private static readonly string[] Scopes = new[] { SheetsService.Scope.Spreadsheets, SheetsService.Scope.SpreadsheetsReadonly };
//     private static readonly string SheetName = "Sheet"; // シート名
//     private static string sheetId = "";         // シートID
//     private static int targetRowNumber = -1;    // 特定の行番号

//     public static async UniTask<IList<object>> GetRow()
//     {
//         var range = $"{SheetName}!A{targetRowNumber}:BJ{targetRowNumber}"; // 列の範囲をAからBJまでに変更
//         return await ReadRow(range);
//     }

//     public static async UniTask<bool> WriteRow(int startColumn, int endColumn, IList<object> rowData)
//     {
//         if (targetRowNumber == -1)
//         {
//             Console.WriteLine("Error: No target row specified.");
//             return false;
//         }

//         var range = $"{SheetName}!{ConvertIndexToColumn(startColumn+1)}{targetRowNumber}:{ConvertIndexToColumn(endColumn+1)}{targetRowNumber}";
//         return await WriteRow(range, rowData);
//     }

//     // 列のインデックスを文字列に変換するメソッド
//     private static string ConvertIndexToColumn(int index)
//     {
//         const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
//         index--;
//         string column = "";
//         while (index >= 0)
//         {
//             column = letters[index % 26] + column;
//             index = (index / 26) - 1;
//         }
//         return column;
//     }

//     // 指定されたメールアドレスを含む行の行番号を検索し、targetRowNumberを更新するメソッド
//     public static async UniTask<bool> FindRowNumber(string id, string emailToFind)
//     {
//         string[] Scopes = new[] { SheetsService.Scope.Spreadsheets, SheetsService.Scope.SpreadsheetsReadonly };
//         var credential = GoogleServiceAccount.GetCredential(Scopes);
//         if (credential == null)
//         {
//             return false;
//         }
//         sheetId = id;

//         var sheetService = new SheetsService(new BaseClientService.Initializer
//         {
//             HttpClientInitializer = credential
//         });

//         // メールアドレスを含む行を検索するためのクエリ
//         string query = $"select * where A='{emailToFind}'";

//         // クエリで指定された行のデータを取得
//         var request = sheetService.Spreadsheets.Values.BatchGet(sheetId);
//         request.Ranges = new List<string> { $"{SheetName}!A:A" };
//         request.MajorDimension = SpreadsheetsResource.ValuesResource.BatchGetRequest.MajorDimensionEnum.ROWS;
//         request.ValueRenderOption = SpreadsheetsResource.ValuesResource.BatchGetRequest.ValueRenderOptionEnum.FORMATTEDVALUE;
//         request.DateTimeRenderOption = SpreadsheetsResource.ValuesResource.BatchGetRequest.DateTimeRenderOptionEnum.FORMATTEDSTRING;

//         var response = await request.ExecuteAsync();
//         var values = response?.ValueRanges?[0]?.Values;

//         if (values != null)
//         {
//             for (int i = 0; i < values.Count; i++)
//             {
//                 var row = values[i];
//                 if (row != null && row.Count > 0 && row[0].ToString() == emailToFind)
//                 {
//                     targetRowNumber = i + 1; // 1-indexed row number
//                     return true;
//                 }
//             }
//         }

//         return false; // メールアドレスが見つからなかった場合
//     }

//     private static async UniTask<IList<object>> ReadRow(string range)
//     {
//         var credential = GoogleServiceAccount.GetCredential(Scopes);
//         if (credential == null)
//         {
//             return null;
//         }

//         var sheetService = new SheetsService(new BaseClientService.Initializer
//         {
//             HttpClientInitializer = credential
//         });

//         var result = await sheetService.Spreadsheets.Values.Get(sheetId, range).ExecuteAsync();
//         return result.Values?[0];
//     }

//     private static async UniTask<bool> WriteRow(string range, IList<object> rowData)
//     {
//         var credential = GoogleServiceAccount.GetCredential(Scopes);
//         if (credential == null)
//         {
//             return false;
//         }

//         var sheetService = new SheetsService(new BaseClientService.Initializer
//         {
//             HttpClientInitializer = credential
//         });

//         var valueRange = new ValueRange { Values = new List<IList<object>> { rowData } };
//         var updateRequest = sheetService.Spreadsheets.Values.Update(valueRange, sheetId, range);
//         updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;

//         var updateResponse = await updateRequest.ExecuteAsync();
//         return updateResponse != null;
//     }
// }

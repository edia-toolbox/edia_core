using System;
using System.Threading;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;

namespace Edia.Data {

    /// <summary>
    /// Represents a table of data: named columns, each holding a list of values. All columns are always the same length.
    /// </summary>
    public class DataTable {

        public string[] Headers { get { return dict.Keys.ToArray(); } }
        private Dictionary<string, List<object>> dict;

        public DataTable(int capacity, params string[] columnNames) {
            dict = new Dictionary<string, List<object>>();
            foreach (string colName in columnNames)
                dict.Add(colName, new List<object>(capacity));
        }

        public DataTable(params string[] columnNames) {
            dict = new Dictionary<string, List<object>>();
            foreach (string colName in columnNames)
                dict.Add(colName, new List<object>());
        }

        public static DataTable FromCSV(string[] csvLines) {
            string[] headers = csvLines[0].Split(',');
            var table = new DataTable(csvLines.Length - 1, headers);

            for (int i = 1; i < csvLines.Length; i++) {
                string[] values = csvLines[i].Split(',');
                if (i == csvLines.Length - 1 && values.Length == 1 && values[0].Trim() == string.Empty) break;
                if (values.Length != headers.Length) throw new Exception($"CSV line {i} has {values.Length} columns, but expected {headers.Length}");

                var row = new DataRow();
                for (int j = 0; j < values.Length; j++)
                    row.Add((headers[j], values[j].Trim('\"')));
                table.AddCompleteRow(row);
            }

            return table;
        }

        public void AddCompleteRow(DataRow newRow) {
            if (newRow == null) throw new ArgumentNullException("newRow");

            bool sameKeys = dict.Keys.All(newRow.Select(item => item.columnName).Contains)
                            && (newRow.Count == dict.Keys.Count);

            if (!sameKeys) {
                throw new InvalidOperationException(
                    string.Format(
                        "The row does not contain values for the same columns as the columns in the table!\nTable: {0}\nRow: {1}",
                        string.Join(", ", Headers),
                        string.Join(", ", newRow.Headers)
                    )
                );
            }

            foreach (var item in newRow)
                dict[item.columnName].Add(item.value);
        }

        public int CountRows() {
            string[] keyArray = dict.Keys.ToArray();
            if (keyArray.Length == 0) return 0;
            return dict[keyArray[0]].Count();
        }

        public string[] GetCSVLines(CultureInfo culture = null, string decimalFormat = "0.######") {
            culture = culture ?? Thread.CurrentThread.CurrentCulture;
            string[] headers = Headers;
            string[] lines = new string[CountRows() + 1];
            lines[0] = string.Join(culture.TextInfo.ListSeparator, headers);
            for (int i = 1; i < lines.Length; i++) {
                lines[i] = string.Join(culture.TextInfo.ListSeparator,
                    headers.Select(h => FormatItem(dict[h][i - 1], culture, decimalFormat))
                );
            }
            return lines;
        }

        static string FormatItem(object item, CultureInfo culture, string decimalFormat = "0.######") {
            switch (item) {
                case sbyte sbyteNum:     return sbyteNum.ToString(culture);
                case byte byteNum:       return byteNum.ToString(culture);
                case short shortNum:     return shortNum.ToString(culture);
                case ushort ushortNum:   return ushortNum.ToString(culture);
                case int intNum:         return intNum.ToString(culture);
                case uint uintNum:       return uintNum.ToString(culture);
                case long longNum:       return longNum.ToString(culture);
                case ulong ulongNum:     return ulongNum.ToString(culture);
                case float floatNum:     return floatNum.ToString(decimalFormat, culture);
                case double doubleNum:   return doubleNum.ToString(decimalFormat, culture);
                case decimal decimalNum: return decimalNum.ToString(decimalFormat, culture);
                case null:               return "null";
                default:                 return item.ToString().Replace(culture.TextInfo.ListSeparator, "_");
            }
        }

        public Dictionary<string, List<object>> GetAsDictOfList() {
            Dictionary<string, List<object>> dictCopy = new Dictionary<string, List<object>>();
            foreach (var kvp in dict)
                dictCopy.Add(kvp.Key, new List<object>(kvp.Value));
            return dictCopy;
        }

        public List<Dictionary<string, object>> GetAsListOfDict() {
            int numRows = CountRows();
            List<Dictionary<string, object>> listCopy = new List<Dictionary<string, object>>(numRows);
            for (int i = 0; i < numRows; i++)
                listCopy.Add(Headers.ToDictionary(h => h, h => dict[h][i]));
            return listCopy;
        }
    }
}

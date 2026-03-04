using System.Collections.Generic;
using System.Linq;

namespace Edia.Data {

    /// <summary>
    /// Represents a single row of data: a list of named (columnName, value) tuples.
    /// </summary>
    public class DataRow : List<(string columnName, object value)> {

        public IEnumerable<string> Headers { get { return this.Select(kvp => kvp.columnName); } }
    }
}

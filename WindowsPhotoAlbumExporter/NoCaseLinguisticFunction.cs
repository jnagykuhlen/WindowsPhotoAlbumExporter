using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

using System.Data.SQLite;

namespace WindowsPhotoAlbumExporter
{
    [SQLiteFunction(FuncType = FunctionType.Collation, Name = "NoCaseLinguistic")]
    public class NoCaseLinguisticFunction : SQLiteFunction
    {
        public override int Compare(string param1, string param2)
        {
            return StringComparer.InvariantCultureIgnoreCase.Compare(param1, param2);
        }
    }
}

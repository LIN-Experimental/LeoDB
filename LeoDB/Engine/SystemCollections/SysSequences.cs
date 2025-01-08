using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using static LeoDB.Constants;

namespace LeoDB.Engine
{
    public partial class LeoEngine
    {
        private IEnumerable<BsonDocument> SysSequences()
        {
            var values = _sequences.ToArray();

            foreach(var value in values)
            {
                yield return new BsonDocument
                {
                    ["collection"] = value.Key,
                    ["value"] = value.Value
                };
            }
        }
    }
}
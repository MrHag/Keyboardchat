using System;
using System.Collections.Generic;
using System.Linq;

namespace Keyboardchat.SaveCollections
{
    public interface ISaveInterface<T3> where T3 : ISaveCollection
    {
        abstract private protected T3 SaveCollection { get; set; }
    }
}

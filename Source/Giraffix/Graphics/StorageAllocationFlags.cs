using System;

namespace Giraffix.Graphics;

[Flags]
public enum StorageAllocationFlags
{
    None = 0,
    Dynamic = 1,
    Client = 2,
    Mappable = 4
}
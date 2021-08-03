# KVCollection - A .NET Key-Value Collection Store


---

KVCollection is a small, fast and lightweight .NET Key-Value Collection. 

- Serverless
- Simple API
- 100% C# code for .NET 4.5 / NETStandard 1.3/2.0 in a single DLL (less than 17kb)
- Thread-safe
- ACID with full transaction support
- Data recovery after write failure (WAL log file)
- Indexed keys for fast search
- Open source and free for everyone - including commercial use
- `KV Explorer` - Nice UI for data access

## How to use KVCollection

A quick example for storing and searching documents:

```C#
// Create your POCO class
var kc =  new KeyValue.Collection();

// Get customer "test""
kc.Open("test");

// Insert new 100K items
for (int i = 1; i < 100000; i++)
    kc.Add("Key-" + i, "This is the content for Key-" + i);


// get value for "Key-50000"
var item = kc.Get("Key-50000");


// Update an item inside a collection
kc.Update("Key-50000", "new value");


// Delete an item
kc.Delete("Key-50000");


// Insert/Update an item inside a collection
kc.Update("Key-XXX", "value of XXX");

// Delete all items
kc.Truncate();

var count = kc.Count;


// Iteration all keys
foreach (var key in kc.GetKeys)
{
    ...
}
    

// Close then collection
kc.Close();
}
```


## Where to use?

- All desktop/web applications need to store key-value data
- Multi client application
- .Net / Asp.Net / .Net Core / Asp.Net Core


## Changelog

Change details for each release are documented in the [release notes](https://github.com/Rubic-Solutions/KVCollection/releases).


## License

[MIT](http://opensource.org/licenses/MIT)

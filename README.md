# PLON (Property-less object notation)

PLON provides an alternative to plain JSON by serializing the objects' definition (properties and types) only once in the form of metadata. Serialized objects will therefore be just plain arrays of their properties values.

The goal of PLON is to considerably decrease serialization and deserialization time.

PLON doesn't allow any whitespaces in its serialization.

Example: Imagine the following class named `Car`, having the properties: `Manufacturer (string)`, `Horsepower (int)`.

A Car object serialized using PLON will have three properties: `types` and `rootType` (the metadata) and `value` (the PLON-serialized root object value):

```json
{
	"types": [{
		"name": "Car",
		"properties": [{
			"name": "Manufacturer",
			"type": "string"
		}, {
			"name": "Horsepower",
			"type": "number"
		}]
	}],
	"rootType": "Car",
	"value": ["BMW", 250]
}
```

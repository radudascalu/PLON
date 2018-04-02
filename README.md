# PLON (Property-less object notation)

PLON provides an alternative to JSON that serializes only the values contained by your objects without any of their properties' name. In order for this to be deserializable, a PLON object also contains the medata of the data it contains.

The goal of PLON is to considerably decrease serialization and deserialization time.

PLON doesn't allow any whitespaces in it's serialization.

For example, imagine the following two classes:
- `Car`, having the properties: `Manufacturer (string)`, `Horsepower (int)`, `Passengers (Passenger[])`;
- `Passenger`, having the properties: `Name (string)`, `Age (int)`;

The metadata of a car object would be a JSON object with the following structure (whitespaces added for readability):

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
		}, {
			"name": "Passengers",
			"type": "Passenger[]"
		}]
	}, {
		"name": "Passenger",
		"properties": [{
			"name": "Name",
			"type": "string"
		}, {
			"name": "Age",
			"type": "number"
		}]
	}],
	"rootType": "Car"
}
```

A Car object would be serialized using PLON like this:

`{"BMW",250,[{"Bob",23},{"Alice",22}]}`

The final PLON object will have three properties: `types` and `rootType` (the metadata) and `value` (the PLON-serialized root object value):

`{"types":[{"name":"Car","properties":[{"name":"Manufacturer","type":"string"},{"name":"Horsepower","type":"number"},{"name":"Passengers","type":"Passenger[]"}]},{"name":"Passenger","properties":[{"name":"Name","type":"string"},{"name":"Age","type":"number"}]}],"rootType":"Car","value":{"BMW",250,[{"Bob",23},{"Alice",22}]}}`

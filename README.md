# PLON (Property-less object notation)

PLON provides an alternative to JSON that serializes only the values contained by your objects without any of their properties' name.

The trade-off is an additional web request needed to get the model metadata before the first request and one more everytime the model changes.

The goal of PLON is to considerably decrease serialization and deserialization time.

PLON is very similar to JSON, the only difference is that it doesn't contain any property names.
For example, imagine the following two classes:
- `Car`, having the properties: `Manufacturer (string)`, `Horsepower (int)`, `Passengers (Passenger[])`;
- `Passenger`, having the properties: `Name (string)`, `Age (int)`;

A Car object would be serialized using PLON like this:
`{"BMW",250,[{"Bob",23},{"Alice",22}]}`

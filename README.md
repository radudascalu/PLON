# PLON (Property-less object notation)

PLON provides an alternative to JSON that serializes only the values contained by your objects without any of their properties' name.

The trade-off is an additional web request needed to get the model metadata before the first request and one more everytime the model changes.

The goal of PLON is to considerably decrease serialization and deserialization time (especially in big objects like 1k+ object arrays), so that you can improve your API's performance when no other option is possible.

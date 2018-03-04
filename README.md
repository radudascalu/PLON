# PLON (Property-less object notation)

PLON provides an alternative to JSON that serializes only the values of your objects, without their property names.

The trade-off is an additional web request to get the model metadata before the first request and everytime the model changes.

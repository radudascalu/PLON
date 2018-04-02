module Plon.Serialization.Common

type PValue = 
    | PString of string
    | PNumber of decimal
    | PObject of PValue list
    | PArray of PValue list
    | PBoolean of bool
    | PNull
    

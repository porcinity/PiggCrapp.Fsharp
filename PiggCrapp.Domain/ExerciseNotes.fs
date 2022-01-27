module PiggCrapp.ExerciseNotes

type SeatHeight =
    | SeatHeight of int
    
type SeatPosition =
    | SeatPosition of int
    
type SeatSetting =
    | SeatHeight
    | SeatPosition
    | MultiSetting of SeatHeight * SeatPosition
    
type Manufacturer =
    | Hoist
    | Nautilus
    
type PlateLoaded = PlateLoaded 
    
type MachineType =
    | PlateLoaded
    | Selectorized of Manufacturer
    | CableStack
    
type ExerciseNotes =
    { SeatSetting : SeatSetting
      MachineType : MachineType }
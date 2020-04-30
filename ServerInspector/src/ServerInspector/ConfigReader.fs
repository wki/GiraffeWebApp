module ServerInspector.ConfigReader

open FSharp.Data

type ConfigFile = JsonProvider<"sample_config.json">

let config file = ConfigFile.Parse file

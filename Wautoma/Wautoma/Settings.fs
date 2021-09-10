module Wautoma.Settings

open System.IO
open System.Text
open Newtonsoft.Json

type WautomaSettings = Map<string, obj>


let getSetting settingName (defaultValue: 'a) (settings: WautomaSettings) : 'a =
    match settings.TryFind settingName with
    | Some value -> value :?> 'a
    | _ -> defaultValue


let getSettingInt
    settingName
    (defaultValue: int)
    (settings: WautomaSettings)
    : int =
    match settings.TryFind settingName with
    | Some value -> value :?> int64 |> int
    | _ -> defaultValue


let setSetting settingName value (settings: WautomaSettings) : WautomaSettings =
    settings.Add(settingName, value)

let saveSettings (filename: string) settings : unit =
    let json =
        JsonConvert.SerializeObject(settings, Formatting.Indented)

    use writer =
        new StreamWriter(filename, append = false, encoding = Encoding.UTF8)

    writer.Write json

let loadSettings filename : WautomaSettings =
    if File.Exists filename then
        use reader =
            new StreamReader(filename, Encoding.UTF8)

        let json = reader.ReadToEnd()

        JsonConvert.DeserializeObject(json, typeof<WautomaSettings>)
        :?> WautomaSettings
    else
        Map.empty

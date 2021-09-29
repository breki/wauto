module Wautoma.Settings

open System
open System.IO
open System.Text
open Newtonsoft.Json

type WautomaSettings = Map<string, Map<string, obj>>

let machineName = Environment.MachineName

let getSetting settingName (defaultValue: 'a) (settings: WautomaSettings) : 'a =
    match settings.TryFind machineName with
    | Some machineSettings ->
        match machineSettings.TryFind settingName with
        | Some value -> value :?> 'a
        | None -> defaultValue
    | None -> defaultValue

let getSettingInt
    settingName
    (defaultValue: int)
    (settings: WautomaSettings)
    : int =
    match settings.TryFind machineName with
    | Some machineSettings ->
        match machineSettings.TryFind settingName with
        | Some value -> value :?> int64 |> int
        | None -> defaultValue
    | None -> defaultValue

let setSetting settingName value (settings: WautomaSettings) : WautomaSettings =
    settings.Change(
        machineName,
        fun machineSettings ->
            match machineSettings with
            | None -> Map.empty.Add(settingName, value) |> Some
            | Some machineSettings ->
                machineSettings.Add(settingName, value) |> Some
    )

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

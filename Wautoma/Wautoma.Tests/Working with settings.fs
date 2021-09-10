module Tests.``Working with settings``

open Wautoma.Settings
open Xunit
open Swensen.Unquote
open System.IO


let deleteAnyExistingSettingsFile settingsFilename =
    if File.Exists settingsFilename then
        File.Delete settingsFilename

[<Fact>]
let ``returns empty settings if settings file does not exist`` () =
    let settingsFilename = "some-settings1.json"
    deleteAnyExistingSettingsFile settingsFilename

    let settings = loadSettings settingsFilename
    test <@ settings = Map.empty @>

[<Fact>]
let ``can store and retrieve settings`` () =
    let settingsFilename = "some-settings2.json"
    deleteAnyExistingSettingsFile settingsFilename

    loadSettings settingsFilename
    |> setSetting "width" 200
    |> setSetting "height" 100
    |> saveSettings settingsFilename

    let settings = loadSettings settingsFilename
    test <@ settings |> getSetting "width" 0L = 200L @>
    test <@ settings |> getSetting "height" 0L = 100L @>
    test <@ settings |> getSettingInt "width" 0 = 200 @>
    test <@ settings |> getSettingInt "height" 0 = 100 @>

module JsonConversion

open Karambolo.PO
open Argu
open System
open System.IO
open System.Text.Json.Nodes
open Shared

type JsonConversionCliArguments =
    | [<AltCommandLine("-v")>] Verbose
    | [<AltCommandLine("-o")>] Output of output: string
    | [<AltCommandLine("-s")>] Source of output: string
    | [<AltCommandLine("-l")>] Language of language: string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Verbose -> "Verbose output"
            | Source _ -> ".json file to translate"
            | Output _ -> ".po/.pot file where to save translation"
            | Language _ -> "Language of the JSON file"

let rec private populateNode (node: JsonNode) (key: string) (catalog: POCatalog) =
    match node with
    | :? JsonObject as obj ->
        for kvp in obj do
            let value = kvp.Value
            let newPrefix = if String.IsNullOrEmpty(key) then kvp.Key else key + "." + kvp.Key
            populateNode value newPrefix catalog
    | :? JsonValue as obj ->
        let entry = POSingularEntry(POKey(key))
        entry.[0] <- obj.ToString()
        catalog.Add(entry)
    | :? JsonArray as arr ->
        raise (NotImplementedException("JsonArray is not implemented"))
    | _ ->
        raise (NotImplementedException("Unknown JSON value is not implemented"))

let processCli argv =
    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let parser = ArgumentParser.Create<JsonConversionCliArguments>(programName = "robotranslator fromjson", errorHandler = errorHandler)
    let results = parser.ParseCommandLine (inputs=argv, ignoreUnrecognized = true)
    let sourceFileName = results.GetResult(JsonConversionCliArguments.Source)
    let root = JsonNode.Parse(File.ReadAllText(sourceFileName))
    let catalog = POCatalog()
    populateNode root "" catalog
    let outputFileName = results.GetResult(JsonConversionCliArguments.Output)
    if results.Contains(JsonConversionCliArguments.Language) then
        let language = results.GetResult(JsonConversionCliArguments.Language)
        catalog.Language <- language

    Directory.CreateDirectory(Path.GetDirectoryName(outputFileName)) |> ignore
    writePOCatalog outputFileName catalog
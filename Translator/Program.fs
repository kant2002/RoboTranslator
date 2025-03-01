open Karambolo.PO
open Argu
open System

type CliArguments =
    | [<AltCommandLine("-v")>] Verbose
    | [<AltCommandLine("-o")>] Output of output: string
    | [<AltCommandLine("-s")>] Source of output: string
    | [<AltCommandLine("-l")>] Language of language: string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Verbose -> "Verbose output"
            | Source _ -> ".po/.pot file to translate"
            | Output _ -> ".po file where to save translation"
            | Language _ -> "Language to which translate file"

[<EntryPoint>]
let main argv =
    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let parser = ArgumentParser.Create<CliArguments>(programName = "robotranslator", errorHandler = errorHandler)
    let results = parser.ParseCommandLine (inputs=argv, ignoreUnrecognized = true)

    let sourceFileName = results.GetResult(CliArguments.Source)
    let outputFileName = results.GetResult(CliArguments.Output)


    let parser = POParser();
    do
        use stream = new System.IO.FileStream(sourceFileName, System.IO.FileMode.Open)
        let result = parser.Parse(stream);
        if result.Success then
            let catalog = result.Catalog
            printfn "Parsed %d entries" catalog.Count
            let targetLanguage = results.GetResult(CliArguments.Language)
            printfn "Source language %s. Translating to %s" catalog.Language targetLanguage
        else
            let diagnostics = result.Diagnostics
            for diagnostic in diagnostics do
                printfn "Error: %s, Severity: %O. %A" diagnostic.Code diagnostic.Severity diagnostic.Args
    0

open Karambolo.PO
open Argu
open System
open Google.Cloud.Translation.V2

type CliArguments =
    | [<AltCommandLine("-v")>] Verbose
    | [<AltCommandLine("-o")>] Output of output: string
    | [<AltCommandLine("-s")>] Source of output: string
    | [<AltCommandLine("-l")>] Language of language: string
    | [<AltCommandLine("-k")>] ApiKey of key: string
    | First of count: int

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Verbose -> "Verbose output"
            | Source _ -> ".po/.pot file to translate"
            | Output _ -> ".po file where to save translation"
            | Language _ -> "Language to which translate file"
            | ApiKey _ -> "API key for Google Translate"
            | First _ -> "Limit translation to only first non-translated entries"


let rec confirm (title: string) =
    printf "%s [yes/y/no/n] " title
    let response = Console.ReadLine().Trim().ToLower()

    match response with
    | "y" | "yes" -> true
    | "n" | "no" -> false
    | _ -> 
        printfn "Invalid input. Please enter 'yes' or 'no'."
        confirm title

let parseCatalog fileName =
    let parser = POParser();
    use stream = new System.IO.FileStream(fileName, System.IO.FileMode.Open)
    let result = parser.Parse(stream)
    result

let createTranslator key targetLanguage =
    let client = TranslationClient.CreateFromApiKey(key)
    let translator = fun (key: string) -> client.TranslateText(key, targetLanguage).TranslatedText
    translator

[<EntryPoint>]
let main argv =
    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let parser = ArgumentParser.Create<CliArguments>(programName = "robotranslator", errorHandler = errorHandler)
    let results = parser.ParseCommandLine (inputs=argv, ignoreUnrecognized = true)

    let sourceFileName = results.GetResult(CliArguments.Source)
    let outputFileName = if results.Contains(CliArguments.Output) then results.GetResult(CliArguments.Output) else sourceFileName

    do
        let result = parseCatalog sourceFileName
        if result.Success then
            let catalog = result.Catalog
            printfn "Parsed %d entries" catalog.Count
            let targetLanguage = results.GetResult(CliArguments.Language)
            let key = results.GetResult(CliArguments.ApiKey)
            printfn "Source language %s. Translating to %s" catalog.Language targetLanguage
            let catalog = result.Catalog
            catalog |> Seq.sumBy (fun item -> item.Key.Id.Length) |> printfn "Approximate count of characters to be translated: %d"
            if confirm "Please confirm that you want to proceed with translation" then
                catalog.Language <- targetLanguage
                let translator = createTranslator key targetLanguage
                let items = catalog |> Seq.filter (fun x -> x[0] |> String.IsNullOrEmpty)
                let items =
                    if results.Contains(CliArguments.First) then 
                        items |> Seq.take (results.GetResult(CliArguments.First)) 
                    else
                        items
                for item in items do
                    try
                        match item with
                        | :? POSingularEntry as pse ->
                            let translation = translator item.Key.Id
                            pse.Translation <- translation
                            let flagComment = 
                                match pse.Comments |> Seq.tryFind (fun c -> c.Kind = POCommentKind.Flags) with
                                | Some comment -> 
                                    comment :?> POFlagsComment
                                | None ->
                                    let newComment = POFlagsComment()
                                    newComment.Flags <- System.Collections.Generic.HashSet()
                                    pse.Comments.Add(newComment)
                                    newComment
                            flagComment.Flags.Add("fuzzy") |> ignore
                        | _ -> 
                            printfn "Only singular entries are supported. Key %s skipped." item.Key.Id
                            ()
                    with
                        | ex -> 
                            printfn "Error translating key %s: %s" item.Key.Id ex.Message
                let generator = POGenerator()
                use writer = new System.IO.StreamWriter(outputFileName)
                generator.Generate(writer, catalog)
        else
            let diagnostics = result.Diagnostics
            for diagnostic in diagnostics do
                printfn "Error: %s, Severity: %O. %A" diagnostic.Code diagnostic.Severity diagnostic.Args
    0

open Karambolo.PO
open System
open System.Diagnostics
open Eto.Forms
open Eto.Drawing

type MyObject() =
    let mutable textProperty = ""
    member this.TextProperty
      with get () = textProperty
      and set value = 
        textProperty <- value
        Console.WriteLine(sprintf "Set TextProperty to %s" value)

let getIso639Languages () =
    let languages =
        dict [
            "af", "Afrikaans"
            "sq", "Albanian"
            "am", "Amharic"
            "ar", "Arabic"
            "hy", "Armenian"
            "az", "Azerbaijani"
            "eu", "Basque"
            "be", "Belarusian"
            "bn", "Bengali"
            "bs", "Bosnian"
            "bg", "Bulgarian"
            "ca", "Catalan"
            "ceb", "Cebuano"
            "zh", "Chinese"
            "co", "Corsican"
            "hr", "Croatian"
            "cs", "Czech"
            "da", "Danish"
            "nl", "Dutch"
            "en", "English"
            "eo", "Esperanto"
            "et", "Estonian"
            "fi", "Finnish"
            "fr", "French"
            "gl", "Galician"
            "ka", "Georgian"
            "de", "German"
            "el", "Greek"
            "gu", "Gujarati"
            "ht", "Haitian Creole"
            "ha", "Hausa"
            "he", "Hebrew"
            "hi", "Hindi"
            "hu", "Hungarian"
            "is", "Icelandic"
            "id", "Indonesian"
            "ga", "Irish"
            "it", "Italian"
            "ja", "Japanese"
            "kn", "Kannada"
            "kk", "Kazakh"
            "km", "Khmer"
            "ko", "Korean"
            "ku", "Kurdish"
            "ky", "Kyrgyz"
            "lo", "Lao"
            "lv", "Latvian"
            "lt", "Lithuanian"
            "lb", "Luxembourgish"
            "mk", "Macedonian"
            "mg", "Malagasy"
            "ms", "Malay"
            "ml", "Malayalam"
            "mt", "Maltese"
            "mi", "Maori"
            "mr", "Marathi"
            "mn", "Mongolian"
            "ne", "Nepali"
            "no", "Norwegian"
            "or", "Odia"
            "ps", "Pashto"
            "fa", "Persian"
            "pl", "Polish"
            "pt", "Portuguese"
            "pa", "Punjabi"
            "ro", "Romanian"
            "ru", "Russian"
            "sm", "Samoan"
            "gd", "Scottish Gaelic"
            "sr", "Serbian"
            "st", "Sesotho"
            "sn", "Shona"
            "sd", "Sindhi"
            "si", "Sinhala"
            "sk", "Slovak"
            "sl", "Slovenian"
            "so", "Somali"
            "es", "Spanish"
            "su", "Sundanese"
            "sw", "Swahili"
            "sv", "Swedish"
            "tl", "Tagalog"
            "tg", "Tajik"
            "ta", "Tamil"
            "tt", "Tatar"
            "te", "Telugu"
            "th", "Thai"
            "tr", "Turkish"
            "tk", "Turkmen"
            "uk", "Ukrainian"
            "ur", "Urdu"
            "ug", "Uyghur"
            "uz", "Uzbek"
            "vi", "Vietnamese"
            "cy", "Welsh"
            "xh", "Xhosa"
            "yi", "Yiddish"
            "yo", "Yoruba"
            "zu", "Zulu"
        ]
    languages

let iso639Languages = getIso639Languages()

type Translation(entry: IPOEntry) =
    member val Source : string = entry.Key.Id with get, set
    member val Translation : string = entry[0] with get, set

let parseCatalog fileName =
    let parser = POParser();
    use stream = new System.IO.FileStream(fileName, System.IO.FileMode.Open)
    let result = parser.Parse(stream)
    result

type MyForm() as this =
    inherit Form()

    do
        this.ClientSize <- Size(600, 400)
        this.Title <- "Robo Translator"

        let grid = new GridView<Translation>()
        grid.ShowHeader <- true
        let createSimpleColumn (column: string) header =
            let c = new GridColumn()
            c.HeaderText <- header
            c.DataCell <- new TextBoxCell(column)
            c.Editable <- false
            c.Resizable <- true
            c.Expand <- true
            c
        grid.Columns.Add(createSimpleColumn "Source" "Source - English")
        grid.Columns.Add(createSimpleColumn "Translation" "Translation")
        this.Content <- grid
        
        // Set data context so it propegates to all child controls
        this.DataContext <- MyObject(TextProperty = "Initial Value 1")

        let quitItemCommand = 
            new Command(
                (fun sender e -> Application.Instance.Quit()),
                MenuText = "Quit",
                Shortcut = (Application.Instance.CommonModifier ||| Keys.Q)
            )
        let quitMenuItem = quitItemCommand.CreateMenuItem()
        let openFileItemCommand = 
            new Command(
                (fun sender e -> 
                    let dialog = new Eto.Forms.OpenFileDialog()
                    if dialog.ShowDialog this = DialogResult.Ok then
                        let parseResults = parseCatalog dialog.FileName
                        if parseResults.Success then
                            let catalog = parseResults.Catalog
                            grid.DataStore <- catalog |> Seq.map (fun item -> Translation(item)) |> Array.ofSeq
                            grid.Columns[1].HeaderText <- "Translation - " + iso639Languages[catalog.Language]
                    else
                        ()
                ),
                MenuText = "&Open",
                Shortcut = (Application.Instance.CommonModifier ||| Keys.O)
            )

        let fileMenu = new SubMenuItem(Text = "&File")
        fileMenu.Items.Add(openFileItemCommand.CreateMenuItem())
        //fileMenu.Items.Add(quitMenuItem)
        //this.Menu.Items <- new MenuItemsCollection()
        let menu = new MenuBar()
        menu.Items.Add(fileMenu)
        menu.QuitItem <- quitMenuItem
        this.Menu <- menu

[<EntryPoint;STAThread>]
let main argv = 
    (new Application()).Run(new MyForm())
    0
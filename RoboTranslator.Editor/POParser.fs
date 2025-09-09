module POParser

open Karambolo.PO

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

let writePOCatalog (fileName: string) (catalog: POCatalog) =
    let generator = POGenerator()
    if catalog.Encoding = null then
        catalog.Encoding <- "utf-8"
    use writer = new System.IO.StreamWriter(fileName)
    generator.Generate(writer, catalog)

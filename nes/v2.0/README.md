![Image](images/nes_logo.png) 

# Paczka eADM i Niezbędne Elementy Struktury (dokumentu elektronicznego) 2.0
- [Krótki opis zawartości](#zawartość)
- [Biblioteki .NET](#nuget)
- [Historia wersji](#historia-wersji)
- [Przykłady użycia bibliotek .NET](#przykłady)
- [Opis merytoryczny](#historia)

## Zawartość

Biblioteki NES służą do tworzenia paczki [eADM](https://gov.legalis.pl/przekazywanie-skargi-do-sadu-administracyjnego/), która swoją strukturą odpowiada paczce archiwalnej przekazywanej do [Archiwów Państwowych](https://www.archiwa.gov.pl/). Paczka oprócz samych dokumentów, zawiera opisujące je pliki metadanych. Więcej informacji można znaleźć w&nbsp;Rozporządzeniu Prezydenta Rzeczypospolitej Polskiej w&nbsp;sprawie szczegółowego sposobu oraz szczegółowych warunków przekazywania skargi wraz z&nbsp;aktami sprawy i&nbsp;odpowiedzią na skargę do sądu administracyjnego  [(Dz.U. z&nbsp;2019&nbsp;r. poz. 1003)](https://eli.gov.pl/eli/DU/2019/1003/ogl).

W tym katalogu znajduje się:

* oryginalny schemat XSD [nes_20.xsd](https://raw.githubusercontent.com/abcpro-pl/elektronizacja-prawa/master/nes/v2.0/nes_20.xsd) dla metadanych paczki eADM i schemat XSD [Metadane-1.7.xsd](https://raw.githubusercontent.com/abcpro-pl/elektronizacja-prawa/master/nes/v2.0/Metadane-1.7.xsd) dla metadanych paczki archiwalnej, 
* kod źródłowy biblioteki do tworzenia, edycji i&nbsp;zapisu plików XML metadanych zgodnych ze schematami [ABCPRO.NES](https://github.com/abcpro-pl/elektronizacja-prawa/tree/master/nes/v2.0/Abc.Nes). Biblioteka pozwala na utworzenie pliku XSD na podstawie modelu. W&nbsp;przeciwieństwie do oryginalnego [pliku XSD](https://raw.githubusercontent.com/abcpro-pl/elektronizacja-prawa/master/nes/v2.0/nes_20.xsd), - który można pobrać również z&nbsp;[profilu Ministerstwa Cyfryzacji](https://github.com/Ministerstwo-Cyfryzacji/ezd-analizy-it/blob/master/nes_bnf_komentarz.md), ten [wygenerowany z&nbsp;modelu](https://raw.githubusercontent.com/abcpro-pl/elektronizacja-prawa/master/nes/v2.0/nes_20_generated.xsd) nie zawiera żadnych błędów walidacji i&nbsp;referecji do zewnętrznych słowników, wszystkie elementy są opatrzone komentarzem, ponadto dodano możliwość umieszczania elementów ds:Signature czyli owzorowania podpisów elektronicznych. W&nbsp;katalogu [doc](https://github.com/abcpro-pl/elektronizacja-prawa/tree/master/nes/v2.0/doc) znajduje się [dokumentacja wygenerowana na podstawie schematu](doc/nes_20_generated.pdf),
* kod źródłowy biblioteki do tworzenia paczki eADM [ABCPRO.NES.ArchivalPackage](https://github.com/abcpro-pl/elektronizacja-prawa/tree/master/nes/v2.0/Abc.Nes.ArchivalPackage). Za pomocą biblioteki można tworzyć, edytować i&nbsp;zapisywać dokumenty i&nbsp;metadane w&nbsp;paczce eADM zgodnie z&nbsp;wymogami rozporządzeń.
* kod źródłowy biblioteki do podpisywania plików XML [ABCPRO.NES.XAdES](https://github.com/abcpro-pl/elektronizacja-prawa/tree/master/nes/v2.0/Abc.Nes.Xades)
* kod źródłowy biblioteki do podpisywania paczki eADM [ABCPRO.NES.ArchivalPackage.Cryptography](https://github.com/abcpro-pl/elektronizacja-prawa/tree/master/nes/v2.0/Abc.Nes.ArchivalPackage.Cryptography)

 ## NuGet

<a href="https://www.nuget.org/packages/ABCPRO.NES/"><img alt="Nuget" src="https://img.shields.io/nuget/v/ABCPRO.NES?label=abcpro.nes%20nuget"> <img alt="Nuget" src="https://img.shields.io/nuget/dt/ABCPRO.NES"></a>

 ## Historia wersji

### Historia wersji ABCPRO.NES

Wersja  | Opis
--------|--------
1.1.11|Dodanie opcji weryfikacji plików ZIPX oraz certyfikatów w podpisach.
1.1.10|Dodanie metody `void AddFiles(IEnumerable<DocumentFile> documents, string folderName = null, IEnumerable<IDocument> metadata = null);`
1.1.9|Naprawa błędu występującego podczas weryfikacji podpisu w&nbsp;aplikacjach .NetCore 3.x.
1.1.8|Zmiana modyfikatora dostepu na public klas RSAPKCS1SHA256SignatureDescription i&nbsp;RSAPKCS1SHA512SignatureDescription w&nbsp;Abc.Nes.Xades.
1.1.7|Dodanie metody do weryfikacji wskazanego pliku w&nbsp;paczce eADM.
1.1.6|Dodanie metod do weryfikacji podpisu plików paczki i&nbsp;pliku .xades dla całej paczki eADM. 
1.1.5|Dodanie rozszerzeń dla enumeracji.
1.1.4|Dodanie metod do pobierania informacji o&nbsp;podpisach elektronicznych.
1.1.3|Dodanie nowych parametrów do ustawiania wizualizacji podpisu pliku PDF.
1.1.2|Dodanie metody do podpisywania plików PDF z&nbsp;dysku, poprawione wyświetlanie informacji o&nbsp;podpisie na PDF.
1.1.1|Usunięcie błędów z&nbsp;wczytywanie schematu metadanych 1.7, dodanie opcji wskazania serwera znacznika czasu podczas podpisywania.
1.1.0|Złączenie wszystkich bibliotek w&nbsp;jeden pakiet 
1.0.10|Dopracowanie opcji zgodności ze schematem `Metadane 1.7`.
1.0.9|Dodanie wsparcia dla schematu metadanych w wersji 1.7 (Klasa `Document17`) czyli używanych przez paczkę archiwalną przekazywaną do AP. Natomiast standardowa klasa `Document` jest zgodna ze schematem 2.0 na potrzeby paczki eADM.
1.0.8|Dodanie polskich opisów błędów przy walidacji metadanych. W&nbsp;pliku `AssemblyIno.cs` projektu należy dodać dyrektywę  `[assembly: NeutralResourcesLanguage("pl")]` lub z&nbsp;poziomu kodu ustawić inne `CultureInfo`.
1.0.7|Dodanie przestrzeni nazw `Abc.Nes.Validators` do wyodrębnienia funkcji walidacji metadanych. W&nbsp;klasie`XmlConverter` dodano metodę `GetValidationResult` zwracającą obiekt ze szczegółową lokalizacją błędów.
1.0.5|Zawiera dodatkowe pola w&nbsp;adresie (gmina, powiat, województwo). Dodane metody statyczne do pobierania wartości z&nbsp;enumeratorów dla pól tekstowych np. `RelationElement.GetRelationType()`.
1.0.3|Pierwsza stabilna wersja biblioteki. Pozwala na dodawanie wszystkich metadanych, walidację i&nbsp;zapis do pliku XML. Za pomocą klasy `XmlConverter` możliwe jest otwieranie plików zarówno z&nbsp;wersji 2.0 jak i&nbsp;tych starszych.

### Historia wersji ABCPRO.NES.ArchivalPackage

Wersja  | Opis
--------|--------
ABCPRO.NES| Zawarty w pakiecie głównym.
1.0.15|Poprawiony błąd przy zapisie podpisanej paczki.
1.0.14|Dopracowanie opcji zgodności ze schematem `Metadane 1.7`.
1.0.13|Aktualizacja zależności.
1.0.12|Dodanie polskich opisów błędów przy walidacji paczki. W&nbsp;pliku `AssemblyIno.cs` projektu należy dodać dyrektywę  `[assembly: NeutralResourcesLanguage("pl")]` lub z&nbsp;poziomu kodu ustawić inne `CultureInfo`.
1.0.11|Dodanie do klasy `PackageManager` metody `GetValidationResult` zwracającą obiekt ze szczegółową lokalizacją błędów walidacji paczki i&nbsp;metadanych.
1.0.10|Dodanie metody `Validate` do sprawdzania poprawności struktury paczki eADM.
1.0.9|Dodanie metod: do pobierania obiektu folderu dla wskazanego obiektu pliku, do pobierania pliku metadanych dla wskazanego pliku z&nbsp;katalogu dokumenty, do pobierania obiektu pliku na podstawie ścieżki wewnątrz archiwum.
1.0.6|Dodanie metody w&nbsp;`PackageManager` umożliwiającej proste uzupełnianie pliku metadanych sprawy.
1.0.5|Aktualizacja zależności.
1.0.4|Pierwsza stabilna wersja biblioteki. Pozwala na tworzenie paczki eADM z&nbsp;wymaganymi katalogami, dodawanie plików oraz metadanych utworzonych za pomocą ABCPRO.NES, zapis i&nbsp;odczyt wcześniej utworzonych paczek.

### Historia wersji ABCPRO.NES.ArchivalPackage.Cryptography

Wersja  | Opis
--------|--------
ABCPRO.NES| Zawarty w pakiecie głównym.
1.0.9|Dodanie metody do podpisywania kolekcji plików wewnątrz paczki.
1.0.8|Poprawki błędów
1.0.7|Dopracowanie opcji zgodności ze schematem `Metadane 1.7`.
1.0.6|Aktualizacja zależności.
1.0.5|Aktualizacja zależności.
1.0.4|Aktualizacja zależności.
1.0.3|Dodanie metod do podpisywania wybranych plików w&nbsp;paczce eADM.
1.0.2|Zastąpienie biblioteki komercyjnej `Aspose.Pdf` otwartym kodem `iTextSharp`.
1.0.1|Dodanie mozliwości podpisywania plików w&nbsp;paczce eADM podpisem zewnętrznym oraz podpisywanie samego archiwum podpisem zewnętrznym.
1.0.0|Dodanie biblioteki pozwalającej na podpisywanie pliku paczki eADM wraz z&nbsp;plikami w&nbsp;niej umieszczonymi. Pliki XML podpisywane są podpisem wewnętrznym XAdES, pliki PDF podpisem PAdES (podpis realizowany jest z&nbsp;wykorzystaniem biblioteki `Aspose.Pdf`. wymagany jest zakup licencji na oprogramowanie [`Aspose.Pdf`](https://products.aspose.com/pdf). Sama paczka archiwalna może zostać umieszczona w&nbsp;pliku `.xades` (podpis okalający - Enveloping) lub w&nbsp;oddzielnym pliku; wówczas plik `.xades` zawiera jedynie referencję do właściwego pliku.

### Historia wersji ABCPRO.NES.XAdES

Wersja  | Opis
--------|--------
ABCPRO.NES| Zawarty w pakiecie głównym.
1.0.6|Wymiana biblioteki `BouncyCastle.NetCore` na `Portable.BouncyCastle` w&nbsp;celu uniknięcia konfliktu z&nbsp;referencją występującą w&nbsp;`ABCPRO.NES.ArchivalPackage.Cryptography`.
1.0.5|Hermetyzacja kodu
1.0.4|Dodanie biblioteki umożliwiającej podpisywanie dokumentów XML. Biblioteka bazuje na kodzie źródłowym [`Microsoft .NET Framework`](https://github.com/dotnet/runtime/tree/master/src/libraries/System.Security.Cryptography.Xml/src) w&nbsp;przestrzeni nazw `Microsoft.XmlDsig`, projektu [`Microsoft.Xades`](https://github.com/Caliper/Xades) utworzonym przez francuski oddział firmy Microsoft oraz na podstawie kodu źródłowego [`FirmaXadesNet`](https://github.com/ctt-gob-es/FirmaXadesNet45) utworzonym przez Departament Nowych Technologii Rady Urbanizacji Miasta Cartagena. Biblioteka pozwala na opatrywanie pliku metadanych bezpiecznym podpisem elektronicznym.  

[&#8682; Do góry](#paczka-eadm-i-niezbędne-elementy-struktury-dokumentu-elektronicznego-20)

## Przykłady

#### Generowanie schematu XSD

``` C#
XElement schema;
using (var xsdGenerator = new XsdGenerator()) {
    schema = xsdGenerator.GetSchema();
}
var filePath = @"..\..\..\nes_20_generated.xsd";
schema.Save(filePath);
```

#### Tworzenie dokumentu XML metadanych

```C#
Abc.Nes.Document document = GetModel(); 
var filePath = Path.Combine(Path.GetTempPath(), "nes.xml");
new Abc.Nes.Converters.XmlConverter().WriteXml(document, filePath);
```

#### Ładowanie dokumentu XML metadanych

```C#
var filePath = Path.Combine(Path.GetTempPath(), "nes.xml");
if (!File.Exists(filePath)) { throw new FileNotFoundException(); }
var document = new Abc.Nes.Converters.XmlConverter().LoadXml(filePath);
```

#### Walidacja dokumentu XML metadanych

Domyslnie komunikaty wyświetlane są w&nbsp;języku angielskim. W&nbsp;celu włączenia komunikatów w&nbsp;języku polskim w&nbsp;pliku `AssemblyIno.cs` projektu należy dodać dyrektywę  `[assembly: NeutralResourcesLanguage("pl")]` lub z&nbsp;poziomu kodu ustawić polskie `CultureInfo`.

```C#
var filePath = Path.Combine(Path.GetTempPath(), "nes.xml");
if (!File.Exists(filePath)) { throw new FileNotFoundException(); }
ar converter = new Abc.Nes.Converters.XmlConverter();
var valid = converter.Validate(filePath);
// converter.ValidationErrors - errors
```
W celu wykonania bardziej szczegółowej walidacji należy użyć metody `GetValidationResult`.

```C#
var c = new  Abc.Nes.Validators.DocumentValidator();
var result = c.Validate(GetModel(true));
foreach (var item in result) {
    Console.WriteLine(item.DefaultMessage);                
}
```

#### Korzystanie z modelu metadanych

```C#
var document = new Abc.Nes.Document() {
    Identifiers = new List<Abc.Nes.Elements.IdentifierElement> {
        new Abc.Nes.Elements.IdentifierElement() {
            Type = Enumerations.IdTypes.ObjectMark.GetIdTypes(),
            Value = "ABC-A.123.77.3.2011.JW.",
            Subject = new Elements.SubjectElement() {
                Institution = new Elements.InstitutionElement() {
                    Name = "Urząd Miasta Wołomierz"
                }
            }
        }
    },
    Titles = new List<Abc.Nes.Elements.TitleElement> {
        new Abc.Nes.Elements.TitleElement(){
            Original = new Abc.Nes.Elements.TitleWithLanguageCodeElement(){
                Type = Enumerations.LanguageCode.pol,
                    Value = "Tytuł dokumentu"
            },
            Alternative = new List<Elements.TitleWithLanguageCodeElement> {
                new Elements.TitleWithLanguageCodeElement() {
                    Type = Enumerations.LanguageCode.eng,
                    Value = "Document title"
                }
            }
        }
    },
    Dates = new List<Elements.DateElement> {
        new Elements.DateElement() {
            Type = Enumerations.EventDateType.Created,
            Date = "2020-06-22"
        }
    },
    Formats = new List<Elements.FormatElement> {
        new Elements.FormatElement() {
            Type = ".pdf",
            Specification = "1.7",
            Uncompleted = Enumerations.BooleanValues.False,
            Size = new Elements.SizeElement() {
                Measure = Enumerations.FileSizeType.kB.GetSizeType(),
                    Value = "4712"
                }
            }
    },
    Access = new List<Elements.AccessElement> {
        new Elements.AccessElement() {
            Access = Enumerations.AccessType.Public,
            Description = "Uwagi dotyczące dostępności",
            Date = new Elements.AccessDateElement() {
                Type = Enumerations.AccessDateType.After,
                Date = "2020-06-23"
            }
        }
    },
    Types = new List<Elements.TypeElement>() {
        new Elements.TypeElement() {
            Class = Enumerations.DocumentClassType.Text.GetDocumentClassType(),
            Kinds = new List<string> { Elements.TypeElement.GetDocumentKindType(Enumerations.DocumentKindType.Document) }
        }
    },
    Groupings = new List<Elements.GroupingElement> {
        new Elements.GroupingElement() {
            Type = "Rejestr korespondencji przychodzącej",
            Code = "RKP01",
            Description = "tekstowy opis grupy"
        }
    },
    Authors = new List<Elements.AuthorElement> {
        new Elements.AuthorElement() {
            Functions = new List<string> { Enumerations.AuthorFunctionType.Created.GetAuthorFunctionType() },
            Subject = new Elements.SubjectElement() {
                Institution = new Elements.InstitutionElement() {
                    Name = "Urząd Miasta Wołomierz"
                }
            }
        }
    },
    Senders = new List<Elements.SenderElement> {
        new Elements.SenderElement() {
            Subject = new Elements.SubjectElement() {
                Institution = new Elements.InstitutionElement() {
                    Name = "Urząd Miasta Wołomierz"
                }
            }
        }
    },
    Recipients = new List<Elements.RecipientElement> {
        new Elements.RecipientElement() {
            CC = Enumerations.BooleanValues.False,
            Subject = new Elements.SubjectElement() {
                Institution = new Elements.InstitutionElement() {
                    Name = "Urząd Miasta Wołomierz"
                }
            }
        },
        new Elements.RecipientElement() {
            CC = Enumerations.BooleanValues.True,
            Subject = new Elements.SubjectElement() {
                Institution = new Elements.InstitutionElement() {
                    Name = "Regionalna Izba Obrachunkowa w Łodzi"
                }
            }
        }
    },
    Relations = new List<Elements.RelationElement> {
        new Elements.RelationElement {
            Identifiers = new List<Elements.IdentifierElement> {
                new Elements.IdentifierElement() {
                    Type = "SystemID",
                    Value = "P00112233.pdf.xades"
                }
            },
            Type = Enumerations.RelationType.HasReference.GetRelationType()
        },
        new Elements.RelationElement {
            Identifiers = new List<Elements.IdentifierElement> {
                new Elements.IdentifierElement() {
                    Type = "SystemID",
                    Value = "dek2010123.txt"
                }
            },
            Type = Enumerations.RelationType.HasAttribution.GetRelationType()
        },
        new Elements.RelationElement {
            Identifiers = new List<Elements.IdentifierElement> {
                new Elements.IdentifierElement() {
                    Type = "SystemID",
                    Value = "P00112233.docx"
                }
            },
            Type = Enumerations.RelationType.IsVersion.GetRelationType()
        },
        new Elements.RelationElement {
            Identifiers = new List<Elements.IdentifierElement> {
                new Elements.IdentifierElement() {
                    Type = "SystemID",
                    Value = "UPD12345.xml"
                }
            },
            Type = Enumerations.RelationType.HasReference.GetRelationType()
        }
    },
    Qualifications = new List<Elements.QualificationElement> {
        new Elements.QualificationElement() {
            Type = Enumerations.ArchivalCategoryType.BE10.GetArchivalCategoryType(),
            Date = "2005-03-05",
            Subject = new Elements.SubjectElement() {
            Institution = new Elements.InstitutionElement() {
                Name = "Urząd Miasta Wołomierz",
                    Unit = new Elements.InstitutionUnitElement() {
                        Name = "Wydział Organizacyjny",
                        Employee = new Elements.EmployeeElement() {
                            FirstNames = new List<string> { "Jan" },
                            Surname = "Kowalski",
                            Position = "Specjalista"
                        }
                    }
                }
            }
        }
    },
    Languages = new List<Elements.LanguageElement>() {
        new Elements.LanguageElement() {
            Type = Enumerations.LanguageCode.pol,
            Value = "polski"
        }
    },
    Description = "projekt dokumentu \"Requirements for elaboration and implementation of information system of General department of Archives\", przekazny przez Przewodniczącą Departamentu Generalnego Archiwów przy Radzie Ministrów Republiki Bułgarii",
    Keywords = new List<Elements.KeywordElement> {
        new Elements.KeywordElement() {
            Matters = new List<string> { "handel" },
            Places = new List<string> { "Polska" },
            Dates = new List<Elements.DateElement> {
                new Elements.DateElement() {
                    Range = Enumerations.DateRangeType.DateFromTo,
                    DateFrom = "2008",
                    DateTo = "2012"
                }
            },
            Others = new List<Elements.KeywordDataElement> {
                new Elements.KeywordDataElement() {
                    Key = "placówki handlowe",
                    Value = "Anna i Jan"
                }
            } 
        }
    },
    Rights = new List<string> { "© Unesco 2003 do polskiego tłumaczenia Naczelna Dyrekcja Archiwów Państwowych" },
    Locations = new List<string> { "Archiwum zakładowe Urzędu Miasta w Wołomierzu" },
    Statuses = new List<Elements.StatusElement> { 
        new Elements.StatusElement() { 
            Kind = "status dokumentu",
            Version = "numer wersji",
            Description = "opis"
        }
    }
};
```
[&#8682; Do góry](#paczka-eadm-i-niezbędne-elementy-struktury-dokumentu-elektronicznego-20)

#### Ładowanie paczki eADM

``` C#
var path = @"../../../sample/ValidatedPackage.zip";
var mgr = new PackageManager();
mgr.LoadPackage(path);

var isNotEmpty = mgr != null && mgr.Package != null && !mgr.Package.IsEmpty;
```            

#### Pobieranie informacji o ilości dokumentów w paczce eADM 

```C#
var path = @"../../../sample/ValidatedPackage.zip";
var mgr = new PackageManager();
mgr.LoadPackage(path);

var count = mgr.GetDocumentsCount();
```

#### Pobieranie listy plików w paczce eADM 

``` C#
var path = @"../../../sample/ValidatedPackage.zip";
var mgr = new PackageManager();
mgr.LoadPackage(path);

var items = mgr.GetAllFiles();
```

#### Pobranie pliku metadanych dla wskazanej ścieżki pliku dokumentu

```C#
var path = @"../../../sample/ValidatedPackage.zip";
var mgr = new PackageManager();
mgr.LoadPackage(path);

var item = mgr.GetItemByFilePath("Dokumenty/Wniosek/Wniosek.xml");
if (item != null) {
    var metadataFile = mgr.GetMetadataFile(item);                
}
```

#### Pobranie obiektu folderu dla wskazanej ścieżki pliku dokumentu

```C#
var path = @"../../../sample/ValidatedPackage.zip";
var mgr = new PackageManager();
mgr.LoadPackage(path);

var folder = mgr.GetParentFolder("dokumenty/Wniosek/Wniosek.xml");
```

#### Ładowanie, dodanie pliku z metadanymi i zapisanie paczki eADM 

``` C#
var path = @"../../../sample/ValidatedPackage.zip";
var mgr = new PackageManager();
mgr.LoadPackage(path);

mgr.AddFile(new DocumentFile() {
    FileData = File.ReadAllBytes(@"../../../sample/sample_file.pdf"),
    FileName = "TabelaWydatkow.pdf"
}, new Document() {
    Identifiers = new List<Elements.IdentifierElement>() {
        new Elements.IdentifierElement() {
            Type = "Numer tabeli",
            Value = "3",
            Subject = new Elements.SubjectElement() {
                Person = new Elements.PersonElement() {
                    FirstNames = new List<string> { "Jan" },
                    Surname = "Kowalski",
                    Contacts = new List<Elements.ContactElement> {
                        new Elements.ContactElement() {
                            Type = ContactType.Email.GetContactType(),
                                Value = "jan.kowalski@miastowolomierz.pl"
                            }
                        }
                    }
                }
            }
        },
        Titles = new List<Elements.TitleElement> {
            new Elements.TitleElement() {
                Original = new Elements.TitleWithLanguageCodeElement(){
                    Type = LanguageCode.pol,
                    Value = "Tabela wydatków"
                }
            }
        },
        Dates = new List<Elements.DateElement> {
            new Elements.DateElement() {
                Type = EventDateType.Created,
                Date = "2020-04-01 12:32:00"
            }
        },
        Formats = new List<Elements.FormatElement> {
            new Elements.FormatElement() {
                Type = "PDF",
                Specification = "1.7",
                Uncompleted = BooleanValues.False,
                Size = new Elements.SizeElement() {
                    Measure = FileSizeType.bajt.GetSizeType(),
                    Value = new FileInfo(@"../../../sample/sample_file.pdf").Length.ToString()
                }
            }
        },
        Access = new List<Elements.AccessElement> {
            new Elements.AccessElement() {
                Access = AccessType.Public
            }
        },
        Types = new List<Elements.TypeElement> {
            new Elements.TypeElement() {
                Class = DocumentClassType.Text.GetDocumentClassType(),
                Kinds = new List<string> { Elements.TypeElement.GetDocumentKindType(DocumentKindType.Regulation) }
            }
        },
        Groupings = new List<Elements.GroupingElement> {
        new Elements.GroupingElement() {
            Type = "Rejestr wydatków",
            Code = "KS_RW",
            Description = "Księgowość: rejestr wydatków"
        }
    }
});

mgr.Save(@"../../../sample/ValidatedPackage.zip");
```

#### Weryfikacja poprawności strukturalnej paczki ADM

Domyslnie komunikaty wyświetlane są w&nbsp;języku angielskim. W&nbsp;celu włączenia komunikatów w&nbsp;języku polskim w&nbsp;pliku `AssemblyIno.cs` projektu należy dodać dyrektywę  `[assembly: NeutralResourcesLanguage("pl")]` lub z&nbsp;poziomu kodu ustawić polskie `CultureInfo`.

```C#
var path = @"../../../sample/ValidatedPackage.zip";
var mgr = new PackageManager();
mgr.LoadPackage(path);
var validateMetdataFiles = true;
var breakOnFirstError = false;
var result = mgr.Validate(out var message, validateMetdataFiles, breakOnFirstError);
if (!result) {
    throw new System.Exception(message);
}
```
W celu wykonania bardziej szczegółowej walidacji należy użyć metody `GetValidationResult`.

```C#
var path = @"../../../sample/ValidatedPackage.zip";
var mgr = new PackageManager();
mgr.LoadPackage(path);
var validateMetdataFiles = true;
var breakOnFirstError = false;
var result = mgr.GetValidationResult(validateMetdataFiles, breakOnFirstError);
if (!result.IsCorrect) {
    foreach (var item in result) {
        Console.WriteLine(item.DefaultMessage);
    }
}
```

[&#8682; Do góry](#paczka-eadm-i-niezbędne-elementy-struktury-dokumentu-elektronicznego-20)

#### Podpisywanie pliku metadanych (podpis w treści pliku XML - enveloped)

```C#
var document = GetModel(); // utworzenie modelu dokumentu metadanych
var documentXml = new Abc.Nes.Converters.XmlConverter().GetXml(document); // konwersja modelu do XML

 using (var manager = new XadesManager()) {
    var xml = new MemoryStream(Encoding.UTF8.GetBytes(documentXml.ToString()));
    var result = manager.AppendSignatureToXmlFile(xml, CertUtil.SelectCertificate(),
    new SignatureProductionPlace() {
        City = "Warszawa",
        CountryName = "Polska",
        PostalCode = "03-825",
        StateOrProvince = "mazowieckie"
   },
   new SignerRole("Wiceprezes Zarządu"));

    var filePath = Path.Combine(Path.GetTempPath(), "signature.xml");
    if (File.Exists(filePath)) { File.Delete(filePath); }
    result.Save(filePath);
    // System.Diagnostics.Process.Start(filePath);
}
```

#### Podpisywanie pliku PDF (podpis okalający - enveloping)

```C#
var path = "../../../doc/nes_20_generated.pdf";
using (var manager = new XadesManager()) {
    var result = manager.CreateEnvelopingSignature(
        new MemoryStream(File.ReadAllBytes(path)), 
        CertUtil.SelectCertificate(),
        new SignatureProductionPlace() {
            City = "Warszawa",
            CountryName = "Polska",
            PostalCode = "03-825",
            StateOrProvince = "mazowieckie"
        },
        new SignerRole("Wiceprezes Zarządu"));

        var filePath = Path.Combine(Path.GetTempPath(), "signature.xml");
        if (File.Exists(filePath)) { File.Delete(filePath); }
        result.Save(filePath);

}
```

#### Podpisywanie pliku PDF (podpis zewnętrzny - detached)

```C#
var path = "../../../doc/nes_20_generated.pdf";
using (var manager = new XadesManager()) {
    var result = manager.CreateDetachedSignature(new FileInfo(path).FullName, CertUtil.SelectCertificate(),
    new SignatureProductionPlace() {
        City = "Warszawa",
        CountryName = "Polska",
        PostalCode = "03-825",
        StateOrProvince = "mazowieckie"
    },
    new SignerRole("Wiceprezes Zarządu"));

    File.Copy(path, Path.Combine(Path.GetTempPath(), "nes_20_generated.pdf"), true);
    var filePath = Path.Combine(Path.GetTempPath(), "nes_20_generated.pdf.xades");
    if (File.Exists(filePath)) { File.Delete(filePath); }
    result.Save(filePath);
```
[&#8682; Do góry](#paczka-eadm-i-niezbędne-elementy-struktury-dokumentu-elektronicznego-20)

#### Podpisywanie paczki eADM  (podpis okalający - enveloping)

```C#
var path = @"../../../sample/ValidatedPackage.zip";
var outputPath = @"../../../sample/SignedPackage.xades";
using (var mgr = new PackageSignerManager()) {
    mgr.Sign(new FileInfo(path).FullName, 
    CertUtil.SelectCertificate(),
    new FileInfo(outputPath).FullName,
    new SignatureProductionPlace() {
        City = "Warszawa",
        CountryName = "Polska",
        PostalCode = "03-825",
        StateOrProvince = "mazowieckie"
    },
    new SignerRole("Wiceprezes Zarządu"),
    true, // Podpisz pliki w paczce eADM
    true, // Podpisz paczkę archiwalną
    false, // w pliku .xades umieść jedynie referencję do pliku paczki (podpis zewnętrzny - detached)
    true, // dodaj znacznik czasu
    "http://time.certum.pl" // adres serwera znacznika czasu
    );
}
```

#### Podpisywanie paczki eADM (podpis zewnętrzny - detached)

```C#
var path = @"../../../sample/ValidatedPackage.zip";
var outputPath = @"../../../sample/SignedPackage.zip";
using (var mgr = new PackageSignerManager()) {
    mgr.Sign(new FileInfo(path).FullName,
        CertUtil.SelectCertificate(),
        new FileInfo(outputPath).FullName,
        new SignatureProductionPlace() {
            City = "Warszawa",
            CountryName = "Polska",
            PostalCode = "03-825",
            StateOrProvince = "mazowieckie"
        },
        new SignerRole("Wiceprezes Zarządu"),
        true, // Podpisz pliki w paczce eADM
        true, // Podpisz paczkę archiwalną
        true, // w pliku .xades umieść jedynie referencję do pliku paczki (podpis zewnętrzny - detached)
        true  // Podpisz pliki w paczce eADM inne niż XML i PDF podpisem zewnętrznym
        );
}

var outputXadesFilePath = @"../../../sample/SignedPackage.zip.xades";
if (File.Exists(outputXadesFilePath)) {
    // ...
}
```

#### Podpisywanie wybranych plików w paczce eADM

```C#
var path = @"../../../sample/ValidatedPackage.zip";
var outputPath = @"../../../sample/SignedPackage.zip";
using (var mgr = new PackageSignerManager()) {
    mgr.SignInternalFile(new FileInfo(path).FullName,
        "Dokumenty/TabelaWydatkow.pdf",
        CertUtil.SelectCertificate(),
        new SignatureProductionPlace() {
            City = "Warszawa",
            CountryName = "Polska",
            PostalCode = "03-825",
            StateOrProvince = "mazowieckie"
        },
        new SignerRole("Wiceprezes Zarządu"),
        false, // Podpis zewnętrzny w pliku .xades
        new FileInfo(outputPath).FullName
    );
}
```

#### Podpisywanie paczki oraz wybranych plików w paczce eADM

```C#
var path = @"../../../sample/ValidatedPackage.zip";
var outputPath = @"../../../sample/SignedPackage.zip";
using (var mgr = new PackageSignerManager()) {
    mgr.Sign(new FileInfo(path).FullName,
        CertUtil.SelectCertificate(),
        new FileInfo(outputPath).FullName,
        new SignatureProductionPlace() {
            City = "Warszawa",
            CountryName = "Polska",
            PostalCode = "03-825",
            StateOrProvince = "mazowieckie"
        },
        new SignerRole("Wiceprezes Zarządu"),
        new string[] {"Dokumenty/Wniosek/Wniosek.xml", "Dokumenty/Tabela_Wydatkow.pdf"}, // Podpisz wybrane pliki w paczce eADM        
        true, // Podpisz paczkę archiwalną
        true, // w pliku .xades umieść jedynie referencję do pliku paczki (podpis zewnętrzny - detached)
        true  // Podpisz pliki w paczce eADM inne niż XML i PDF podpisem zewnętrznym
        );
}

var outputXadesFilePath = @"../../../sample/SignedPackage.zip.xades";
if (File.Exists(outputXadesFilePath)) {
    // ...
}
```

#### Podpisywanie pliku PDF znajdującego się na dysku

```C#
var path = @"../../../sample/sample_file.pdf";
var imagePath = @"../../../sample/legislator.png";
var outputpath = @"../../../sample/sample_file.signed.pdf";
var mgr = new PackageSignerManager();
mgr.SignPdfFile(
    new FileInfo(path).FullName,
    CertUtil.SelectCertificate(),
    "Podpis za zgodnosc z oryginalem",
    "Warszawa",
    true,
    // wielkość obrazka 220x50 pikseli
    apperancePngImage :File.ReadAllBytes(new FileInfo(imagePath).FullName),
    apperancePngImageLocation: PdfSignatureLocation.BottomLeft,
    apperanceLocationX: 360F,
    apperanceLocationY: 620F, //700F,
    apperanceWidth: 220F,
    apperanceHeight: 50F,
    margin: 10F,
    outputFilePath: new FileInfo(outputpath).FullName
);
```

#### Pobranie informacji o podpisie z pliku .xades

```C#
var path = @"../../../sample/SignedPackage.zip.xades";
using (var mgr = new PackageSignerManager()) {
    var result = mgr.GetSignatureInfos(path);    
}
```

#### Pobranie informacji o podpisie z pliku w paczce eADM

```C#
var path = @"../../../sample/SignedPackage.zip";
using (var mgr = new PackageSignerManager()) {
    var result = mgr.GetSignatureInfos(path, "Dokumenty/LegalAct.pdf"); 
}
```

#### Weryfikacja podpisów w paczce eADM

```C#
var path = @"../../../sample/SignedPackage.zip";
var list = new List<ArchivalPackage.Cryptography.Model.SignatureVerifyInfo>();
using (var mgr = new PackageSignerManager()) {
    list.AddRange(mgr.VerifySignatures(path));
}
```
#### Weryfikacja podpisu dla wskazanego pliku w paczce eADM

```C#
var path = @"../../../sample/SignedPackage.zip";
var list = new List<ArchivalPackage.Cryptography.Model.SignatureVerifyInfo>();
using (var mgr = new PackageSignerManager()) {
    list.AddRange(mgr.VerifySignatures(path, "Dokumenty/LegalAct.zip.xades"));
}
```

#### Weryfikacja podpisów w pliku xades paczki eADM

```C#
var path = @"../../../sample/SignedPackage.zip.xades";
var list = new List<ArchivalPackage.Cryptography.Model.SignatureVerifyInfo>();
using (var mgr = new PackageSignerManager()) {
    list.AddRange(mgr.VerifyXadesSignature(path));
}
```

[&#8682; Do góry](#paczka-eadm-i-niezbędne-elementy-struktury-dokumentu-elektronicznego-20)

## Historia

Historia niezbędnych elementów dokumentów elektronicznych rozpoczęła się 30 października 2006 roku wraz z wydaniem rozporządzenia Ministra Spraw Wewnętrznych i&nbsp;Administracji w&nbsp;sprawie niezbędnych elementów struktury dokumentów elektronicznych [(Dz.U. z&nbsp;2006&nbsp;r. Nr 206, poz. 1517)](https://eli.gov.pl/eli/DU/2006/1517/ogl). 

NES od początku miały opisywać dokumenty przekazywane do Archiwów Państwowych, a&nbsp;rozporządzenie wynika z&nbsp;delegacji zamieszczonej w&nbsp;ustawie o&nbsp;narodowym zasobie archiwalnym i&nbsp;archiwach  [(Dz.U. z 2019 r. poz. 553)](https://eli.gov.pl/eli/DU/1983/173/ogl), a&nbsp;wydał je Minister Spraw Wewnętrznych i&nbsp;Administracji w&nbsp;rozporządzeniu w&nbsp;sprawie wymagań technicznych formatów zapisu i&nbsp;informatycznych nośników danych, na których utrwalono materiały archiwalne przekazywane do archiwów państwowych  [(Dz.U. z&nbsp;2006&nbsp;r. Nr&nbsp;206, poz. 1519)](https://eli.gov.pl/eli/DU/2006/1519/ogl).

W Dz.U. z&nbsp;2019&nbsp;r. pod poz. 1003 opublikowano rozporządzenia Prezydenta Rzeczypospolitej Polskiej z&nbsp;27 maja 2019&nbsp;r. w&nbsp;sprawie szczegółowego sposobu oraz szczegółowych warunków przekazywania skargi wraz z&nbsp;aktami sprawy i&nbsp;odpowiedzią na skargę do sądu administracyjnego [(Dz.U. z&nbsp;2019&nbsp;r. poz. 1003)](https://eli.gov.pl/eli/DU/2019/1003/ogl).

Z&nbsp;rozporządzenia wynika, że skargę oraz odpowiedź na skargę organ przekazuje w&nbsp;formie lub postaci, w jakiej zostały sporządzone. Natomiast skargę lub odpowiedź na skargę *sporządzoną w formie dokumentu elektronicznego* organ przekazuje do elektronicznej skrzynki podawczej sądu.

Jeżeli forma lub postać skargi i&nbsp;odpowiedzi na skargę różnią się, organ, przekazując dokument elektroniczny **załącza uwierzytelnioną kopię kwalifikowaną pieczęcią elektroniczną, kwalifikowanym podpisem elektronicznym lub podpisem (profilem) zaufanym**.

Akta sprawy organ przekazuje do sądu w takiej postaci, w&nbsp;jakiej są prowadzone. Akta można prowadzić w&nbsp;formie papierowej i&nbsp;elektronicznej. W&nbsp;przypadku gdy akta sprawy są prowadzone w&nbsp;postaci elektronicznej, organ przekazuje akta sprawy w&nbsp;sposób, o&nbsp;którym mowa w&nbsp;§7 ust.&nbsp;1 rozporządzenia, *wraz ze skargą lub odpowiedzią na skargę sporządzoną w&nbsp;formie dokumentu elektronicznego*, do elektronicznej skrzynki podawczej sądu. Jeżeli w&nbsp;aktach sprawy prowadzonych w&nbsp;postaci elektronicznej znajdują się dokumenty, których treść nie jest dostępna w&nbsp;całości w&nbsp;postaci elektronicznej, organ przekazuje do sądu:
* akta sprawy, podając informację o&nbsp;sposobie i&nbsp;dacie przekazania dokumentów, których treść nie jest dostępna w&nbsp;całości w&nbsp;postaci elektronicznej;
* dokumenty, których treść nie jest dostępna w&nbsp;całości w&nbsp;postaci elektronicznej, wskazując na akta sprawy oraz datę ich przekazania do sądu.

Z&nbsp;§7 rozporządzenia wynika, że akta sprawy prowadzone w&nbsp;postaci elektronicznej przekazuje się jako wyodrębniony z&nbsp;systemu elektronicznego zarządzania dokumentacją, w&nbsp;rozumieniu przepisów o&nbsp;narodowym zasobie archiwalnym i&nbsp;archiwach, zbiór dokumentów obejmujących akta sprawy (paczka eADM). Opisane powyżej biblioteki służą do jej sporządzenia i&nbsp;podpisania bezpiecznym podpisem elektronicznym. Więcej informacji o&nbsp;eADM można znaleźć na stronie [gov.legalis.pl](https://gov.legalis.pl/przekazywanie-skargi-do-sadu-administracyjnego/).

[&#8682; Do góry](#paczka-eadm-i-niezbędne-elementy-struktury-dokumentu-elektronicznego-20)

<a href="https://www.abcpro.pl"><img alt="www" src="https://img.shields.io/badge/www-abcpro.pl-orange?style=for-the-badge"></a> 
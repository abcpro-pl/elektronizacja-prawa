![Image](images/nes_logo.png) 

# Paczka eADM i Niezbędne Elementy Struktury (dokumentu elektronicznego) 2.0
- [Krótki opis zawartości](#zawartość)
- [Biblioteki .NET](#nuget)
- [Przykłady użycia bibliotek .NET](#przykłady)
    * [Biblioteka ABCPRO.NES](#abcprones)
    * [Biblioteka ABCPRO.NES.ArchivalPackage](#abcpronesarchivalpackage)
    * [Biblioteka ABCPRO.NES.ArchivalPackage.Cryptography](#abcpronesarchivalpackagecryptography)
    * [Biblioteka ABCPRO.NES.XAdES](#abcpronesxades)
- [Opis merytoryczny](#historia)

## Zawartość

Biblioteki NES służą do tworzenia paczki [eADM](https://gov.legalis.pl/przekazywanie-skargi-do-sadu-administracyjnego/), która swoją strukturą odpowiada paczce archiwalnej przekazywanej do [Archiwów Państwowych](https://www.archiwa.gov.pl/). Paczka oprócz samych dokumentów, zawiera opisujące je pliki metadanych. Więcej informacji można znaleźć w&nbsp;Rozporządzeniu Prezydenta Rzeczypospolitej Polskiej w&nbsp;sprawie szczegółowego sposobu oraz szczegółowych warunków przekazywania skargi wraz z&nbsp;aktami sprawy i&nbsp;odpowiedzią na skargę do sądu administracyjnego  [(Dz.U. z&nbsp;2019&nbsp;r. poz. 1003)](https://eli.gov.pl/eli/DU/2019/1003/ogl).

W tym katalogu znajduje się:

* oryginalny schemat XSD [nes_20.xsd](https://raw.githubusercontent.com/abcpro-pl/elektronizacja-prawa/master/nes/v2.0/nes_20.xsd), 
* kod źródłowy biblioteki do tworzenia, edycji i&nbsp;zapisu pliku XML zgodnego ze schematem [ABCPRO.NES](https://github.com/abcpro-pl/elektronizacja-prawa/tree/master/nes/v2.0/Abc.Nes). Biblioteka pozwala na utworzenie pliku XSD na podstawie modelu. W&nbsp;przeciwieństwie do oryginalnego [pliku XSD](https://raw.githubusercontent.com/abcpro-pl/elektronizacja-prawa/master/nes/v2.0/nes_20.xsd), - który można pobrać również z&nbsp;[profilu Ministerstwa Cyfryzacji](https://github.com/Ministerstwo-Cyfryzacji/ezd-analizy-it/blob/master/nes_bnf_komentarz.md), ten [wygenerowany z modelu](https://raw.githubusercontent.com/abcpro-pl/elektronizacja-prawa/master/nes/v2.0/nes_20_generated.xsd) nie zawiera żadnych błędów walidacji i&nbsp;referecji do zewnętrznych słowników, wszystkie elementy są opatrzone komentarzem, ponadto dodano możliwość umieszczania elementów ds:Signature czyli owzorowania podpisów elektronicznych. W&nbsp;katalogu [doc](https://github.com/abcpro-pl/elektronizacja-prawa/tree/master/nes/v2.0/doc) znajduje się [dokumentacja wygenerowana na podstawie schematu](doc/nes_20_generated.pdf),
* kod źródłowy biblioteki do tworzenia paczki eADM [ABCPRO.NES.ArchivalPackage](https://github.com/abcpro-pl/elektronizacja-prawa/tree/master/nes/v2.0/Abc.Nes.ArchivalPackage). Za pomocą biblioteki można tworzyć, edytować i&nbsp;zapisywać dokumenty i&nbsp;metadane w&nbsp;paczce eADM zgodnie z&nbsp;wymogami rozporządzeń.
* kod źródłowy biblioteki do podpisywania plików XML [ABCPRO.NES.XAdES](https://github.com/abcpro-pl/elektronizacja-prawa/tree/master/nes/v2.0/Abc.Nes.Xades)
* kod źródłowy biblioteki do podpisywania paczki eADM [ABCPRO.NES.ArchivalPackage.Cryptography](https://github.com/abcpro-pl/elektronizacja-prawa/tree/master/nes/v2.0/Abc.Nes.ArchivalPackage.Cryptography)

 ## NuGet

<a href="https://www.nuget.org/packages/ABCPRO.NES/"><img alt="Nuget" src="https://img.shields.io/nuget/v/ABCPRO.NES?label=abcpro.nes%20nuget"> <img alt="Nuget" src="https://img.shields.io/nuget/dt/ABCPRO.NES"></a>

<a href="https://www.nuget.org/packages/ABCPRO.NES.ArchivalPackage/"><img alt="Nuget" src="https://img.shields.io/nuget/v/ABCPRO.NES.ArchivalPackage?label=abcpro.nes.archivalpackage%20nuget"> <img alt="Nuget" src="https://img.shields.io/nuget/dt/ABCPRO.NES.ArchivalPackage"></a>

<a href="https://www.nuget.org/packages/ABCPRO.NES.XAdES/"><img alt="Nuget" src="https://img.shields.io/nuget/v/ABCPRO.NES.XAdES?label=abcpro.nes.xades%20nuget"> <img alt="Nuget" src="https://img.shields.io/nuget/dt/ABCPRO.NES.XAdES"></a>

<a href="https://www.nuget.org/packages/ABCPRO.NES.ArchivalPackage.Cryptography/"><img alt="Nuget" src="https://img.shields.io/nuget/v/ABCPRO.NES.ArchivalPackage.Cryptography?label=abcpro.nes.archivalpackage.cryptography%20nuget"> <img alt="Nuget" src="https://img.shields.io/nuget/dt/ABCPRO.NES.ArchivalPackage.Cryptography"></a>

Nazwa | Wersja | Opis
------|--------|--------
ABCPRO.NES.ArchivalPackage.Cryptography|1.0.5|Aktualizacja zależności.
ABCPRO.NES.ArchivalPackage|1.0.12|Dodanie polskich opisów błędów przy walidacji paczki. W&nbsp;pliku `AssemblyIno.cs` projektu należy dodać dyrektywę  `[assembly: NeutralResourcesLanguage("pl")]` lub z&nbsp;poziomu kodu ustawić inne `CultureInfo`.
ABCPRO.NES|1.0.8|Dodanie polskich opisów błędów przy walidacji metadanych. W&nbsp;pliku `AssemblyIno.cs` projektu należy dodać dyrektywę  `[assembly: NeutralResourcesLanguage("pl")]` lub z&nbsp;poziomu kodu ustawić inne `CultureInfo`.
ABCPRO.NES.ArchivalPackage.Cryptography|1.0.4|Aktualizacja zależności.
ABCPRO.NES.ArchivalPackage|1.0.11|Dodanie do klasy `PackageManager` metody `GetValidationResult` zwracającą obiekt ze szczegółową lokalizacją błędów walidacji paczki i&nbsp;metadanych.
ABCPRO.NES|1.0.7|Dodanie przestrzeni nazw `Abc.Nes.Validators` do wyodrębnienia funkcji walidacji metadanych. W&nbsp;klasie`XmlConverter` dodano metodę `GetValidationResult` zwracającą obiekt ze szczegółową lokalizacją błędów.
ABCPRO.NES.ArchivalPackage.Cryptography|1.0.3|Dodanie metod do podpisywania wybranych plików w&nbsp;paczce eADM.
ABCPRO.NES.ArchivalPackage|1.0.10|Dodanie metody `Validate` do sprawdzania poprawności struktury paczki eADM.
ABCPRO.NES.ArchivalPackage.Cryptography|1.0.2|Zastąpienie biblioteki komercyjnej `Aspose.Pdf` otwartym kodem `iTextSharp`.
ABCPRO.NES.XAdES|1.0.6|Wymiana biblioteki `BouncyCastle.NetCore` na `Portable.BouncyCastle` w&nbsp;celu uniknięcia konfliktu z&nbsp;referencją występującą w&nbsp;`ABCPRO.NES.ArchivalPackage.Cryptography`.
ABCPRO.NES.XAdES|1.0.5|Hermetyzacja kodu
ABCPRO.NES.ArchivalPackage.Cryptography|1.0.1|Dodanie mozliwości podpisywania plików w&nbsp;paczce eADM podpisem zewnętrznym oraz podpisywanie samego archiwum podpisem zewnętrznym.
ABCPRO.NES.ArchivalPackage|1.0.9|Dodanie metod: do pobierania obiektu folderu dla wskazanego obiektu pliku, do pobierania pliku metadanych dla wskazanego pliku z&nbsp;katalogu dokumenty, do pobierania obiektu pliku na podstawie ścieżki wewnątrz archiwum.
ABCPRO.NES.ArchivalPackage.Cryptography|1.0.0|Dodanie biblioteki pozwalającej na podpisywanie pliku paczki eADM wraz z&nbsp;plikami w&nbsp;niej umieszczonymi. Pliki XML podpisywane są podpisem wewnętrznym XAdES, pliki PDF podpisem PAdES (podpis realizowany jest z&nbsp;wykorzystaniem biblioteki `Aspose.Pdf`. wymagany jest zakup licencji na oprogramowanie [`Aspose.Pdf`](https://products.aspose.com/pdf). Sama paczka archiwalna może zostać umieszczona w&nbsp;pliku `.xades` (podpis okalający - Enveloping) lub w&nbsp;oddzielnym pliku; wówczas plik `.xades` zawiera jedynie referencję do właściwego pliku.
ABCPRO.NES.XAdES|1.0.4|Dodanie biblioteki umożliwiającej podpisywanie dokumentów XML. Biblioteka bazuje na kodzie źródłowym [`Microsoft .NET Framework`](https://github.com/dotnet/runtime/tree/master/src/libraries/System.Security.Cryptography.Xml/src) w&nbsp;przestrzeni nazw `Microsoft.XmlDsig`, projektu [`Microsoft.Xades`](https://github.com/Caliper/Xades) utworzonym przez francuski oddział firmy Microsoft oraz na podstawie kodu źródłowego [`FirmaXadesNet`](https://github.com/ctt-gob-es/FirmaXadesNet45) utworzonym przez Departament Nowych Technologii Rady Urbanizacji Miasta Cartagena. Biblioteka pozwala na opatrywanie pliku metadanych bezpiecznym podpisem elektronicznym.  
ABCPRO.NES.ArchivalPackage|1.0.6|Dodanie metody w&nbsp;`PackageManager` umożliwiającej proste uzupełnianie pliku metadanych sprawy.
ABCPRO.NES.ArchivalPackage|1.0.5|Aktualizacja zależności.
ABCPRO.NES|1.0.5|Zawiera dodatkowe pola w&nbsp;adresie (gmina, powiat, województwo). Dodane metody statyczne do pobierania wartości z&nbsp;enumeratorów dla pól tekstowych np. `RelationElement.GetRelationType()`.
ABCPRO.NES.ArchivalPackage|1.0.4|Pierwsza stabilna wersja biblioteki. Pozwala na tworzenie paczki eADM z&nbsp;wymaganymi katalogami, dodawanie plików oraz metadanych utworzonych za pomocą ABCPRO.NES, zapis i&nbsp;odczyt wcześniej utworzonych paczek.
ABCPRO.NES|1.0.3|Pierwsza stabilna wersja biblioteki. Pozwala na dodawanie wszystkich metadanych, walidację i&nbsp;zapis do pliku XML. Za pomocą klasy `XmlConverter` możliwe jest otwieranie plików zarówno z&nbsp;wersji 2.0 jak i&nbsp;tych starszych.

[&#8682; Do góry](#paczka-eadm-i-niezbędne-elementy-struktury-dokumentu-elektronicznego-20)

## Przykłady

### ABCPRO.NES

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
            Type = Abc.Nes.Elements.IdentifierElement.GetIdTypes(Enumerations.IdTypes.ObjectMark),
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
                Measure = Elements.SizeElement.GetSizeType(Enumerations.FileSizeType.kB),
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
            Class = Elements.TypeElement.GetDocumentClassType(Enumerations.DocumentClassType.Text),
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
            Functions = new List<string> { Elements.AuthorElement.GetAuthorFunctionType(Enumerations.AuthorFunctionType.Created) },
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
            Type = Elements.RelationElement.GetRelationType(Enumerations.RelationType.HasReference)
        },
        new Elements.RelationElement {
            Identifiers = new List<Elements.IdentifierElement> {
                new Elements.IdentifierElement() {
                    Type = "SystemID",
                    Value = "dek2010123.txt"
                }
            },
            Type = Elements.RelationElement.GetRelationType(Enumerations.RelationType.HasAttribution)
        },
        new Elements.RelationElement {
            Identifiers = new List<Elements.IdentifierElement> {
                new Elements.IdentifierElement() {
                    Type = "SystemID",
                    Value = "P00112233.docx"
                }
            },
            Type = Elements.RelationElement.GetRelationType(Enumerations.RelationType.IsVersion)
        },
        new Elements.RelationElement {
            Identifiers = new List<Elements.IdentifierElement> {
                new Elements.IdentifierElement() {
                    Type = "SystemID",
                    Value = "UPD12345.xml"
                }
            },
            Type = Elements.RelationElement.GetRelationType(Enumerations.RelationType.HasReference)
        }
    },
    Qualifications = new List<Elements.QualificationElement> {
        new Elements.QualificationElement() {
            Type = Elements.QualificationElement.GetArchivalCategoryType(Enumerations.ArchivalCategoryType.BE10),
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

### ABCPRO.NES.ArchivalPackage

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
                            Type = Elements.ContactElement.GetContactType(ContactType.Email),
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
                    Measure = Elements.SizeElement.GetSizeType(FileSizeType.bajt),
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
                Class = Elements.TypeElement.GetDocumentClassType(DocumentClassType.Text),
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

### ABCPRO.NES.XAdES

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

### ABCPRO.NES.ArchivalPackage.Cryptography

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
    false // w pliku .xades umieść jedynie referencję do pliku paczki (podpis zewnętrzny - detached)
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
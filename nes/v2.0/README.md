![Image](images/nes_logo.jpg) 

# Niezbędne Elementy Struktury 2.0

## Zawartość

W tym katalogu znajduje się:

* oryginalny schemat XSD [nes_20.xsd](nes_20.xsd), 
* kod źródłowy biblioteki do tworzenia, edycji i zapisu pliku XML zgodnego ze schematem [ABCPRO.NES](Abc.Nes). Biblioteka pozwala na utworzenie pliku XSD na podstawie modelu. W przeciwieństwie do oryginalnego [pliku XSD](nes_20.xsd), - który można pobrać również z [profilu Ministerstwa Cyfryzacji](https://github.com/Ministerstwo-Cyfryzacji/ezd-analizy-it), ten [wygenerowany z modelu](nes_20_generated.xsd) nie zawiera żadnych błędów walidacji i referecji do zewnętrznych słowników, wszystkie elementy są opatrzone komentarzem, ponadto dodano możliwość umieszczania elementów ds:Signature czyli owzorowania podpisów elektronicznych. W katalogu doc znajduje się [dokumentacja wygenerowana na podstawie schematu](doc),
* kod źródłowy biblioteki do tworzenia paczki archiwalnej [ABCPRO.NES.ArchivalPackage](Abc.Nes.ArchivalPackage). Za pomocą biblioteki można tworzyć, edytować i zapisywać dokumenty i metadane w paczce archiwalnej zgodnie z wymogami rozporządzeń.

 ## NuGet

<a href="https://www.nuget.org/packages/ABCPRO.NES/"><img alt="Nuget" src="https://img.shields.io/nuget/v/ABCPRO.NES?label=abcpro.nes%20nuget"> <img alt="Nuget" src="https://img.shields.io/nuget/dt/ABCPRO.NES"></a>

<a href="https://www.nuget.org/packages/ABCPRO.NES.ArchivalPackage/"><img alt="Nuget" src="https://img.shields.io/nuget/v/ABCPRO.NES.ArchivalPackage?label=abcpro.nes.archivalpackage%20nuget"> <img alt="Nuget" src="https://img.shields.io/nuget/dt/ABCPRO.NES.ArchivalPackage"></a>

Nazwa | Wersja | Opis
------|--------|--------
ABCPRO.NES.ArchivalPackage|1.0.5|Aktualizacja zależności.
ABCPRO.NES|1.0.5|Zawiera dodatkowe pola w adresie (gmina, powiat, województwo). Dodane metody klasyczne do pobiearania wartości z Enumeratorów dla pól tekstowych np. `RelationElement.GetRelationType`.
ABCPRO.NES.ArchivalPackage|1.0.4|Pierwsza stabilna wersja biblioteki. Pozwala na tworzenie paczki archiwalnej z wymaganymi katalogami, dodawanie plików oraz metadanych utworzonych za pomocą ABCPRO.NES, zapis i odczyt wcześniej utworzonych paczek.
ABCPRO.NES|1.0.3|Pierwsza stabilna wersja biblioteki. Pozwala na dodawanie wszystkich metadanych, walidację i zapis do pliku XML. Za pomocą klasy `XmlConverter` możliwe jest otwieranie plików zarówno z wersji 2.0 jak i tych starszych.


## Przykłady użycia

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

#### Tworzenie dokumentu XML

```C#
            Abc.Nes.Document document = GetModel(); 
            var filePath = Path.Combine(Path.GetTempPath(), "nes.xml");
            new Abc.Nes.Converters.XmlConverter().WriteXml(document, filePath);
```

#### Ładowanie dokumentu XML

```C#
            var filePath = Path.Combine(Path.GetTempPath(), "nes.xml");
            if (!File.Exists(filePath)) { throw new FileNotFoundException(); }
            var document = new Abc.Nes.Converters.XmlConverter().LoadXml(filePath);
```

#### Walidacja dokumentu XML

```C#
            var filePath = Path.Combine(Path.GetTempPath(), "nes.xml");
            if (!File.Exists(filePath)) { throw new FileNotFoundException(); }
            var converter = new Abc.Nes.Converters.XmlConverter();
            var valid = converter.Validate(filePath);
            // converter.ValidationErrors - errors
```

#### Korzystanie z modelu

```C#
            var document = new Abc.Nes.Document() {
                Identifiers = new List<Abc.Nes.Elements.IdentifierElement> {
                    new Abc.Nes.Elements.IdentifierElement() {
                        Type = "Znak sprawy",
                        Value = "ABC-A.123.77.3.2011.JW.",
                        Subject = new Elements.SubjectElement(){
                            Institution = new Elements.InstitutionElement(){
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
                        Size = new Elements.SizeElement(){
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
                        Kinds = new List<string> { 
                            Elements.TypeElement.GetDocumentKindType(Enumerations.DocumentKindType.Document) 
                        }
                    }
                },
                Groupings = new List<Elements.GroupingElement> {
                    new Elements.GroupingElement() {
                        Type = "Rejestr korespondencji przychodzącej",
                        Code = "RKP01",
                        Description = "tekstowy opis grupy"
                    }
                },
                Description = "Opis dokumentu"
            };
```
### ABCPRO.NES.ArchivalPackage

#### Ładowanie paczki archiwalnej

``` C#
            var path = @"../../../sample/ValidatedPackage.zip";
            var mgr = new PackageManager();
            mgr.LoadPackage(path);
            var isNotEmpty = mgr != null && mgr.Package != null && !mgr.Package.IsEmpty;
```            

#### Pobieranie informacji o ilości dokumentów w paczce archiwalnej 

```C#
            var path = @"../../../sample/ValidatedPackage.zip";
            var mgr = new PackageManager();
            mgr.LoadPackage(path);
            var count = mgr.GetDocumentsCount();
```

#### Ładowanie, dodanie pliku z metadanymi i zapisanie paczki archiwalnej 

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

## Historia

Historia niezbędnych elementów dokumentów elektronicznych rozpoczęła się 30 października 2006 roku wraz z wydaniem rozporządzenia Ministra Spraw Wewnętrznych i Administracji w sprawie niezbędnych elementów struktury dokumentów elektronicznych [(Dz.U. Nr 206, poz. 1517)](https://eli.gov.pl/eli/DU/2006/1517/ogl). 

NES łaczą się bezpośrednio z Ustawą o narodowym zasobie archiwalnym i archiwach  [(Dz.U. z 2019 r. poz. 553)](https://eli.gov.pl/eli/DU/1983/173/ogl), Rozporządzeniem Ministra Spraw Wewnętrznych i Administracji w sprawie wymagań technicznych formatów zapisu i informatycznych nośników danych, na których utrwalono materiały archiwalne przekazywane do archiwów państwowych  [(Dz.U. Nr 206, poz. 1519)](https://eli.gov.pl/eli/DU/2006/1519/ogl) i Rozporządzeniem Prezydenta Rzeczypospolitej Polskiej w sprawie szczegółowego sposobu oraz szczegółowych warunków przekazywania skargi wraz z aktami sprawy i odpowiedzią na skargę do sądu administracyjnego  [(Dz.U. z 2019 r. poz. 1003)](https://eli.gov.pl/eli/DU/2019/1003/ogl)

<a href="https://www.abcpro.pl"><img alt="www" src="https://img.shields.io/badge/www-abcpro.pl-orange?style=for-the-badge"></a> 
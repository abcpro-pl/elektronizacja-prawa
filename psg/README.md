![Image](images/psg_logo.jpg) 

# Generator losowych zdań po polsku

Biblioteka służy do:

1. Tworzenia tekstów Lorem Ipsum,
2. Tworzenia tekstów złożonych z polskich przysłów i wersetów z Biblii,
3. Tworzenia losowych zdań na podstawie słownika języka polskiego.  

 ## NuGet

<a href="https://www.nuget.org/packages/ABCPRO.PolishSentencesGenerator/"><img alt="Nuget" src="https://img.shields.io/nuget/v/ABCPRO.PolishSentencesGenerator?label=abcpro.polishsentencesgenerator%20nuget"> <img alt="Nuget" src="https://img.shields.io/nuget/dt/ABCPRO.PolishSentencesGenerator"></a>

## Przykłady

### Tworzenia tekstów złożonych z polskich przysłów 

``` C#
var minSentences = 1;
var maxSentences = 5;
var numParagraphs = 1;
var useHtml = false;
var whitoutNewLine = false;
var result = Proverbs.Get(minSentences, maxSentences, numParagraphs, useHtml, whitoutNewLine);
```

### Tworzenia losowych zdań na podstawie słownika języka polskiego

```C#
var minWords = 5;
var maxWords = 20;
var minSentences = 1;
var maxSentences = 10;
var numParagraphs = 1;
var useHtml = false;
var whitoutNewLine = false;
var result = Generator.Get(minWords, maxWords, 
    minSentences, maxSentences, numParagraphs, useHtml, whitoutNewLine);           
```

### Tworzenia tekstów Lorem Ipsum

```C#
var minWords = 5;
var maxWords = 20;
var minSentences = 1;
var maxSentences = 10;
var numParagraphs = 1;
var useHtml = false;
var whitoutNewLine = false;
var result = LoremIpsum.Get(minWords, maxWords, 
    minSentences, maxSentences, numParagraphs, useHtml, whitoutNewLine);
```


<a href="https://www.abcpro.pl"><img alt="www" src="https://img.shields.io/badge/www-abcpro.pl-orange?style=for-the-badge"></a> 
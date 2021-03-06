![Image](images/eli_brand_invert.svg) 
# Europejski Identyfikator Prawodawstwa

<a href="http://eli.gov.pl"><img alt="WWW" src="https://img.shields.io/badge/ELI-Dziennik Ustaw i Monitor Polski-black"></a>

<a href="http://e-dziennik.msw.gov.pl/eli"><img alt="WWW" src="https://img.shields.io/badge/ELI-DUM_MSW-darkblue"></a>
<a href="http://edziennik.mswia.gov.pl/eli"><img alt="WWW" src="https://img.shields.io/badge/ELI-DUM_MSWiA-darkblue"></a>
<a href="http://e-dziennik.mac.gov.pl/eli"><img alt="WWW" src="https://img.shields.io/badge/ELI-DUM_MAC-darkblue"></a>
<a href="http://e-dziennik.msport.gov.pl/eli"><img alt="WWW" src="https://img.shields.io/badge/ELI-DUM_MSiT-darkblue"></a>


<a href="https://edu.cba.gov.pl/eli"><img alt="WWW" src="https://img.shields.io/badge/ELI-DU_CBA-darkblue"></a>
<a href="https://dzu.nbp.pl/eli"><img alt="WWW" src="https://img.shields.io/badge/ELI-DU_NBP -darkgreen"></a>
<a href="http://edziennik.uke.gov.pl/eli"><img alt="WWW" src="https://img.shields.io/badge/ELI-DU_UKE-darkblue"></a>
<a href="http://dziennik.urpl.gov.pl/eli"><img alt="WWW" src="https://img.shields.io/badge/ELI-DU_RPL-darkblue"></a>
<a href="http://edziennik.kgpsp.gov.pl/eli"><img alt="WWW" src="https://img.shields.io/badge/ELI-DU_PSP -darkblue"></a>
<a href="http://edziennik.sop.gov.pl/eli/"><img alt="WWW" src="https://img.shields.io/badge/ELI-DU_SOP -darkblue"></a>


<a href="https://edzienniki.duw.pl/eli/"><img alt="WWW" src="https://img.shields.io/badge/ELI-POL_WOJ_DS-darkred"></a>
<a href="http://www.edzienniki.bydgoszcz.uw.gov.pl/eli/"><img alt="WWW" src="https://img.shields.io/badge/ELI-POL_WOJ_KP-darkred"></a>
<a href="http://edziennik.lublin.uw.gov.pl/eli/"><img alt="WWW" src="https://img.shields.io/badge/ELI-POL_WOJ_LB-darkred"></a>
<a href="http://dzienniki.luw.pl/eli/"><img alt="WWW" src="https://img.shields.io/badge/ELI-POL_WOJ_LS-darkred"></a>
<a href="http://dziennik.lodzkie.eu/eli/"><img alt="WWW" src="https://img.shields.io/badge/ELI-POL_WOJ_LD-darkred"></a>

<a href="http://edziennik.malopolska.uw.gov.pl/eli/"><img alt="WWW" src="https://img.shields.io/badge/ELI-POL_WOJ_MP-darkred"></a>
<a href="http://edziennik.mazowieckie.pl/eli/"><img alt="WWW" src="https://img.shields.io/badge/ELI-POL_WOJ_MZ-darkred"></a>
<a href="https://duwo.opole.uw.gov.pl/eli/"><img alt="WWW" src="https://img.shields.io/badge/ELI-POL_WOJ_OP-darkred"></a>
<a href="http://edziennik.rzeszow.uw.gov.pl/eli/"><img alt="WWW" src="https://img.shields.io/badge/ELI-POL_WOJ_PK-darkred"></a>
<a href="http://edziennik.bialystok.uw.gov.pl/eli/"><img alt="WWW" src="https://img.shields.io/badge/ELI-POL_WOJ_PL-darkred"></a>

<a href="http://edziennik.gdansk.uw.gov.pl/eli/"><img alt="WWW" src="https://img.shields.io/badge/ELI-POL_WOJ_PM-darkred"></a>
<a href="http://dzienniki.slask.eu/eli"><img alt="WWW" src="https://img.shields.io/badge/ELI-POL_WOJ_SL-darkred"></a>
<a href="http://edziennik.kielce.uw.gov.pl/eli/"><img alt="WWW" src="https://img.shields.io/badge/ELI-POL_WOJ_SK-darkred"></a>
<a href="http://edzienniki.olsztyn.uw.gov.pl/eli/"><img alt="WWW" src="https://img.shields.io/badge/ELI-POL_WOJ_WM-darkred"></a>
<a href="http://edziennik.poznan.uw.gov.pl/eli/"><img alt="WWW" src="https://img.shields.io/badge/ELI-POL_WOJ_WP-darkred"></a>
<a href="http://e-dziennik.szczecin.uw.gov.pl/eli/"><img alt="WWW" src="https://img.shields.io/badge/ELI-POL_WOJ_ZP-darkred"></a>

W tym folderze przechowywane są :
1. Słowniki (ontology) utworzone przez Dyrektora Jarosława Demineta z Rządowego Centrum Legislacji.

    * Resource-types – rodzaje aktów
    * ATU_PL – jednostki podziału administracyjnego (województwa, powiaty, miasta na prawach powiatu, gminy)
    * Legal-institutions_Central – organy centralne
    * Legal-institutions_WOJ – organy administracji zespolonej i inne wojewódzkie
    * Legal-institutions_POW – organy powiatowe
    * Legal-institutions_MPP – organy miast na prawach powiatu
    * Legal-institutions_GM – organy gminne
    * Legal-institutions_Inne – inne organy terenowe (komisarze wyborczy, prezesi sądów, dyrektorzy urzędów, organy związków międzygminnych i metropolitalnych)


2. Schematy XML aktów prawnych programu Legislator 3.x wykorzystujące przestrzenie nazw ELI dla dokumentów. W przeciwieństwie do EDAP, schematy dokumentów dostosowane są do faktycznie występującej zawartości, są więc bardziej restrykcyjne. 
3. Kod źródłowy C# klienta ELI  do pobierania danych z Wojewódzkich Dzienników Urzędowych oraz niektórych Dzienników Ministerstw i Urzedów Centralnych. W celu wygenerowania klienta dla innego języka programowania, skorzystaj ze [Swagger CodeGen](https://swagger.io/tools/swagger-codegen/) w generatorze wskaż url do dokumentacji np. http://dziennik.lodzkie.eu/api/eli/openapi/ui/v1/elijson 

    Od 2020 aplikacja Elektroniczne Dzienniki Urzedowe posiada podwitrynę ELI http://{adres-dziennika}/eli podobnie jak Dziennik Ustaw i Monitor Polski https://eli.gov.pl/eli

Koncepcja identyfikatora została opisana w dokumencie Konkluzje Rady zalecające wprowadzenie europejskiego identyfikatora prawodawstwa (European Legislation Identifier – ELI) (2012/C 325/02) oraz dokumencie Konkluzje Rady w sprawie europejskiego identyfikatora prawodawstwa (2017/C 441/05). Koncepcja zakłada stworzenie wspólnego systemu identyfikowania prawodawstwa i jego metadanych, który uzupełni systemy informacji prawnej państw członkowskich. Wprowadzany przez koncepcję nowy standard dostępu do informacji prawnej zakłada, że sposób przygotowywania dzienników urzędowych i biuletynów prawnych w dalszym ciągu pozostawiony będzie w gestii państw wdrażających ELI.

## Struktura stosowanego szablonu URI (filar 1)

URI składa się z części stałej i części zmiennej:

1. Część stała: http://dziennik.lodzkie.eu/eli/
2. Część zmienna:
  
    * { dziennik } / { rok } / { pozycja } / { ogl } / pol / { pdf | html }
        
        * { dziennik } - symbol dziennika, np.: POL_WOJ_LD - Wojewoda Łódzki
        * { rok } - rok w zakresie od 2009 do 2020
        * { pozycja } - pozycja w danym dzienniku
        * ogl - metryczka ogłoszonego aktu,
        * pol - tekst w języku polskim
        * pdf - tekst aktu w formacie PDF
        * html - tekst aktu w formacie HTML

## Korzyści wynikające z wdrożenia ELI

1. Wdrożenie ELI będzie się wiązać ze zwiększeniem interoperacyjności systemów informacyjnych państw i instytucji europejskich i łatwiejszym dostępem do informacji prawnych opublikowanych w krajowych, europejskich i światowych systemach informacji prawnych.
2. Zarówno obywatele i przedsiębiorcy, jak i administracja, będą mogli zapoznać się z prawodawstwem krajowym i unijnym w sposób szybszy, bardziej otwarty, bezpośredni i przejrzysty.
3. Ułatwiona zostanie również wymiana informacji prawnych pomiędzy państwami członkowskimi i zwiększone zostaną możliwości ponownego wykorzystania zgromadzonych danych.
4. Umożliwienie obywatelom i osobom wykonującym zawody prawnicze efektywniejsze przeprowadzanie kwerendy ustawodawstwa w różnych systemach prawnych.
5. Poprawienie skuteczności procedur mających zastosowanie do publikacji informacji prawnych, co prowadzi do podniesienia jakości, większej wiarygodności prawodawstwa oraz do zmniejszania kosztów.
6. Umożliwienie ponownego inteligentnego wykorzystanie danych prawnych i stworzenie możliwości rozwijania nowych usług przez sektor prywatny, przyczyniając się tym samym do rozwoju jednolitego rynku cyfrowego.
7. Ułatwione zostanie wykonanie Rozporządzenia Parlamentu Europejskiego i Rady (UE) 2018/1724 z dnia 2 października 2018 r. w sprawie utworzenia jednolitego portalu cyfrowego w celu zapewnienia dostępu do informacji, procedur oraz usług wsparcia i rozwiązywania problemów, a także zmieniającego rozporządzenie (UE) nr 1024/2012. Zgodnie z art. 12 UE zapewni tłumaczenie wybranych informacji na inne języki UE. Rozporządzenie nie wymienia tu wprawdzie wyraźnie aktów prawnych, ale w niektórych przypadkach przytoczenie tłumaczenia takich aktów byłoby bardzo użyteczne (np. Prawo zamówień publicznych, Prawo przedsiębiorców). Identyfikatory ELI pozwalają łatwo opisać różne wersje językowe aktu prawnego (np. http://dane.gov.pl/eli/DU/2018/1000/ogl/eng), przy czym w metadanych można zapisać, że ta wersja tekstu nie ma charakteru autentycznego.
8. Odpowiednie uzupełnienie metadanych ułatwi także wykonywanie obowiązków sprawozdawczych. Przykładowo zgodnie z art. 70 ustawy z dnia 6 marca 2018 r. - Prawo przedsiębiorców ministrowie kierujący działami administracji rządowej dokonują, w zakresie swojej właściwości, bieżącego przeglądu funkcjonowania aktów normatywnych określających zasady podejmowania, wykonywania lub zakończenia działalności gospodarczej oraz corocznie przedkładają Radzie Ministrów informację o działaniach podjętych w poprzednim roku kalendarzowym w wyniku dokonania tego przeglądu. Wpisanie do metadanych aktów prawnych informacji o dziale administracji rządowej oraz dodatkowego znacznika określającego, że akt dotyczy podejmowania, wykonywania lub zakończenia działalności gospodarczej, pozwoliłoby zautomatyzować proces wybierania obowiązujących aktów spełniających te kryteria należących do właściwości poszczególnych ministrów.
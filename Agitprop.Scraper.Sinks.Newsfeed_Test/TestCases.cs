
using System.Text.Json.Serialization;

using Agitprop.Core;
using Agitprop.Core.Enums;

namespace Agitprop.Scraper.Sinks.Newsfeed_Test;
public partial class ContentParserTests
{

    public static IEnumerable<TestCase> AlfahirTestCases()
    {
        return new List<TestCase>
        {
            new TestCase
            {
                HtmlPath = "TestData/alfahir/1.html",
                ExpectedContent = new ContentParserResult
                {

                    SourceSite = NewsSites.Alfahir,
                    PublishDate = DateTime.Parse("2025-01-08T16:01:05Z"),
                    Text = """Ezúttal egy 85 éves bácsit vertek félholtra az otthonában Egyre gyakoribb, hogy idős emberekre rontanak rá a házukban. Bács-Kiskun Vármegyei Főügyészség életveszélyt okozó testi sértés és minősített rablás bűntette miatt vádat emelt egy testvérpárral szemben. Az elkövetők az otthonában rátörtek a 85 éves férfira, brutálisan bántalmazták, elvették 400.000 forintját, majd életveszélyes sérülésekkel magára hagyták - írja az Ügyészség honlapja. A vádirat szerint a kiskunhalasi testvérpár – a büntetett előéletű fiatalabb testvér felvetése alapján – 2023. szeptember 22-én, kilátástalan anyagi helyzetük megoldása érdekében, elhatározták, hogy betörnek az idős, nehézkesen mozgó sértetthez. A testvérek átmásztak a kerítésen, majd először a nyitott melléképületekben néztek szét, de mivel ott számukra értékkel bíró tárgyat nem találtak, a kutatást a házban folytatták. Ennek zajára felébredt a sértett, nem ijedt meg a vádlottaktól, kérdőre vonta őket. Válaszul a testvérek odaléptek a sértett ágyához és ököllel ütlegelni kezdték az idős férfit, aki az ágyról is leesett és ugyan fel tudott állni, de a váratlan, sorozatos, nagy erejű ütésekkel szemben nem tudott védekezni. A testvérek ellökték a sértettet, aki ismét elesett és beütötte a fejét a járólapba. Ezután az elkövetők letépték a sértett karján lévő gondosórát, elvették a telefonját. Átkutatták a házat, a sértett pénztárcájából kivettek 400.000 forintot, majd távoztak. A bántalmazás következtében az idős áldozat súlyos, életveszélyes sérüléseket szenvedett, életét az idejében érkezett orvosi ellátás és kórházi gyógykezelés mentette meg. A főügyészség a letartóztatásban lévő testvéreket életveszélyt okozó testi sértés bűntettével és a bűncselekmény felismerésére vagy elhárítására idős koránál fogva korlátozottan képes személy sérelmére elkövetett rablás bűntettével vádolja és velük szemben szabadságvesztés büntetés kiszabását indítványozta. Fuldoklik a vidék a bűn áradatában Az elmúlt években növekedett az erőszakos bűncselekmények száma, a korábbiakhoz képest szokatlanul gyakran érkezik hír hasonló brutális támadásról. Novemberben egy 85 éves veszprémi nénit gyilkoltak meg saját otthonában. Augusztusban Pécsen egy 94 éves embert öltek meg, majd rágyújtották a házát. Ugyanebben a hónapban Öcsödön egy 16 éves fiú 10 éves öccse szeme láttára vert agyon egy 85 éves bácsit, Jánoshalmán, egy 80 éves néni életét oltották ki a saját házában. A pénz az öcsödi és jánoshalmai gyilkosnak is kábítószerre kellett. Mindkét áldozat házát kamera figyelte. Az elkövetők valószínűleg tisztában voltak ezzel, ám ez sem tartotta vissza őket. Az Országgyűlésben Lukács László, a Jobbik frakcióvezetője október 7-i felszólalásában szembesítette a kormányt "a bűnözés és a kábítószer járványszerű terjedésével". A "vidék kormányának" 14-ik évében éppen a leszakadó, szegényebb vidéki régiók fuldoklanak a bűnözés áradatában" - mutatott rá a politikus. (Kép: Pxhere)""",
                }
            },
        };
    }
    public static IEnumerable<TestCase> HvgTestCases()
    {
        return new List<TestCase>
        {
            new TestCase
            {
                HtmlPath = "TestData/hvg/1.html",
                ExpectedContent = new ContentParserResult
                {
                    SourceSite = NewsSites.HVG,
                    PublishDate = DateTime.Parse("2025-01-11T18:14:30.0000000+01:00"),
                    Text = """Két villamos futott egymásba Strasbourgban, ötven ember sérült meg Rossz vágányra került át egy villamos Strasbourgban, és összeütközött egy másik szerelvénnyel. Összeütközött két villamos Strasbourgban, egy alagútban, nagyjából ötven ember sérült meg, közülük 15-öt kellett kórházba szállítani. A helyszínen ijesztő fényképek és videók készültek, de a Le Figaro a helyi tűzoltó- és mentőszolgálat szóvivőjére hivatkozva arról ír, hogy súlyos sérülés nem történt.ALERTE – Collision entre deux tramways dans le tunnel de la gare de #Strasbourg. Des dizaines de blessés. Les secours affluent en nombre sur place. (@Samstark75 / France 3) pic.twitter.com/oATObaIWbj— Infos Françaises (@InfosFrancaises) January 11, 2025 A BFMTV első információja szerint egy villamos rossz vágányra került, ahol nekiütközött egy ott álló járműnek. Jeanne Barseghian polgármester azt nyilatkozta: az állomáson még mindig több olyan sérültet ápolnak, akiket nem az elsők között vittek a kórházba.""",
                }
            },
        };
    }
    public static IEnumerable<TestCase> IndexTestCases()
    {
        return new List<TestCase>
        {
            new TestCase
            {
                HtmlPath = "TestData/index/1.html",
                ExpectedContent = new ContentParserResult
                {
                    SourceSite = NewsSites.Index,
                    PublishDate = DateTime.Parse("2025-01-11T21:48:00+01:00"),
                    Text = """Ezúttal egy 85 éves bácsit vertek félholtra az otthonában Egyre gyakoribb, hogy idős emberekre rontanak rá a házukban. Bács-Kiskun Vármegyei Főügyészség életveszélyt okozó testi sértés és minősített rablás bűntette miatt vádat emelt egy testvérpárral szemben. Az elkövetők az otthonában rátörtek a 85 éves férfira, brutálisan bántalmazták, elvették 400.000 forintját, majd életveszélyes sérülésekkel magára hagyták - írja az Ügyészség honlapja. A vádirat szerint a kiskunhalasi testvérpár – a büntetett előéletű fiatalabb testvér felvetése alapján – 2023. szeptember 22-én, kilátástalan anyagi helyzetük megoldása érdekében, elhatározták, hogy betörnek az idős, nehézkesen mozgó sértetthez. A testvérek átmásztak a kerítésen, majd először a nyitott melléképületekben néztek szét, de mivel ott számukra értékkel bíró tárgyat nem találtak, a kutatást a házban folytatták. Ennek zajára felébredt a sértett, nem ijedt meg a vádlottaktól, kérdőre vonta őket. Válaszul a testvérek odaléptek a sértett ágyához és ököllel ütlegelni kezdték az idős férfit, aki az ágyról is leesett és ugyan fel tudott állni, de a váratlan, sorozatos, nagy erejű ütésekkel szemben nem tudott védekezni. A testvérek ellökték a sértettet, aki ismét elesett és beütötte a fejét a járólapba. Ezután az elkövetők letépték a sértett karján lévő gondosórát, elvették a telefonját. Átkutatták a házat, a sértett pénztárcájából kivettek 400.000 forintot, majd távoztak. A bántalmazás következtében az idős áldozat súlyos, életveszélyes sérüléseket szenvedett, életét az idejében érkezett orvosi ellátás és kórházi gyógykezelés mentette meg. A főügyészség a letartóztatásban lévő testvéreket életveszélyt okozó testi sértés bűntettével és a bűncselekmény felismerésére vagy elhárítására idős koránál fogva korlátozottan képes személy sérelmére elkövetett rablás bűntettével vádolja és velük szemben szabadságvesztés büntetés kiszabását indítványozta. Fuldoklik a vidék a bűn áradatában Az elmúlt években növekedett az erőszakos bűncselekmények száma, a korábbiakhoz képest szokatlanul gyakran érkezik hír hasonló brutális támadásról. Novemberben egy 85 éves veszprémi nénit gyilkoltak meg saját otthonában. Augusztusban Pécsen egy 94 éves embert öltek meg, majd rágyújtották a házát. Ugyanebben a hónapban Öcsödön egy 16 éves fiú 10 éves öccse szeme láttára vert agyon egy 85 éves bácsit, Jánoshalmán, egy 80 éves néni életét oltották ki a saját házában. A pénz az öcsödi és jánoshalmai gyilkosnak is kábítószerre kellett. Mindkét áldozat házát kamera figyelte. Az elkövetők valószínűleg tisztában voltak ezzel, ám ez sem tartotta vissza őket. Az Országgyűlésben Lukács László, a Jobbik frakcióvezetője október 7-i felszólalásában szembesítette a kormányt "a bűnözés és a kábítószer járványszerű terjedésével". A "vidék kormányának" 14-ik évében éppen a leszakadó, szegényebb vidéki régiók fuldoklanak a bűnözés áradatában" - mutatott rá a politikus. (Kép: Pxhere)""",
                }
            },
        };
    }

    public static IEnumerable<TestCase> KurucinfoTestCases()
    {
        return new List<TestCase>
        {
            new TestCase
            {
                HtmlPath = "TestData/kurucinfo/1.html",
                ExpectedContent = new ContentParserResult
                {
                    SourceSite = NewsSites.HVG,
                    PublishDate = DateTime.Parse("2025-01-11T18:14:30Z"),
                    Text = """Ezúttal egy 85 éves bácsit vertek félholtra az otthonában Egyre gyakoribb, hogy idős emberekre rontanak rá a házukban. Bács-Kiskun Vármegyei Főügyészség életveszélyt okozó testi sértés és minősített rablás bűntette miatt vádat emelt egy testvérpárral szemben. Az elkövetők az otthonában rátörtek a 85 éves férfira, brutálisan bántalmazták, elvették 400.000 forintját, majd életveszélyes sérülésekkel magára hagyták - írja az Ügyészség honlapja. A vádirat szerint a kiskunhalasi testvérpár – a büntetett előéletű fiatalabb testvér felvetése alapján – 2023. szeptember 22-én, kilátástalan anyagi helyzetük megoldása érdekében, elhatározták, hogy betörnek az idős, nehézkesen mozgó sértetthez. A testvérek átmásztak a kerítésen, majd először a nyitott melléképületekben néztek szét, de mivel ott számukra értékkel bíró tárgyat nem találtak, a kutatást a házban folytatták. Ennek zajára felébredt a sértett, nem ijedt meg a vádlottaktól, kérdőre vonta őket. Válaszul a testvérek odaléptek a sértett ágyához és ököllel ütlegelni kezdték az idős férfit, aki az ágyról is leesett és ugyan fel tudott állni, de a váratlan, sorozatos, nagy erejű ütésekkel szemben nem tudott védekezni. A testvérek ellökték a sértettet, aki ismét elesett és beütötte a fejét a járólapba. Ezután az elkövetők letépték a sértett karján lévő gondosórát, elvették a telefonját. Átkutatták a házat, a sértett pénztárcájából kivettek 400.000 forintot, majd távoztak. A bántalmazás következtében az idős áldozat súlyos, életveszélyes sérüléseket szenvedett, életét az idejében érkezett orvosi ellátás és kórházi gyógykezelés mentette meg. A főügyészség a letartóztatásban lévő testvéreket életveszélyt okozó testi sértés bűntettével és a bűncselekmény felismerésére vagy elhárítására idős koránál fogva korlátozottan képes személy sérelmére elkövetett rablás bűntettével vádolja és velük szemben szabadságvesztés büntetés kiszabását indítványozta. Fuldoklik a vidék a bűn áradatában Az elmúlt években növekedett az erőszakos bűncselekmények száma, a korábbiakhoz képest szokatlanul gyakran érkezik hír hasonló brutális támadásról. Novemberben egy 85 éves veszprémi nénit gyilkoltak meg saját otthonában. Augusztusban Pécsen egy 94 éves embert öltek meg, majd rágyújtották a házát. Ugyanebben a hónapban Öcsödön egy 16 éves fiú 10 éves öccse szeme láttára vert agyon egy 85 éves bácsit, Jánoshalmán, egy 80 éves néni életét oltották ki a saját házában. A pénz az öcsödi és jánoshalmai gyilkosnak is kábítószerre kellett. Mindkét áldozat házát kamera figyelte. Az elkövetők valószínűleg tisztában voltak ezzel, ám ez sem tartotta vissza őket. Az Országgyűlésben Lukács László, a Jobbik frakcióvezetője október 7-i felszólalásában szembesítette a kormányt "a bűnözés és a kábítószer járványszerű terjedésével". A "vidék kormányának" 14-ik évében éppen a leszakadó, szegényebb vidéki régiók fuldoklanak a bűnözés áradatában" - mutatott rá a politikus. (Kép: Pxhere)""",
                }
            },
        };
    }

    public static IEnumerable<TestCase> MagyarJelenTestCases()
    {
        return new List<TestCase>
        {
            new TestCase
            {
                HtmlPath = "TestData/magyarjelen/1.html",
                ExpectedContent = new ContentParserResult
                {
                    SourceSite = NewsSites.HVG,
                    PublishDate = DateTime.Parse("2025-01-11T18:14:30Z"),
                    Text = """Ezúttal egy 85 éves bácsit vertek félholtra az otthonában Egyre gyakoribb, hogy idős emberekre rontanak rá a házukban. Bács-Kiskun Vármegyei Főügyészség életveszélyt okozó testi sértés és minősített rablás bűntette miatt vádat emelt egy testvérpárral szemben. Az elkövetők az otthonában rátörtek a 85 éves férfira, brutálisan bántalmazták, elvették 400.000 forintját, majd életveszélyes sérülésekkel magára hagyták - írja az Ügyészség honlapja. A vádirat szerint a kiskunhalasi testvérpár – a büntetett előéletű fiatalabb testvér felvetése alapján – 2023. szeptember 22-én, kilátástalan anyagi helyzetük megoldása érdekében, elhatározták, hogy betörnek az idős, nehézkesen mozgó sértetthez. A testvérek átmásztak a kerítésen, majd először a nyitott melléképületekben néztek szét, de mivel ott számukra értékkel bíró tárgyat nem találtak, a kutatást a házban folytatták. Ennek zajára felébredt a sértett, nem ijedt meg a vádlottaktól, kérdőre vonta őket. Válaszul a testvérek odaléptek a sértett ágyához és ököllel ütlegelni kezdték az idős férfit, aki az ágyról is leesett és ugyan fel tudott állni, de a váratlan, sorozatos, nagy erejű ütésekkel szemben nem tudott védekezni. A testvérek ellökték a sértettet, aki ismét elesett és beütötte a fejét a járólapba. Ezután az elkövetők letépték a sértett karján lévő gondosórát, elvették a telefonját. Átkutatták a házat, a sértett pénztárcájából kivettek 400.000 forintot, majd távoztak. A bántalmazás következtében az idős áldozat súlyos, életveszélyes sérüléseket szenvedett, életét az idejében érkezett orvosi ellátás és kórházi gyógykezelés mentette meg. A főügyészség a letartóztatásban lévő testvéreket életveszélyt okozó testi sértés bűntettével és a bűncselekmény felismerésére vagy elhárítására idős koránál fogva korlátozottan képes személy sérelmére elkövetett rablás bűntettével vádolja és velük szemben szabadságvesztés büntetés kiszabását indítványozta. Fuldoklik a vidék a bűn áradatában Az elmúlt években növekedett az erőszakos bűncselekmények száma, a korábbiakhoz képest szokatlanul gyakran érkezik hír hasonló brutális támadásról. Novemberben egy 85 éves veszprémi nénit gyilkoltak meg saját otthonában. Augusztusban Pécsen egy 94 éves embert öltek meg, majd rágyújtották a házát. Ugyanebben a hónapban Öcsödön egy 16 éves fiú 10 éves öccse szeme láttára vert agyon egy 85 éves bácsit, Jánoshalmán, egy 80 éves néni életét oltották ki a saját házában. A pénz az öcsödi és jánoshalmai gyilkosnak is kábítószerre kellett. Mindkét áldozat házát kamera figyelte. Az elkövetők valószínűleg tisztában voltak ezzel, ám ez sem tartotta vissza őket. Az Országgyűlésben Lukács László, a Jobbik frakcióvezetője október 7-i felszólalásában szembesítette a kormányt "a bűnözés és a kábítószer járványszerű terjedésével". A "vidék kormányának" 14-ik évében éppen a leszakadó, szegényebb vidéki régiók fuldoklanak a bűnözés áradatában" - mutatott rá a politikus. (Kép: Pxhere)""",
                }
            },
        };
    }

    public static IEnumerable<TestCase> MagyarNemzetTestCases()
    {
        return new List<TestCase>
        {
            new TestCase
            {
                HtmlPath = "TestData/magyarnemzet/1.html",
                ExpectedContent = new ContentParserResult
                {
                    SourceSite = NewsSites.HVG,
                    PublishDate = DateTime.Parse("2025-01-11T18:14:30Z"),
                    Text = """Ezúttal egy 85 éves bácsit vertek félholtra az otthonában Egyre gyakoribb, hogy idős emberekre rontanak rá a házukban. Bács-Kiskun Vármegyei Főügyészség életveszélyt okozó testi sértés és minősített rablás bűntette miatt vádat emelt egy testvérpárral szemben. Az elkövetők az otthonában rátörtek a 85 éves férfira, brutálisan bántalmazták, elvették 400.000 forintját, majd életveszélyes sérülésekkel magára hagyták - írja az Ügyészség honlapja. A vádirat szerint a kiskunhalasi testvérpár – a büntetett előéletű fiatalabb testvér felvetése alapján – 2023. szeptember 22-én, kilátástalan anyagi helyzetük megoldása érdekében, elhatározták, hogy betörnek az idős, nehézkesen mozgó sértetthez. A testvérek átmásztak a kerítésen, majd először a nyitott melléképületekben néztek szét, de mivel ott számukra értékkel bíró tárgyat nem találtak, a kutatást a házban folytatták. Ennek zajára felébredt a sértett, nem ijedt meg a vádlottaktól, kérdőre vonta őket. Válaszul a testvérek odaléptek a sértett ágyához és ököllel ütlegelni kezdték az idős férfit, aki az ágyról is leesett és ugyan fel tudott állni, de a váratlan, sorozatos, nagy erejű ütésekkel szemben nem tudott védekezni. A testvérek ellökték a sértettet, aki ismét elesett és beütötte a fejét a járólapba. Ezután az elkövetők letépték a sértett karján lévő gondosórát, elvették a telefonját. Átkutatták a házat, a sértett pénztárcájából kivettek 400.000 forintot, majd távoztak. A bántalmazás következtében az idős áldozat súlyos, életveszélyes sérüléseket szenvedett, életét az idejében érkezett orvosi ellátás és kórházi gyógykezelés mentette meg. A főügyészség a letartóztatásban lévő testvéreket életveszélyt okozó testi sértés bűntettével és a bűncselekmény felismerésére vagy elhárítására idős koránál fogva korlátozottan képes személy sérelmére elkövetett rablás bűntettével vádolja és velük szemben szabadságvesztés büntetés kiszabását indítványozta. Fuldoklik a vidék a bűn áradatában Az elmúlt években növekedett az erőszakos bűncselekmények száma, a korábbiakhoz képest szokatlanul gyakran érkezik hír hasonló brutális támadásról. Novemberben egy 85 éves veszprémi nénit gyilkoltak meg saját otthonában. Augusztusban Pécsen egy 94 éves embert öltek meg, majd rágyújtották a házát. Ugyanebben a hónapban Öcsödön egy 16 éves fiú 10 éves öccse szeme láttára vert agyon egy 85 éves bácsit, Jánoshalmán, egy 80 éves néni életét oltották ki a saját házában. A pénz az öcsödi és jánoshalmai gyilkosnak is kábítószerre kellett. Mindkét áldozat házát kamera figyelte. Az elkövetők valószínűleg tisztában voltak ezzel, ám ez sem tartotta vissza őket. Az Országgyűlésben Lukács László, a Jobbik frakcióvezetője október 7-i felszólalásában szembesítette a kormányt "a bűnözés és a kábítószer járványszerű terjedésével". A "vidék kormányának" 14-ik évében éppen a leszakadó, szegényebb vidéki régiók fuldoklanak a bűnözés áradatában" - mutatott rá a politikus. (Kép: Pxhere)""",
                }
            },
        };
    }
    public static IEnumerable<TestCase> MandinerTestCases()
    {
        return new List<TestCase>
        {
            new TestCase
            {
                HtmlPath = "TestData/mandiner/1.html",
                ExpectedContent = new ContentParserResult
                {
                    SourceSite = NewsSites.HVG,
                    PublishDate = DateTime.Parse("2025-01-11T18:14:30Z"),
                    Text = """Ezúttal egy 85 éves bácsit vertek félholtra az otthonában Egyre gyakoribb, hogy idős emberekre rontanak rá a házukban. Bács-Kiskun Vármegyei Főügyészség életveszélyt okozó testi sértés és minősített rablás bűntette miatt vádat emelt egy testvérpárral szemben. Az elkövetők az otthonában rátörtek a 85 éves férfira, brutálisan bántalmazták, elvették 400.000 forintját, majd életveszélyes sérülésekkel magára hagyták - írja az Ügyészség honlapja. A vádirat szerint a kiskunhalasi testvérpár – a büntetett előéletű fiatalabb testvér felvetése alapján – 2023. szeptember 22-én, kilátástalan anyagi helyzetük megoldása érdekében, elhatározták, hogy betörnek az idős, nehézkesen mozgó sértetthez. A testvérek átmásztak a kerítésen, majd először a nyitott melléképületekben néztek szét, de mivel ott számukra értékkel bíró tárgyat nem találtak, a kutatást a házban folytatták. Ennek zajára felébredt a sértett, nem ijedt meg a vádlottaktól, kérdőre vonta őket. Válaszul a testvérek odaléptek a sértett ágyához és ököllel ütlegelni kezdték az idős férfit, aki az ágyról is leesett és ugyan fel tudott állni, de a váratlan, sorozatos, nagy erejű ütésekkel szemben nem tudott védekezni. A testvérek ellökték a sértettet, aki ismét elesett és beütötte a fejét a járólapba. Ezután az elkövetők letépték a sértett karján lévő gondosórát, elvették a telefonját. Átkutatták a házat, a sértett pénztárcájából kivettek 400.000 forintot, majd távoztak. A bántalmazás következtében az idős áldozat súlyos, életveszélyes sérüléseket szenvedett, életét az idejében érkezett orvosi ellátás és kórházi gyógykezelés mentette meg. A főügyészség a letartóztatásban lévő testvéreket életveszélyt okozó testi sértés bűntettével és a bűncselekmény felismerésére vagy elhárítására idős koránál fogva korlátozottan képes személy sérelmére elkövetett rablás bűntettével vádolja és velük szemben szabadságvesztés büntetés kiszabását indítványozta. Fuldoklik a vidék a bűn áradatában Az elmúlt években növekedett az erőszakos bűncselekmények száma, a korábbiakhoz képest szokatlanul gyakran érkezik hír hasonló brutális támadásról. Novemberben egy 85 éves veszprémi nénit gyilkoltak meg saját otthonában. Augusztusban Pécsen egy 94 éves embert öltek meg, majd rágyújtották a házát. Ugyanebben a hónapban Öcsödön egy 16 éves fiú 10 éves öccse szeme láttára vert agyon egy 85 éves bácsit, Jánoshalmán, egy 80 éves néni életét oltották ki a saját házában. A pénz az öcsödi és jánoshalmai gyilkosnak is kábítószerre kellett. Mindkét áldozat házát kamera figyelte. Az elkövetők valószínűleg tisztában voltak ezzel, ám ez sem tartotta vissza őket. Az Országgyűlésben Lukács László, a Jobbik frakcióvezetője október 7-i felszólalásában szembesítette a kormányt "a bűnözés és a kábítószer járványszerű terjedésével". A "vidék kormányának" 14-ik évében éppen a leszakadó, szegényebb vidéki régiók fuldoklanak a bűnözés áradatában" - mutatott rá a politikus. (Kép: Pxhere)""",
                }
            },
        };
    }

    public static IEnumerable<TestCase> MerceTestCases()
    {
        return new List<TestCase>
        {
            new TestCase
            {
                HtmlPath = "TestData/merce/1.html",
                ExpectedContent = new ContentParserResult
                {
                    SourceSite = NewsSites.HVG,
                    PublishDate = DateTime.Parse("2025-01-11T18:14:30Z"),
                    Text = """Ezúttal egy 85 éves b"""
                }
            }
        };
    }

    public static IEnumerable<TestCase> MetropolTestCases()
    {
        return new List<TestCase>
        {
            new TestCase
            {
                HtmlPath = "TestData/metropol/1.html",
                ExpectedContent = new ContentParserResult
                {
                    SourceSite = NewsSites.HVG,
                    PublishDate = DateTime.Parse("2025-01-11T18:14:30Z"),
                    Text = """Ezúttal egy 85 éves bácsit vertek félholtra az otthonában Egyre gyakoribb, hogy idős emberekre rontanak rá a házukban. Bács-Kiskun Vármegyei Főügyészség életveszélyt okozó testi sértés és minősített rablás bűntette miatt vádat emelt egy testvérpárral szemben. Az elkövetők az otthonában rátörtek a 85 éves férfira, brutálisan bántalmazták, elvették 400.000 forintját, majd életveszélyes sérülésekkel magára hagyták - írja az Ügyészség honlapja. A vádirat szerint a kiskunhalasi testvérpár – a büntetett előéletű fiatalabb testvér felvetése alapján – 2023. szeptember 22-én, kilátástalan anyagi helyzetük megoldása érdekében, elhatározták, hogy betörnek az idős, nehézkesen mozgó sértetthez. A testvérek átmásztak a kerítésen, majd először a nyitott melléképületekben néztek szét, de mivel ott számukra értékkel bíró tárgyat nem találtak, a kutatást a házban folytatták. Ennek zajára felébredt a sértett, nem ijedt meg a vádlottaktól, kérdőre vonta őket. Válaszul a testvérek odaléptek a sértett ágyához és ököllel ütlegelni kezdték az idős férfit, aki az ágyról is leesett és ugyan fel tudott állni, de a váratlan, sorozatos, nagy erejű ütésekkel szemben nem tudott védekezni. A testvérek ellökték a sértettet, aki ismét elesett és beütötte a fejét a járólapba. Ezután az elkövetők letépték a sértett karján lévő gondosórát, elvették a telefonját. Átkutatták a házat, a sértett pénztárcájából kivettek 400.000 forintot, majd távoztak. A bántalmazás következtében az idős áldozat súlyos, életveszélyes sérüléseket szenvedett, életét az idejében érkezett orvosi ellátás és kórházi gyógykezelés mentette meg. A főügyészség a letartóztatásban lévő testvéreket életveszélyt okozó testi sértés bűntettével és a bűncselekmény felismerésére vagy elhárítására idős koránál fogva korlátozottan képes személy sérelmére elkövetett rablás bűntettével vádolja és velük szemben szabadságvesztés büntetés kiszabását indítványozta. Fuldoklik a vidék a bűn áradatában Az elmúlt években növekedett az erőszakos bűncselekmények száma, a korábbiakhoz képest szokatlanul gyakran érkezik hír hasonló brutális támadásról. Novemberben egy 85 éves veszprémi nénit gyilkoltak meg saját otthonában. Augusztusban Pécsen egy 94 éves embert öltek meg, majd rágyújtották a házát. Ugyanebben a hónapban Öcsödön egy 16 éves fiú 10 éves öccse szeme láttára vert agyon egy 85 éves bácsit, Jánoshalmán, egy 80 éves néni életét oltották ki a saját házában. A pénz az öcsödi és jánoshalmai gyilkosnak is kábítószerre kellett. Mindkét áldozat házát kamera figyelte. Az elkövetők valószínűleg tisztában voltak ezzel, ám ez sem tartotta vissza őket. Az Országgyűlésben Lukács László, a Jobbik frakcióvezetője október 7-i felszólalásában szembesítette a kormányt "a bűnözés és a kábítószer járványszerű terjedésével". A "vidék kormányának" 14-ik évében éppen a leszakadó, szegényebb vidéki régiók fuldoklanak a bűnözés áradatában" - mutatott rá a politikus. (Kép: Pxhere)""",
                }
            },
        };
    }

    public static IEnumerable<TestCase> OrigoTestCases()
    {
        return new List<TestCase>
        {
            new TestCase
            {
                HtmlPath = "TestData/origo/1.html",
                ExpectedContent = new ContentParserResult
                {
                    SourceSite = NewsSites.HVG,
                    PublishDate = DateTime.Parse("2025-01-11T18:14:30Z"),
                    Text = """Ezúttal egy 85 éves bácsit vertek félholtra az otthonában Egyre gyakoribb, hogy idős emberekre rontanak rá a házukban. Bács-Kiskun Vármegyei Főügyészség életveszélyt okozó testi sértés és minősített rablás bűntette miatt vádat emelt egy testvérpárral szemben. Az elkövetők az otthonában rátörtek a 85 éves férfira, brutálisan bántalmazták, elvették 400.000 forintját, majd életveszélyes sérülésekkel magára hagyták - írja az Ügyészség honlapja. A vádirat szerint a kiskunhalasi testvérpár – a büntetett előéletű fiatalabb testvér felvetése alapján – 2023. szeptember 22-én, kilátástalan anyagi helyzetük megoldása érdekében, elhatározták, hogy betörnek az idős, nehézkesen mozgó sértetthez. A testvérek átmásztak a kerítésen, majd először a nyitott melléképületekben néztek szét, de mivel ott számukra értékkel bíró tárgyat nem találtak, a kutatást a házban folytatták. Ennek zajára felébredt a sértett, nem ijedt meg a vádlottaktól, kérdőre vonta őket. Válaszul a testvérek odaléptek a sértett ágyához és ököllel ütlegelni kezdték az idős férfit, aki az ágyról is leesett és ugyan fel tudott állni, de a váratlan, sorozatos, nagy erejű ütésekkel szemben nem tudott védekezni. A testvérek ellökték a sértettet, aki ismét elesett és beütötte a fejét a járólapba. Ezután az elkövetők letépték a sértett karján lévő gondosórát, elvették a telefonját. Átkutatták a házat, a sértett pénztárcájából kivettek 400.000 forintot, majd távoztak. A bántalmazás következtében az idős áldozat súlyos, életveszélyes sérüléseket szenvedett, életét az idejében érkezett orvosi ellátás és kórházi gyógykezelés mentette meg. A főügyészség a letartóztatásban lévő testvéreket életveszélyt okozó testi sértés bűntettével és a bűncselekmény felismerésére vagy elhárítására idős koránál fogva korlátozottan képes személy sérelmére elkövetett rablás bűntettével vádolja és velük szemben szabadságvesztés büntetés kiszabását indítványozta. Fuldoklik a vidék a bűn áradatában Az elmúlt években növekedett az erőszakos bűncselekmények száma, a korábbiakhoz képest szokatlanul gyakran érkezik hír hasonló brutális támadásról. Novemberben egy 85 éves veszprémi nénit gyilkoltak meg saját otthonában. Augusztusban Pécsen egy 94 éves embert öltek meg, majd rágyújtották a házát. Ugyanebben a hónapban Öcsödön egy 16 éves fiú 10 éves öccse szeme láttára vert agyon egy 85 éves bácsit, Jánoshalmán, egy 80 éves néni életét oltották ki a saját házában. A pénz az öcsödi és jánoshalmai gyilkosnak is kábítószerre kellett. Mindkét áldozat házát kamera figyelte. Az elkövetők valószínűleg tisztában voltak ezzel, ám ez sem tartotta vissza őket. Az Országgyűlésben Lukács László, a Jobbik frakcióvezetője október 7-i felszólalásában szembesítette a kormányt "a bűnözés és a kábítószer járványszerű terjedésével". A "vidék kormányának" 14-ik évében éppen a leszakadó, szegényebb vidéki régiók fuldoklanak a bűnözés áradatában" - mutatott rá a politikus. (Kép: Pxhere)""",
                }
            },
        };
    }

    public static IEnumerable<TestCase> PestiSracokTestCases()
    {
        return new List<TestCase>
        {
            new TestCase
            {
                HtmlPath = "TestData/pestisracok/1.html",
                ExpectedContent = new ContentParserResult
                {
                    SourceSite = NewsSites.HVG,
                    PublishDate = DateTime.Parse("2025-01-11T18:14:30Z"),
                    Text = """Ezúttal egy"""
                }
            }
        };
    }

    public static IEnumerable<TestCase> RipostTestCases()
    {
        return new List<TestCase>
        {
            new TestCase
            {
                HtmlPath = "TestData/ripost/1.html",
                ExpectedContent = new ContentParserResult
                {
                    SourceSite = NewsSites.HVG,
                    PublishDate = DateTime.Parse("2025-01-11T18:14:30Z"),
                    Text = """Ezúttal egy"""
                }
            }
        };
    }

    public static IEnumerable<TestCase> RTLTestCases()
    {
        return new List<TestCase>
        {
            new TestCase
            {
                HtmlPath = "TestData/rtl/1.html",
                ExpectedContent = new ContentParserResult
                {
                    SourceSite = NewsSites.HVG,
                    PublishDate = DateTime.Parse("2025-01-11T18:14:30Z"),
                    Text = """Ezúttal egy"""
                }
            }
        };
    }

    public static IEnumerable<TestCase> TelexTestCases()
    {
        return new List<TestCase>
        {
            new TestCase
            {
                HtmlPath = "TestData/telex/1.html",
                ExpectedContent = new ContentParserResult
                {
                    SourceSite = NewsSites.HVG,
                    PublishDate = DateTime.Parse("2025-01-11T18:14:30Z"),
                    Text = """Ezúttal egy"""
                }
            }
        };
    }

    public static IEnumerable<TestCase> HuszonnegyTestCases()
    {
        return new List<TestCase>
        {
            new TestCase
            {
                HtmlPath = "TestData/24hu/1.html",
                ExpectedContent = new ContentParserResult
                {
                    SourceSite = NewsSites.HVG,
                    PublishDate = DateTime.Parse("2025-01-11T18:14:30Z"),
                    Text = """Ezúttal egy"""
                }
            }
        };
    }

    public static IEnumerable<TestCase> NegynegynegyTestCases()
    {
        return new List<TestCase>
        {
            new TestCase
            {
                HtmlPath = "TestData/444/1.html",
                ExpectedContent = new ContentParserResult
                {
                    SourceSite = NewsSites.HVG,
                    PublishDate = DateTime.Parse("2025-01-11T18:14:30Z"),
                    Text = """Ezúttal egy"""
                }
            }
        };
    }
}
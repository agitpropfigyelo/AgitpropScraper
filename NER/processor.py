import spacy

def getNamedEntities(text:str):
    nlp = spacy.load("hu_core_news_lg")
    doc = nlp(text)
    
    named_entities = {}
    entity_mapping = {}  # Dictionary to map different entity mentions to a canonical representation

    for ent in doc.ents:
        canonical_entity = entity_mapping.get(ent.text, ent)  # Get the canonical representation or use the entity itself

        # Update entity mapping to link all text variations to the same canonical representation
        for alias in ent:
            entity_mapping[alias.text] = canonical_entity

        if canonical_entity.label_ not in named_entities:
            named_entities[canonical_entity.label_] = set()

        named_entities[canonical_entity.label_].add(canonical_entity.lemma_)
    
    return {key: list(value) for key, value in named_entities.items()}

    




if __name__ =="__main__":
    txt="""
    Gyurcsány Ferenc bement az ATV stúdiójába, és ismét feladatot osztott ki Karácsony Gergelynek. Ez nem lehet véletlen, a baloldal legerősebb pártja támogatásának ugyanis komoly ára van, és amennyiben újabb 5 évig szeretne főpolgármester maradni, ahhoz ezeket maradéktalanul teljesítenie is kell.
    Az ATV Egyenes Beszédében Gyurcsány Ferenc, miután eljátszotta a Momentummal szokásos hazug színjátékot az álveszekedésről, Karácsony Gergelynek osztott ki feladatokat.
    Mint ismert, idén márciusban a Spirit FM műsorában a bukott miniszterelnöknek kellett ráébresztenie Karácsony Gergelyt, hogy "ambíciói végesek" és nem végzi jól a munkáját, "nem teljesíti hazafias kötelezettségét", ha nem megy szembe élesen Orbán Viktorral és a magyar kormánnyal.
    A főpolgármester nem hajlandó azt a hazafias kötelezettségét teljesíteni, hogy ebbe a küzdelembe beleviszi a várost
    - mondta akkor Gyurcsány.
    A tegnap esti adásban pedig a következőket mondta:
    Többször is megkíséreltem, hogy énszerintem ezzel a kormánnyal szemben más politikát kell követni.
    Majd emlékeztetett:
    mindig úgy tesszük föl a kérdést természetesen, hogy az adott helyzetben ki jelenti a legjobb megoldást? Egyikünk sem tökéletes, valószínűleg úgy láttuk, hogy az adott helyzetben Karácsony Gergely a legjobb megoldás.
    Arra a kérdésre, hogy végső soron elégedettek-e azzal a politikával, amit a főpolgármester képvisel vagy nem, illetve támasztottak-e bármilyen feltételt a támogatásukért cserébe, Gyurcsány Ferenc kitérő választ adott. 
    Nekem ebben az időszakban már az a dolgom, hogy abban segítsek a főpolgármesternek, hogy azt mutassuk meg, hogy milyen erényeink vannak nekünk közösen
    Emlékezetes, a DK elnöke 2019-ben még azon az állásponton volt, hogy Karácsony Gergely mögül "elment a támogatás", és pártjával inkább Kálmán Olgát támogatták a főpolgármester-jelöltek közül az ellenzéki előválasztáson. 
    """
    result=getNamedEntities(txt)
    print(result)
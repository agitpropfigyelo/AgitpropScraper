# Python

"""
FastAPI-based NLP service for named entity recognition (NER) using spaCy.
"""

from fastapi import FastAPI, HTTPException
from typing import List, Dict, Any
import spacy
import os

app = FastAPI(title="NLP Service", description="Named Entity Recognition API using spaCy")

try:
    nlp = spacy.load("hu_core_news_lg")
except Exception as e:
    raise



@app.get("/health")
def healthcheck():
    return {"status": "alive"}

@app.post("/analyzeSingle")
def analyze_single_corpus(text: str):
    try:
        doc = nlp(text)
        result = get_named_entities(doc)
        return result
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/analyzeBatch")
def analyze_batch_corpus(texts: List[str]):
    try:
        result = []
        
        for doc in nlp.pipe(texts):
            entities = get_named_entities(doc)
            result.append(entities)
            
        return result
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

def get_named_entities(doc):
    entities = []
    seen = set()
    for ent in doc.ents:
        entity_dict = {
            "Item1": ent.lemma_,
            "Item2": ent.label_
        }
        key = (entity_dict["Item1"], entity_dict["Item2"])
        if key not in seen:
            seen.add(key)
            entities.append(entity_dict)
    return entities

@app.get("/discovery")
def discovery():
    return {"endpoints": ["/health", "/analyzeSingle", "/analyzeBatch", "/discovery"]}

if __name__ == '__main__':
    import uvicorn
    
    port = int(os.environ.get('PORT', 8111))
    debug = bool(os.environ.get('DEBUG', False))
    host = os.environ.get('HOST', '127.0.0.1')
    
    uvicorn.run(app, host=host, port=port, reload=debug)

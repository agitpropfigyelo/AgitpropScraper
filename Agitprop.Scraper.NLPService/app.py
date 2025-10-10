# Python

"""
Flask-based NLP service for named entity recognition (NER) using spaCy.
"""

from flask import Flask, request, jsonify
import spacy
import os

app = Flask(__name__)

try:
    nlp = spacy.load("hu_core_news_lg")
except Exception as e:
    raise

@app.route("/health")
def healthcheck():
    return {"status": "alive"}, 200

@app.route("/analyzeSingle", methods=['POST'])
def analyzeSingleCorpus():
    try:
        data = request.get_json()
        if not data:
            return jsonify({"error": "No data provided"}), 400

        text = str(data)
        doc = nlp(text)
        result = getNamedEntities(doc)
        return jsonify(result), 200

    except Exception as e:
        return jsonify({"error": str(e)}), 500

@app.route("/analyzeBatch", methods=['POST'])
def analyzeBatchCorpus():
    try:
        data = request.get_json()
        if not data:
            return jsonify({"error": "No data provided"}), 400

        texts = list(data)
        result = []
        
        for doc in nlp.pipe(texts):
            entities = getNamedEntities(doc)
            result.append(entities)
            
        return jsonify(result), 200

    except Exception as e:
        return jsonify({"error": str(e)}), 500

def getNamedEntities(doc):
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

@app.route("/discovery")
def discovery():
    return {"endpoints": ["/health", "/analyzeSingle", "/analyzeBatch", "/discovery"]}, 200

if __name__ == '__main__':

    port = int(os.environ.get('PORT', 8111))
    debug = bool(os.environ.get('DEBUG', False))
    host = os.environ.get('HOST', '127.0.0.1')
    app.run(port=port, debug=debug, host=host)

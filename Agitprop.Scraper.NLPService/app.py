"""
This module implements a Flask-based NLP service for named entity recognition (NER) using spaCy.

Endpoints:
- /health: Health check endpoint to verify the service is running.
- /analyzeSingle: Analyzes a single text corpus for named entities.
- /analyzeBatch: Analyzes a batch of text corpora for named entities.
- /discovery: Lists available endpoints.

The service uses the Hungarian language model `hu_core_news_lg` from spaCy.
"""

from flask import Flask, request, jsonify
import spacy

nlp = spacy.load("hu_core_news_lg")
app = Flask(__name__)

@app.route("/health")
def healthcheck():
    """
    Health check endpoint to verify the service is running.

    Returns:
        dict: A dictionary with the status of the service.
        int: HTTP status code 200.
    """
    return {"status": "alive"}, 200

@app.route("/analyzeSingle", methods=['POST'])
def analyzeSingleCorpus():
    """
    Analyzes a single text corpus for named entities.

    Expects:
        JSON payload containing the text corpus.

    Returns:
        dict: A dictionary of named entities grouped by their types.
        int: HTTP status code 200.
    """
    data = request.get_json()
    text = str(data)
    doc = nlp(text)
    result = getNamedEntities(doc)

    return jsonify(result), 200

@app.route("/analyzeBatch", methods=['POST'])
def analyzeBatchCorpus():
    """
    Analyzes a batch of text corpora for named entities.

    Expects:
        JSON payload containing a list of text corpora.

    Returns:
        list: A list of dictionaries, each containing named entities grouped by their types.
        int: HTTP status code 200.
    """
    data = request.get_json()
    texts = list(data)
    result = []
    for doc in nlp.pipe(texts):
        result.append(getNamedEntities(doc))

    return jsonify(result), 200

def getNamedEntities(doc):
    """
    Extracts named entities from a spaCy document.

    Args:
        doc (spacy.tokens.Doc): The spaCy document to analyze.

    Returns:
        dict: A dictionary where keys are entity types and values are lists of canonical entity names.
    """
    named_entities = {}
    # Dictionary to map different entity mentions to a canonical representation
    entity_mapping = {}

    for ent in doc.ents:
        # Get the canonical representation or use the entity itself
        canonical_entity = entity_mapping.get(ent.text, ent)

        # Update entity mapping to link all text variations to the same canonical representation
        for alias in ent:
            entity_mapping[alias.text] = canonical_entity

        if canonical_entity.label_ not in named_entities:
            named_entities[canonical_entity.label_] = set()

        named_entities[canonical_entity.label_].add(canonical_entity.lemma_)

    return {key: list(value) for key, value in named_entities.items()}

@app.route("/discovery")
def discovery():
    """
    Lists available endpoints of the service.

    Returns:
        dict: A dictionary containing the list of available endpoints.
        int: HTTP status code 200.
    """
    return {"endpoints": ["/healthcheck", "/analyzeSingle", "/analyzeBatch"]}, 200

if __name__ == "__main__":
    #port = int(os.environ.get('PORT', 8111))
    app.run(host='0.0.0.0', port=8111, debug=True)
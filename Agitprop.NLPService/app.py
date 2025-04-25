from flask import Flask, request, jsonify
import spacy
import os
from opentelemetry import trace
from opentelemetry.instrumentation.flask import FlaskInstrumentor
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor
from opentelemetry.exporter.otlp.proto.grpc.trace_exporter import OTLPSpanExporter

nlp = spacy.load("hu_core_news_lg")
app = Flask(__name__)

@app.route("/health")
def healthcheck():
    return {"status": "alive"}, 200


@app.route("/analyzeSingle", methods=['POST'])
def analyzeSingleCorpus():
    data = request.get_json()
    text = str(data)
    doc = nlp(text)
    result = getNamedEntities(doc)

    return jsonify(result), 200


@app.route("/analyzeBatch", methods=['POST'])
def analyzeBatchCorpus():
    data = request.get_json()
    texts = list(data)
    result = []
    for doc in nlp.pipe(texts):
        result.append(getNamedEntities(doc))

    return jsonify(result), 200


def getNamedEntities(doc):
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
    return {"endpoints": ["/healthcheck", "/analyzeSingle", "/analyzeBatch"]}, 200


if __name__ == "__main__":
    OTEL_EXPORTER_OTLP_ENDPOINT = os.environ.get("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:4317")
    # Set up OpenTelemetry tracing
    trace.set_tracer_provider(TracerProvider())
    otlp_exporter = OTLPSpanExporter(endpoint=OTEL_EXPORTER_OTLP_ENDPOINT, insecure=True)
    span_processor = BatchSpanProcessor(otlp_exporter)
    trace.get_tracer_provider().add_span_processor(span_processor)

    # Instrument Flask app
    FlaskInstrumentor().instrument_app(app)

    port = int(os.environ.get('PORT', 8111))
    app.run(host='0.0.0.0', port=port)

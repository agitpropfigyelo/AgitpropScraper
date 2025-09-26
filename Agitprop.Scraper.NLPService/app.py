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
import spacy,logging,os
from opentelemetry import trace
from opentelemetry.exporter.otlp.proto.grpc.trace_exporter import OTLPSpanExporter
from opentelemetry.sdk.trace import TracerProvider
from opentelemetry.sdk.trace.export import BatchSpanProcessor
from opentelemetry.instrumentation.flask import FlaskInstrumentor
from opentelemetry.sdk.resources import Resource
from opentelemetry.semconv.resource import ResourceAttributes
from opentelemetry.instrumentation.logging import LoggingInstrumentor
import logging.handlers

# Define service information
resource = Resource.create({
    ResourceAttributes.SERVICE_NAME: "nlp-service",
    ResourceAttributes.SERVICE_VERSION: "1.0.0",
    ResourceAttributes.DEPLOYMENT_ENVIRONMENT: os.getenv("DEPLOYMENT_ENVIRONMENT", "development")
})

nlp = spacy.load("hu_core_news_lg")
app = Flask(__name__)


provider = TracerProvider(resource=resource)
trace.set_tracer_provider(provider)
otlp_exporter = OTLPSpanExporter(endpoint=os.getenv("OTEL_EXPORTER_OTLP_ENDPOINT", "http://localhost:4317"))
processor = BatchSpanProcessor(otlp_exporter)
provider.add_span_processor(processor)

# Initialize tracer
tracer = trace.get_tracer(__name__)

# Define hooks for Flask instrumentation
def request_hook(span, environ):
    try:
        content_length = environ.get("CONTENT_LENGTH", "0")
        span.set_attribute("http.request.size", int(content_length))
    except (ValueError, TypeError):
        span.set_attribute("http.request.size", 0)

def response_hook(span, status, response_headers):
    try:
        # Response headers are a list of tuples, find Content-Length
        content_length = "0"
        for header, value in response_headers:
            if header == "Content-Length":
                content_length = value
                break
        span.set_attribute("http.response.size", int(content_length))
    except (ValueError, TypeError):
        span.set_attribute("http.response.size", 0)

# Configure Flask instrumentation with request/response hooks
FlaskInstrumentor().instrument_app(
    app,
    excluded_urls="health",  # Don't trace health checks
    request_hook=request_hook,
    response_hook=response_hook
)

# Configure logging with a format that includes trace context
formatter = logging.Formatter(
    fmt='%(asctime)s %(levelname)s [%(name)s] [trace_id=%(otelTraceID)s span_id=%(otelSpanID)s] %(message)s',
    datefmt='%Y-%m-%d %H:%M:%S'
)

# Create handlers
console_handler = logging.StreamHandler()
console_handler.setFormatter(formatter)

file_handler = logging.handlers.RotatingFileHandler(
    'nlp_service.log',
    maxBytes=10485760,  # 10MB
    backupCount=3
)
file_handler.setFormatter(formatter)

# Configure root logger
logging.basicConfig(level=logging.INFO, handlers=[console_handler, file_handler])
logger = logging.getLogger(__name__)

# Instrument logging to include trace context
LoggingInstrumentor().instrument(
    set_logging_format=True,
    log_level=logging.INFO
)

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
        list: A list of dictionaries, each containing:
            - Item1 (str): The entity text (lemmatized)
            - Item2 (str): The entity type
        Example:
            [
                {"Item1": "Budapest", "Item2": "GPE"},
                {"Item1": "János", "Item2": "PERSON"}
            ]
        int: HTTP status code 200 on success, 400 for bad request, 500 for server error.
    """
    try:
        with tracer.start_as_current_span("analyze_single") as span:
            data = request.get_json()
            if not data:
                logger.error("No data provided in request")
                return jsonify({"error": "No data provided"}), 400
            
            text = str(data)
            span.set_attribute("text.length", len(text))
            
            doc = nlp(text)
            result = getNamedEntities(doc)
            span.set_attribute("entities.count", len(result))
            
            logger.info(f"Successfully analyzed text with {len(result)} entities")
            return jsonify(result), 200
            
    except Exception as e:
        logger.exception("Error processing single corpus")
        return jsonify({"error": str(e)}), 500

@app.route("/analyzeBatch", methods=['POST'])
def analyzeBatchCorpus():
    """
    Analyzes a batch of text corpora for named entities.

    Expects:
        JSON payload containing a list of text corpora.

    Returns:
        list: A list of lists, where each inner list contains dictionaries with:
            - Item1 (str): The entity text (lemmatized)
            - Item2 (str): The entity type
        Example:
            [
                [
                    {"Item1": "Budapest", "Item2": "GPE"},
                    {"Item1": "János", "Item2": "PERSON"}
                ],
                [
                    {"Item1": "Magyarország", "Item2": "GPE"}
                ]
            ]
        int: HTTP status code 200 on success, 400 for bad request, 500 for server error.
    """
    try:
        with tracer.start_as_current_span("analyze_batch") as span:
            data = request.get_json()
            if not data:
                logger.error("No data provided in request")
                return jsonify({"error": "No data provided"}), 400

            texts = list(data)
            span.set_attribute("batch.size", len(texts))
            
            result = []
            total_entities = 0
            
            with tracer.start_as_current_span("process_documents") as process_span:
                for doc in nlp.pipe(texts):
                    entities = getNamedEntities(doc)
                    result.append(entities)
                    total_entities += len(entities)
                
                process_span.set_attribute("total_entities", total_entities)
            
            logger.info(f"Successfully analyzed batch of {len(texts)} texts with {total_entities} total entities")
            return jsonify(result), 200
            
    except Exception as e:
        logger.exception("Error processing batch corpus")
        return jsonify({"error": str(e)}), 500

def getNamedEntities(doc):
    """
    Extracts named entities from a spaCy document.

    Args:
        doc (spacy.tokens.Doc): The spaCy document to analyze.

    Returns:
        list: A list of dictionaries, each containing 'text' and 'type' keys
              formatted to be compatible with C# ValueTuple<string, string>.
    """
    entities = []
    # Dictionary to map different entity mentions to a canonical representation
    entity_mapping = {}

    for ent in doc.ents:
        # Get the canonical representation or use the entity itself
        canonical_entity = entity_mapping.get(ent.text, ent)

        # Update entity mapping to link all text variations to the same canonical representation
        for alias in ent:
            entity_mapping[alias.text] = canonical_entity

        # Create a dictionary with the required structure
        entity_dict = {
            "Item1": canonical_entity.lemma_,  # text
            "Item2": canonical_entity.label_   # type
        }
        entities.append(entity_dict)

    # Remove duplicates while preserving order (based on both text and type)
    unique_entities = []
    seen = set()
    for entity in entities:
        key = (entity["Item1"], entity["Item2"])
        if key not in seen:
            seen.add(key)
            unique_entities.append(entity)
    
    return unique_entities

@app.route("/discovery")
def discovery():
    """
    Lists available endpoints of the service.

    Returns:
        dict: A dictionary containing the list of available endpoints.
        int: HTTP status code 200.
    """
    return {"endpoints": ["/healthcheck", "/analyzeSingle", "/analyzeBatch"]}, 200

def shutdown_handler(signal_int, frame):
    """Handle shutdown gracefully by flushing telemetry data."""
    logger.info("Shutting down NLP service...")
    
    # Flush trace data
    trace.get_tracer_provider().shutdown()
    
    # Close log handlers
    logging.shutdown()
    
    # Exit gracefully
    sys.exit(0)

if __name__ == '__main__':
    import signal
    import sys
    
    # Register shutdown handler
    signal.signal(signal.SIGINT, shutdown_handler)
    signal.signal(signal.SIGTERM, shutdown_handler)
    
    port = int(os.environ.get('PORT', 8111))
    debug = bool(os.environ.get('DEBUG', False))
    host = os.environ.get('HOST', '127.0.0.1')
    
    logger.info(f"Starting NLP service on {host}:{port}")
    app.run(port=port, debug=debug, host=host)
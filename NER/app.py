from flask import Flask, request, jsonify
import processor as proc

app = Flask(__name__)

@app.route("/")
def hello_world():
    return "<p>Hello, World!</p>"

@app.route("/analyze", methods=['POST'])
def analyzeCorpus():
    data = request.get_json()
    if 'corpus' not in data:
        return jsonify({"error": "Missing 'corpus' key in JSON data"}), 400
    text=data['corpus']
    entities = proc.getNamedEntities(text)
    return jsonify({"entities": entities})

if __name__=="__main__":
    app.run()

    #print(f"{source} Processed result: {result}")
from flask import Flask, request, jsonify
from flask_cors import CORS
from services.gemini_service import GeminiService
import os

app = Flask(__name__)
CORS(app) # Enable CORS for all routes

gemini_service = GeminiService()

@app.route('/analyze-claim', methods=['POST'])
def analyze_claim():
    try:
        data = request.get_json()
        
        # Validation
        required_fields = [
            "claimAmount", "coverageAmount", "policyAgeDays", 
            "riskZone", "marketValue", "hasSecuritySystem", 
            "yearBuilt", "claimReason"
        ]
        
        missing_fields = [field for field in required_fields if field not in data]
        if missing_fields:
            return jsonify({
                "error": "Missing required fields",
                "missing": missing_fields
            }), 400

        # Process analysis
        result = gemini_service.analyze_claim(data)
        
        return jsonify(result), 200

    except Exception as e:
        return jsonify({
            "error": "Internal Server Error",
            "message": str(e)
        }), 500

@app.route('/analyze-prospect', methods=['POST'])
def analyze_prospect():
    try:
        data = request.get_json()
        
        # Validation
        required_fields = [
            "policyCount", "totalPremiumPaid", "claimCount", "customerTenureDays"
        ]
        
        missing_fields = [field for field in required_fields if field not in data]
        if missing_fields:
            return jsonify({
                "error": "Missing required fields",
                "missing": missing_fields
            }), 400

        # Process analysis
        result = gemini_service.analyze_prospect(data)
        
        return jsonify(result), 200

    except Exception as e:
        return jsonify({
            "error": "Internal Server Error",
            "message": str(e)
        }), 500

@app.route('/health', methods=['GET'])
def health_check():
    return jsonify({"status": "healthy"}), 200

if __name__ == '__main__':
    port = int(os.getenv("PORT", 5000))
    app.run(host='0.0.0.0', port=port, debug=True)

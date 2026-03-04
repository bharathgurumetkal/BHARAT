import os
import json
import requests
from dotenv import load_dotenv
from services.heuristic_service import HeuristicService

load_dotenv()

class GeminiService:
    """
    Service to handle AI Analysis using OpenRouter (LLM) with a Heuristic fallback.
    Renamed from GeminiService to keep compatibility with existing app.py imports.
    """
    def __init__(self):
        self.heuristic = HeuristicService()
        self.api_key = os.getenv("OPENROUTER_API_KEY")
        self.api_url = "https://openrouter.ai/api/v1/chat/completions"
        self.model = "google/gemini-2.0-flash-001" # Default OpenRouter model for Gemini 2.0 Flash
        
        if not self.api_key:
            # If no OpenRouter key, we'll hit the fallback immediately in analyze_claim
            print("WARNING: OPENROUTER_API_KEY not found. Operating in HEURISTIC ONLY mode.")

    def analyze_claim(self, claim_data):
        if not self.api_key:
            return self.heuristic.analyze_claim_locally(claim_data)

        prompt = f"""
Analyze the following insurance claim data for risk and potential fraud.
Return ONLY a valid JSON object in this exact format:
{{
  "riskScore": number (0-100),
  "riskLevel": "Low" | "Medium" | "High",
  "fraudProbability": number (0-1),
  "explanation": "Brief analytical explanation",
  "recommendation": "Final action recommendation"
}}

Data:
{json.dumps(claim_data, indent=2)}
"""

        payload = {
            "model": self.model,
            "messages": [{"role": "user", "content": prompt}],
            "temperature": 0.1,
            "response_format": { "type": "json_object" }
        }

        headers = {
            "Authorization": f"Bearer {self.api_key}",
            "HTTP-Referer": "http://localhost:5033", # Optional for OpenRouter
            "X-Title": "Insurance Management System",
            "Content-Type": "application/json"
        }

        try:
            response = requests.post(self.api_url, headers=headers, json=payload, timeout=15)
            response.raise_for_status()
            
            resp_json = response.json()
            ai_text = resp_json['choices'][0]['message']['content'].strip()
            
            # Use JSON parsing with robust extraction
            start_idx = ai_text.find('{')
            end_idx = ai_text.rfind('}') + 1
            if start_idx != -1 and end_idx != -1:
                json_str = ai_text[start_idx:end_idx]
                return json.loads(json_str)
            else:
                return json.loads(ai_text)

        except Exception as e:
            print(f"OpenRouter Error (Falling back to local): {str(e)}")
            # If OpenRouter fails (Quota, Auth, Network), use the local Heuristic fallback
            return self.heuristic.analyze_claim_locally(claim_data)

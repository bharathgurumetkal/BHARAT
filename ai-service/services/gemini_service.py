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
        self.model = "google/gemini-2.0-flash-001"
        
        if not self.api_key:
            print("WARNING: OPENROUTER_API_KEY not found. Operating in HEURISTIC ONLY mode.")

    def _call_llm(self, prompt: str, temperature: float = 0.1) -> dict | None:
        """
        Internal helper to call the LLM and return the parsed JSON response.
        Returns None if the API key is missing or if an exception occurs.
        """
        if not self.api_key:
            return None
        
        payload = {
            "model": self.model,
            "messages": [{"role": "user", "content": prompt}],
            "temperature": temperature,
            "response_format": {"type": "json_object"}
        }
        headers = {
            "Authorization": f"Bearer {self.api_key}",
            "HTTP-Referer": "http://localhost:5033",
            "X-Title": "Insurance Management System",
            "Content-Type": "application/json"
        }
        try:
            response = requests.post(self.api_url, headers=headers, json=payload, timeout=20)
            response.raise_for_status()
            resp_json = response.json()
            ai_text = resp_json['choices'][0]['message']['content'].strip()
            start_idx = ai_text.find('{')
            end_idx = ai_text.rfind('}') + 1
            if start_idx != -1 and end_idx > start_idx:
                return json.loads(ai_text[start_idx:end_idx])
            return json.loads(ai_text)
        except Exception as e:
            print(f"OpenRouter LLM call failed: {str(e)}")
            return None

    def analyze_claim(self, claim_data: dict) -> dict:
        """
        Performs a comprehensive fraud and risk analysis on a submitted claim.
        Uses LLM with heuristic fallback.
        """
        # Build a rich, structured prompt
        damage_type = claim_data.get("damageType", "Not specified")
        incident_location = claim_data.get("incidentLocation", "Not specified")
        incident_date = claim_data.get("incidentDate", "Not specified")
        claim_amount = claim_data.get("claimAmount", 0)
        coverage_amount = claim_data.get("coverageAmount", 1)
        policy_age = claim_data.get("policyAgeDays", 0)
        risk_zone = claim_data.get("riskZone", "Unknown")
        market_value = claim_data.get("marketValue", 0)
        has_security = claim_data.get("hasSecuritySystem", False)
        year_built = claim_data.get("yearBuilt", 0)
        claim_reason = claim_data.get("claimReason", "Not provided")

        coverage_ratio_pct = round((claim_amount / max(coverage_amount, 1)) * 100, 1)

        prompt = f"""
You are a senior insurance fraud analyst and claims risk assessment expert at a leading Indian insurance company. 
Your task is to perform a rigorous, data-driven analysis of the following insurance claim submission and determine its risk level and fraud probability.

## CLAIM DATA
- Claim Amount Requested: ₹{claim_amount:,}
- Policy Total Coverage: ₹{coverage_amount:,}
- Coverage Utilization: {coverage_ratio_pct}% of total policy limit
- Policy Age: {policy_age} days (since policy start date)
- Incident Date: {incident_date}
- Incident Location: {incident_location}
- Damage Type: {damage_type}
- Customer's Explanation: "{claim_reason}"

## PROPERTY PROFILE
- Property Market Value: ₹{market_value:,}
- Year Built: {year_built}
- Risk Zone Classification: {risk_zone}
- Has Security/Alarm System: {"Yes" if has_security else "No"}

## FRAUD DETECTION RULES TO APPLY
Evaluate using these industry-standard fraud indicators:
1. **Claim Timing**: Claims within 30 days of policy start are high-risk (opportunistic fraud indicator). 31-90 days = moderate risk.
2. **Coverage Ratio**: Claims exceeding 80% of coverage are statistically rare and require additional scrutiny. 50-80% warrants review.
3. **Damage Consistency**: Does the stated damage type (e.g., fire, flood) logically match the claim amount and the property's risk zone?
4. **Incident Location vs. Property**: Flag if incident location appears inconsistent with the insured property address.
5. **Property Age/Value**: Older properties claiming high amounts for fires/floods are more frequently associated with fraud.
6. **Security System**: Properties with security systems are less likely to face theft, so theft claims without one are normal, but theft claims WITH one are worth examining.
7. **Narrative Quality**: Vague or extremely short explanations may indicate fraud. Specific, detailed explanations reduce risk.

## YOUR TASK
Analyze ALL the above factors holistically and return ONLY a valid JSON object in this EXACT format:
{{
  "riskScore": <integer 0-100, where 0=no risk, 100=definite fraud>,
  "riskLevel": "<one of: 'Low', 'Medium', 'High'>",
  "fraudProbability": <decimal 0.0-1.0>,
  "explanation": "<A professional 2-3 sentence analysis citing the specific factors that drove your score. Be concrete and reference the actual numbers provided.>",
  "recommendation": "<A clear, actionable directive for the claims officer (e.g., 'Approve with standard document check', 'Request police FIR and independent property valuation', 'Flag for immediate fraud investigation and freeze payout')>"
}}

CRITICAL: Return ONLY the JSON object, no preamble, no markdown code blocks, no trailing text.
"""
        result = self._call_llm(prompt, temperature=0.05)
        if result:
            return result
        
        print("LLM unavailable, falling back to heuristic for analyze_claim.")
        return self.heuristic.analyze_claim_locally(claim_data)

    def analyze_claim_smart(self, form_data: dict) -> dict:
        """
        AI Smart Assistant: Analyzes a partially filled claim form and provides
        intelligent suggestions for the user — recommended documents, missing info, 
        and a preliminary risk level. This is the 'live helper' during form filling.
        """
        policy_name = form_data.get("policyName", "Insurance Policy")
        policy_number = form_data.get("policyNumber", "N/A")
        coverage_amount = form_data.get("coverageAmount", 0)
        claim_amount = form_data.get("claimAmount", 0)
        reason = form_data.get("reason", "")
        damage_type = form_data.get("damageType", "")
        incident_location = form_data.get("incidentLocation", "")
        incident_date = form_data.get("incidentDate", "")
        property_address = form_data.get("propertyAddress", "")
        risk_zone = form_data.get("riskZone", "")
        year_built = form_data.get("yearBuilt", 0)
        has_security = form_data.get("hasSecuritySystem", False)
        documents_count = form_data.get("documentsCount", 0)

        coverage_ratio_pct = round((claim_amount / max(coverage_amount, 1)) * 100, 1) if claim_amount and coverage_amount else 0

        prompt = f"""
You are an expert insurance claims assistant AI helping a customer fill out an insurance claim form at an Indian insurance company.
Based on the claim details entered so far, you must provide intelligent, personalized suggestions to help the customer submit a complete, successful, and honest claim.

## CLAIM FORM DATA (AS FILLED SO FAR)
- Policy: {policy_name} (#{policy_number})
- Total Policy Coverage: ₹{coverage_amount:,}
- Claim Amount Requested: ₹{claim_amount:,} ({coverage_ratio_pct}% of coverage limit)
- Incident Date: {incident_date if incident_date else "NOT PROVIDED"}
- Incident Location: {incident_location if incident_location else "NOT PROVIDED"}
- Damage Type Selected: {damage_type if damage_type else "NOT SELECTED"}
- Customer's Reason (Description): "{reason if reason else "NOT PROVIDED"}"
- Property Address: {property_address if property_address else "N/A"}
- Risk Zone: {risk_zone if risk_zone else "N/A"}
- Year Built: {year_built if year_built else "N/A"}
- Security System: {"Yes" if has_security else "No"}
- Documents Uploaded: {documents_count}

## INSTRUCTIONS
1. **recommendedDocuments**: Based on the damage type and claim context, list the specific documents the customer MUST upload. Be damage-type-specific:
   - FIRE: FIR copy, Fire Brigade Report, Property valuation report, Photographs of damage, Smoke/soot assessment report
   - WATER LEAK/FLOOD: Plumber's report, Municipal flood records / weather data, Repair estimates, Photographs
   - THEFT: Police FIR (mandatory), CCTV footage (if available), List of stolen items with valuation
   - NATURAL DISASTER: Meteorological Department certificate, Government disaster declaration, Property inspection report
   - OTHER: General repair estimate, Incident photos, Third-party witness statement (if any)
   Always include: Claim form, Policy document copy, and Government-issued ID proof.

2. **missingInfo**: Identify which REQUIRED fields are empty/insufficient. Mark as missing ONLY if critically absent.
   Check for: incidentDate (empty = critical), incidentLocation (empty = critical), damageType (empty = critical), 
   reason/description (provided but too short < 20 chars = warn), documentsCount (0 = critical).

3. **riskLevel**: Based on the data, give a preliminary "Low", "Medium", or "High" risk assessment.
   - High: claimAmount > 80% of coverage, OR incidentDate not given, OR very new policy, OR very vague reason
   - Medium: claimAmount 50-80% of coverage, OR incomplete location, OR moderate reason detail
   - Low: All fields present, reasonable claim amount, detailed reason, documents uploaded

4. **riskMessage**: A single, friendly, encouraging sentence explaining the risk level to the customer (NOT accusatorial).

5. **tips**: 2-3 short, actionable tips to improve their claim submission quality.

Return ONLY this JSON object with no extra text or markdown:
{{
  "recommendedDocuments": ["<doc1>", "<doc2>", "..."],
  "missingInfo": ["<field or warning message>", "..."],
  "riskLevel": "<Low | Medium | High>",
  "riskMessage": "<Friendly 1-sentence message about current risk level>",
  "tips": ["<tip1>", "<tip2>", "<tip3>"]
}}
"""
        result = self._call_llm(prompt, temperature=0.15)
        if result:
            return result
        
        # Robust local fallback
        return self._smart_fallback(form_data, coverage_ratio_pct, documents_count)

    def _smart_fallback(self, form_data: dict, coverage_ratio_pct: float, documents_count: int) -> dict:
        """Local heuristic fallback for the smart assistant if LLM is unavailable."""
        damage_type = form_data.get("damageType", "").lower()
        reason = form_data.get("reason", "")
        incident_date = form_data.get("incidentDate", "")
        incident_location = form_data.get("incidentLocation", "")

        # Recommended documents by damage type
        doc_map = {
            "fire": ["FIR / Police Report", "Fire Brigade Report", "Damage Photographs", "Property Valuation Report", "Claim Form & Policy Copy", "Government ID Proof"],
            "water leak": ["Plumber's Inspection Report", "Repair Cost Estimate", "Damage Photographs", "Claim Form & Policy Copy", "Government ID Proof"],
            "flood": ["Meteorological Data / Weather Report", "Municipal Flood Records", "Damage Photographs", "Repair Estimate", "Claim Form & Policy Copy", "Government ID Proof"],
            "theft": ["Police FIR (Mandatory)", "CCTV Footage (if available)", "List of Stolen Items with Valuation", "Claim Form & Policy Copy", "Government ID Proof"],
            "natural disaster": ["Meteorological Certificate", "Government Disaster Declaration", "Property Inspection Report", "Damage Photographs", "Claim Form & Policy Copy", "Government ID Proof"],
        }
        docs = doc_map.get(damage_type, ["Damage Photographs", "Incident Report", "Repair Estimate", "Claim Form & Policy Copy", "Government ID Proof"])

        # Missing info
        missing = []
        if not incident_date:
            missing.append("Incident Date is required")
        if not incident_location:
            missing.append("Incident Location is required")
        if not damage_type:
            missing.append("Damage Type must be selected")
        if len(reason) < 20:
            missing.append("Reason/Description needs more detail (minimum 20 characters)")
        if documents_count == 0:
            missing.append("No supporting documents uploaded — at least 1 is required")

        # Risk level
        if coverage_ratio_pct > 80 or not incident_date or not damage_type:
            risk_level = "High"
            risk_message = "This claim has some high-risk indicators. Completing all fields and uploading documents will help expedite your review."
        elif coverage_ratio_pct > 50 or len(missing) > 1:
            risk_level = "Medium"
            risk_message = "Your claim looks good so far! Fill in the remaining details to improve your submission quality."
        else:
            risk_level = "Low"
            risk_message = "Excellent! Your claim is well-documented. You're on track for a smooth review process."

        return {
            "recommendedDocuments": docs,
            "missingInfo": missing,
            "riskLevel": risk_level,
            "riskMessage": risk_message,
            "tips": [
                "Provide a detailed, chronological description of the incident for faster processing.",
                "Upload clear, high-resolution photographs of all damage areas.",
                "Ensure all uploaded documents are legible and current."
            ]
        }

    def analyze_prospect(self, prospect_data):
        """Analyzes customer renewal likelihood."""
        if not self.api_key:
            return self.heuristic.analyze_prospect_locally(prospect_data)

        prompt = f"""
You are a senior customer retention analyst at an Indian insurance company. Analyze the following customer data to predict their likelihood to renew their policies and their overall sentiment toward the company.

Return ONLY a valid JSON object in this exact format:
{{
  "renewalScore": <number 0-100>,
  "likelihood": "<Low | Medium | High | Very High>",
  "churnProbability": <decimal 0.0-1.0>,
  "explanation": "<Brief analytical explanation of why they might or might not renew>",
  "recommendedAction": "<Specific, actionable advice for the agent to retain this customer>"
}}

Customer Data:
{json.dumps(prospect_data, indent=2)}

Return ONLY the JSON object, no additional text.
"""

        result = self._call_llm(prompt, temperature=0.2)
        if result:
            return result
        
        print("LLM unavailable, falling back to heuristic for analyze_prospect.")
        return self.heuristic.analyze_prospect_locally(prospect_data)

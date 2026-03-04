class HeuristicService:
    def analyze_claim_locally(self, data):
        """
        Provides a data-driven risk analysis using business rules
        when the LLM service is unavailable (e.g., Quota Exceeded).
        """
        score = 15 # Base risk
        
        claim_amt = float(data.get("claimAmount", 0))
        coverage_amt = float(data.get("coverageAmount", 1))
        policy_age = int(data.get("policyAgeDays", 0))
        risk_zone = data.get("riskZone", "Medium")
        has_security = data.get("hasSecuritySystem", False)
        
        # Rule 1: High claim relative to coverage
        ratio = claim_amt / coverage_amt
        if ratio > 0.8:
            score += 40
        elif ratio > 0.5:
            score += 20
            
        # Rule 2: New policy (within 30 days)
        if policy_age < 30:
            score += 25
        elif policy_age < 90:
            score += 10
            
        # Rule 3: High risk zones
        if risk_zone == "High":
            score += 15
            
        # Rule 4: Security system mitigates risk
        if has_security:
            score -= 10
            
        # Final Score Clamping
        score = max(5, min(95, score))
        
        # Risk Level
        if score > 75:
            risk_level = "High"
            recommendation = "IMMEDIATE AUDIT: High probability of soft fraud due to claim timing/amount."
        elif score > 40:
            risk_level = "Medium"
            recommendation = "Secondary Review: Validate claim cause and verify property condition."
        else:
            risk_level = "Low"
            recommendation = "Standard Review: Proceed with document verification."

        return {
            "riskScore": score,
            "riskLevel": risk_level,
            "fraudProbability": score / 100.0,
            "explanation": f"Statistical review triggered (Heuristic). Flags: {ratio*100:.1f}% coverage utilization, {policy_age} day policy age, {risk_zone} zone risk.",
            "recommendation": recommendation,
            "isFallback": True
        }

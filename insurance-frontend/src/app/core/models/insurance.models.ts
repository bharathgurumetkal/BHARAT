export interface Policy {
  id: string;
  policyNumber: string;
  customerId: string;
  propertyId: string;
  coverageAmount: number;
  premium: number;
  status: string;
  startDate: string | null;
  endDate: string | null;
  applicationId: string | null;
  productName: string;
}

export interface Claim {
  id: string;
  claimAmount: number;
  reason: string;
  status: string;
  createdAt: string;

  // Policy Details
  policyId: string;
  policyNumber: string;
  policyCoverageAmount: number;
  policyPremium: number;
  policyStatus: string;
  policyStartDate: string | null;
  policyEndDate: string | null;
  policyProductName: string;

  // Customer Details
  customerName: string;
  customerEmail: string;
  customerPhone: string;

  // Property Details
  propertyCategory: string;
  propertySubCategory: string;
  propertyAddress: string;
  propertyYearBuilt: number;
  propertyMarketValue: number;
  propertyRiskZone: string;
  propertyHasSecuritySystem: boolean;

  // Documents
  documents: ClaimDocument[];

  // AI Risk Analysis
  aiRiskScore?: number;
  aiRiskLevel?: string;
  aiFraudProbability?: number;
  aiExplanation?: string;
  aiRecommendation?: string;
  aiSource?: string;
}

export interface ClaimDocument {
  fileName: string;
  filePath: string;
}

export interface PolicyApplication {
  id: string;
  productId: string;
  productName: string;
  customerId: string;
  customerName: string;
  assignedAgentId: string | null;
  assignedAgentName: string | null;
  propertySubCategory: string;
  address: string;
  yearBuilt: number;
  marketValue: number;
  riskZone: string;
  hasSecuritySystem: boolean;
  requestedCoverageAmount: number;
  calculatedPremium: number;
  status: string;
  submittedAt: string;
  assignedAt: string | null;
  reviewedAt: string | null;
}

export interface PolicyProduct {
  id: string;
  name: string;
  description: string;
  propertyCategory: string;
  baseRatePercentage: number;
  maxCoverageAmount: number;
  isActive: boolean;
  createdAt: string;
}

export interface CustomerReport {
  id: string;
  userId: string;
  assignedAgentId: string | null;
  status: string;
  user?: {
    name: string;
    email: string;
  };
}

export interface ClaimsReport {
  status: number;
  count: number;
  totalAmount: number;
}

export interface RevenueReport {
  year: number;
  month: number;
  totalRevenue: number;
}

export interface AgentPerformance {
  agentId: string | null;
  policyCount: number;
  totalPremium: number;
}

export interface Commission {
  id: string;
  policyId: string;
  policyNumber: string;
  customerName: string;
  premium: number;
  commissionRate: number;
  commissionAmount: number;
  createdAt: string;
  isPaid: boolean;
}

export interface AiAnalytics {
  totalClaims: number;
  scoredClaims: number;
  highRiskClaims: number;
  mediumRiskClaims: number;
  lowRiskClaims: number;
  averageRiskScore: number;
  riskTrendMonthly: { month: string; avgRisk: number }[];
}

export interface AgentPerformanceAnalytics {
  agentId: string;
  agentName: string;
  totalPolicies: number;
  totalClaims: number;
  highRiskPercent: number;
  approvalRate: number;
  riskExposureScore: number;
}

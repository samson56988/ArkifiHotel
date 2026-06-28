export interface PlatformStaffAccount {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  displayName: string;
}

export interface PlatformLoginData {
  accessToken: string;
  expiresAtUtc: string;
  account: PlatformStaffAccount;
}

export interface PlatformBusinessSummary {
  id: string;
  businessName: string;
  slug: string | null;
  businessType: string;
  status: string;
  contactEmail: string;
  isEmailVerified: boolean;
  planName: string;
  planTier: string;
  subscriptionExpiresAt: string | null;
  createdAt: string;
  adminNotes: string | null;
}

export interface PlatformBusinessDetail extends PlatformBusinessSummary {
  firstName: string;
  lastName: string;
  phoneNumber: string;
  locationCount: number;
  roomCount: number;
  bookingCount: number;
  staffCount: number;
}

export interface PlatformDashboardStats {
  totalBusinesses: number;
  activeBusinesses: number;
  hotelCount: number;
  shortletCount: number;
  proSubscriptions: number;
  recentActivityCount: number;
}

export interface PlatformActivityLog {
  id: string;
  businessId: string;
  businessName: string;
  entityType: string;
  action: string;
  entityId: string | null;
  summary: string | null;
  actorName: string | null;
  createdAt: string;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface SubscriptionPlan {
  id: string;
  code: string;
  name: string;
  description: string | null;
  tier: string;
  billingInterval: string;
  priceAmount: number;
  currency: string;
  yearlyDiscountPercent: number | null;
  sortOrder: number;
}

export interface PlatformSubscriptionPayment {
  id: string;
  businessId: string;
  businessName: string;
  planName: string;
  amount: number;
  currency: string;
  status: string;
  paymentReference: string | null;
  createdAt: string;
}

export interface UpdatePlatformBusinessRequest {
  status?: string;
  adminNotes?: string;
}

import type { ApiResult } from './api-result.model';
import type { PagedResultDto } from './bookings.models';

export interface OrganizationAuditLogDto {
  id: string;
  userOrganizationId?: string | null;
  userDisplayName?: string | null;
  userEmail?: string | null;
  action: string;
  entityType: string;
  entityId?: string | null;
  locationId?: string | null;
  locationName?: string | null;
  summary?: string | null;
  detailsJson?: string | null;
  createdAt: string;
}

export interface ListOrganizationAuditQuery {
  locationId?: string;
  entityType?: string;
  action?: string;
  userOrganizationId?: string;
  fromUtc?: string;
  toUtc?: string;
  page?: number;
  pageSize?: number;
}

export type AuditLogsApiResponse = ApiResult<PagedResultDto<OrganizationAuditLogDto>>;

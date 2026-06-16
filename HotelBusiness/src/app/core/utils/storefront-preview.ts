import type { BusinessSocialProfileDto } from '../models/business-social-profile.models';

export interface PreviewSocialLink {
  platform: string;
  handle: string;
  url: string;
  emoji: string;
  color: string;
  followers: string | null;
}

export function buildPreviewSocialLinks(social: BusinessSocialProfileDto): PreviewSocialLink[] {
  const links: PreviewSocialLink[] = [];

  if (social.instagramUrl) {
    links.push({
      platform: 'Instagram',
      handle: social.instagramHandle?.trim() || 'Instagram',
      url: social.instagramUrl,
      emoji: '📸',
      color: '#E1306C',
      followers: social.instagramFollowers?.trim() || null,
    });
  }
  if (social.facebookUrl) {
    links.push({
      platform: 'Facebook',
      handle: social.facebookHandle?.trim() || 'Facebook',
      url: social.facebookUrl,
      emoji: '👤',
      color: '#1877F2',
      followers: social.facebookFollowers?.trim() || null,
    });
  }
  if (social.tikTokUrl) {
    links.push({
      platform: 'TikTok',
      handle: social.tikTokHandle?.trim() || 'TikTok',
      url: social.tikTokUrl,
      emoji: '🎵',
      color: '#010101',
      followers: social.tikTokFollowers?.trim() || null,
    });
  }
  if (social.xUrl) {
    links.push({
      platform: 'X',
      handle: social.xHandle?.trim() || 'X',
      url: social.xUrl,
      emoji: '𝕏',
      color: '#14171A',
      followers: social.xFollowers?.trim() || null,
    });
  }

  return links;
}

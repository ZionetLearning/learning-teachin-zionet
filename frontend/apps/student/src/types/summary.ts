export interface DateRangePeriod {
  startDate: string; // ISO 8601 date string
  endDate: string; // ISO 8601 date string
}

export interface PeriodOverviewSummary {
  totalAttempts: number;
  wordsLearned: number;
  achievementsUnlocked: number;
  practiceDays: number;
}

export interface GetPeriodOverviewResponse {
  period: DateRangePeriod;
  summary: PeriodOverviewSummary;
}

export interface GetPeriodOverviewParams {
  userId: string;
  startDate?: string; // ISO 8601 date string
  endDate?: string; // ISO 8601 date string
}

export interface UnlockedAchievement {
  achievementId: string;
  name: string;
  description: string;
  feature: string;
  unlockedAt: string; // ISO 8601 date-time string
}

export interface GetPeriodAchievementsResponse {
  unlockedInPeriod: UnlockedAchievement[];
}

export interface GetPeriodAchievementsParams {
  userId: string;
  startDate?: string; // ISO 8601 date string
  endDate?: string; // ISO 8601 date string
}

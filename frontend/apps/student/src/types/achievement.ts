export interface Achievement {
  achievementId: string;
  key: string;
  name: string;
  description: string;
  type: string;
  feature: string;
  targetCount: number;
  isUnlocked: boolean;
  unlockedAt: string | null;
}

export interface AchievementUnlockedNotification {
  achievementId: string;
  key: string;
  name: string;
  description: string;
}

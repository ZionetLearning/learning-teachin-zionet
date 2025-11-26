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

export interface TrackProgressRequest {
  userId: string;
  feature: string;
  incrementBy?: number;
}

export interface TrackProgressResponse {
  success: boolean;
  newCount: number;
  unlockedAchievements: string[];
}

export type PracticeFeature =
  | "WordCards"
  | "TypingPractice"
  | "SpeakingPractice"
  | "WordOrder"
  | "PracticeMistakes"
  | "WordCardsChallenge";

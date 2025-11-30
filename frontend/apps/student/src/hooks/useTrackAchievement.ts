import { useCallback } from "react";
import { useAuth } from "@app-providers/auth";
import { useTrackProgress } from "../api/achievements";
import type { PracticeFeature } from "../types/achievement";

export const useTrackAchievement = (feature: PracticeFeature) => {
  const { user } = useAuth();
  const trackProgressMutation = useTrackProgress();

  const track = useCallback(
    (incrementBy: number = 1) => {
      if (!user?.userId) {
        console.warn("Cannot track achievement: user not authenticated");
        return;
      }

      trackProgressMutation.mutate({
        userId: user.userId,
        feature,
        incrementBy,
      });
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [user?.userId, feature],
  );

  return {
    track,
    isTracking: trackProgressMutation.isPending,
  };
};

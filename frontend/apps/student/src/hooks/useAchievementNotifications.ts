import { useEffect, createElement } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { toast } from "react-toastify";
import { useSignalR } from "./useSignalR";
import { AchievementToast } from "../components/AchievementToast";
import type { AchievementUnlockedNotification } from "../types/achievement";

export const useAchievementNotifications = () => {
  const queryClient = useQueryClient();
  const { connection } = useSignalR();

  useEffect(
    function setupAchievementNotificationListener() {
      if (!connection) return;

      const handleAchievementUnlocked = (event: {
        eventType: string;
        payload: AchievementUnlockedNotification;
      }) => {
        if (event.eventType === "AchievementUnlocked") {
          toast.success(
            createElement(AchievementToast, { achievement: event.payload }),
            {
              autoClose: 5000,
              position: "top-right",
            },
          );

          queryClient.invalidateQueries({ queryKey: ["achievements"] });
        }
      };

      connection.on("ReceiveEvent", handleAchievementUnlocked);

      return () => {
        connection.off("ReceiveEvent", handleAchievementUnlocked);
      };
    },
    [connection, queryClient],
  );
};

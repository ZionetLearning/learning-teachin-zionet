import { useEffect } from "react";
import { useQueryClient } from "@tanstack/react-query";

import type { OnlineUserDto } from "@app-providers";
import { useSignalR } from "./useSignalR";

export const useOnlineUsersSync = () => {
  const { connection, status } = useSignalR();
  const qc = useQueryClient();

  useEffect(
    function setupSignalRSubscriptionAndEventHandlers() {
      if (!connection || status !== "connected") return;

      connection
        .invoke("SubscribeAdmin")
        .then(() => {})
        .catch((e) => {
          console.error("SubscribeAdmin failed:", e);
          console.log("Connection state:", connection.state);
        });

      const onUserOnline = (userId: string, role: string, name: string) => {
        qc.setQueryData<OnlineUserDto[] | undefined>(
          ["onlineUsers"],
          (prev) => {
            const list = prev ?? [];
            const idx = list.findIndex((u) => u.userId === userId);
            const next: OnlineUserDto = {
              userId,
              name,
              role,
              connectionsCount: idx >= 0 ? list[idx].connectionsCount : 1,
            };
            if (idx >= 0) {
              const copy = list.slice();
              copy[idx] = next;
              return copy;
            }
            return [...list, next];
          },
        );
      };

      const onUserOffline = (userId: string) => {
        qc.setQueryData<OnlineUserDto[] | undefined>(
          ["onlineUsers"],
          (prev) => {
            const list = prev ?? [];
            return list.filter((u) => u.userId !== userId);
          },
        );
      };

      const onUpdateUserConnections = (
        userId: string,
        connectionsCount: number,
      ) => {
        qc.setQueryData<OnlineUserDto[] | undefined>(
          ["onlineUsers"],
          (prev) => {
            const list = prev ?? [];
            const idx = list.findIndex((u) => u.userId === userId);
            if (idx >= 0) {
              const copy = list.slice();
              copy[idx] = { ...copy[idx], connectionsCount };
              return copy;
            }
            return list;
          },
        );
      };

      connection.on("UserOnline", onUserOnline);
      connection.on("UserOffline", onUserOffline);
      connection.on("UpdateUserConnections", onUpdateUserConnections);

      return function cleanupSignalRSubscriptionAndEventHandlers() {
        if (connection && connection.state === "Connected") {
          connection
            .invoke("UnSubscribeAdmin")
            .then(() => {})
            .catch((e) => console.warn("UnSubscribeAdmin failed", e));
        }
        connection?.off("UserOnline", onUserOnline);
        connection?.off("UserOffline", onUserOffline);
        connection?.off("UpdateUserConnections", onUpdateUserConnections);
      };
    },
    [connection, status, qc],
  );
};

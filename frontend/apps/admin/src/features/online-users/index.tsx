import { useEffect } from "react";
import { useGetOnlineUsers } from "@admin/api";
import { useSignalR } from "@admin/hooks";
import { useQueryClient } from "@tanstack/react-query";
import type { OnlineUserDto } from "@app-providers";

export const OnlineUsers = () => {
  const { data, isLoading, isError } = useGetOnlineUsers();
  const { connection, status } = useSignalR();
  const qc = useQueryClient();

  useEffect(() => {
    if (!connection || status !== "connected") {
      console.log("SignalR not ready:", { connection: !!connection, status });
      return;
    }

    console.log("Attempting to subscribe admin...");

    connection
      .invoke("SubscribeAdmin")
      .then(() => {
        console.log("SubscribeAdmin successful");
      })
      .catch((e) => {
        console.error("SubscribeAdmin failed:", e);
        console.log("Connection state:", connection.state);
      });

    const onUserOnline = (userId: string, role: string, name: string) => {
      qc.setQueryData<OnlineUserDto[] | undefined>(["onlineUsers"], (prev) => {
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
      });
    };

    const onUserOffline = (userId: string) => {
      qc.setQueryData<OnlineUserDto[] | undefined>(["onlineUsers"], (prev) => {
        const list = prev ?? [];
        return list.filter((u) => u.userId !== userId);
      });
    };

    connection.on("UserOnline", onUserOnline);
    connection.on("UserOffline", onUserOffline);

    return () => {
      if (connection && connection.state === "Connected") {
        connection
          .invoke("UnSubscribeAdmin")
          .then(() => console.log("UnSubscribeAdmin successful"))
          .catch((e) => console.warn("UnSubscribeAdmin failed", e));
      }
      connection?.off("UserOnline", onUserOnline);
      connection?.off("UserOffline", onUserOffline);
    };
  }, [connection, status, qc]);

  if (isLoading) return <div>Loading...</div>;
  if (isError) return <div>Error loading online users</div>;

  return (
    <ul>
      {data?.map((user) => (
        <li key={user.userId}>
          {user.name} - {user.role}
        </li>
      ))}
    </ul>
  );
};

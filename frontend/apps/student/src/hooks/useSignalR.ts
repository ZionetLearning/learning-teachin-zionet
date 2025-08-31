import { useContext } from "react";
import { SignalRContext } from "@app-providers/context";

export const useSignalR = () => {
  const context = useContext(SignalRContext);
  if (!context) {
    throw new Error("useSignalR must be used within SignalRProvider");
  }
  return context;
};
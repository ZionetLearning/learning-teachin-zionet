import { useContext } from "react";
import { DevToolsContext } from "../context";

export const useDevTools = () => {
  const ctx = useContext(DevToolsContext);
  if (!ctx) throw new Error("useDevTools must be used inside DevToolsProvider");
  return ctx;
};

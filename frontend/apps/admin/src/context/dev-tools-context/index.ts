import { createContext } from "react";

export type DevToolsContextValue = {
  isOpen: boolean;
  setOpen: (v: boolean) => void;
  isHebrew: boolean;
};

export const DevToolsContext = createContext<DevToolsContextValue | null>(null);

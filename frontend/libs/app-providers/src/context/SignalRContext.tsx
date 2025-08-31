import { createContext } from "react";
import type { SignalRContextType } from "@app-providers/types";

export const SignalRContext = createContext<SignalRContextType | null>(null);
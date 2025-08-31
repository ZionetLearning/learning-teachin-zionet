import { createContext } from "react";
import type { SignalRContextType } from "@/types/signalR";

export const SignalRContext = createContext<SignalRContextType | null>(null);
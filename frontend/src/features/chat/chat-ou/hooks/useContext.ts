import { useState, useCallback, useRef, useEffect } from "react";
import type { MessageContext } from "../types";
import { ContextServiceImpl } from "../services";

export interface UseContextReturn {
  currentContext: MessageContext | null;
  isContextAttached: boolean;
  attachContext: () => void;
  detachContext: () => void;
  refreshContext: () => void;
  hasSignificantContext: boolean;
  contextDisplayText: string;
}

export function useContext(): UseContextReturn {
  const [currentContext, setCurrentContext] = useState<MessageContext | null>(
    null
  );
  const [isContextAttached, setIsContextAttached] = useState(false);
  const contextServiceRef = useRef<ContextServiceImpl | null>(null);

  useEffect(() => {
    if (!contextServiceRef.current) {
      contextServiceRef.current = new ContextServiceImpl();
    }
  }, []);

  const attachContext = useCallback(() => {
    if (!contextServiceRef.current) return;

    const context = contextServiceRef.current.getCurrentPageContext();
    setCurrentContext(context);
    setIsContextAttached(true);
  }, []);

  const detachContext = useCallback(() => {
    setCurrentContext(null);
    setIsContextAttached(false);
  }, []);

  const refreshContext = useCallback(() => {
    if (!contextServiceRef.current || !isContextAttached) return;

    const context = contextServiceRef.current.getCurrentPageContext();
    setCurrentContext(context);
  }, [isContextAttached]);

  const hasSignificantContext = currentContext
    ? (contextServiceRef.current?.hasSignificantContext(currentContext) ??
      false)
    : false;

  const contextDisplayText =
    currentContext && contextServiceRef.current
      ? contextServiceRef.current.formatContextForDisplay(currentContext)
      : "";

  return {
    currentContext,
    isContextAttached,
    attachContext,
    detachContext,
    refreshContext,
    hasSignificantContext,
    contextDisplayText,
  };
}

import { useCallback } from "react";
import { useNavigate } from "react-router-dom";

/**
 * Hook for managing navigation in retry mode
 * Provides consistent navigation back to practice mistakes
 */
export const useRetryNavigation = () => {
  const navigate = useNavigate();

  const navigateToMistakes = useCallback(() => {
    navigate("/practice-mistakes");
  }, [navigate]);

  return {
    navigateToMistakes,
  };
};

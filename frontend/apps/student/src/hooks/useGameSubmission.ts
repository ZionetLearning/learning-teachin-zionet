import { useQueryClient } from "@tanstack/react-query";
import { useAuth } from "@app-providers";
import { useSubmitGameAttempt } from "@student/api";

interface UseGameSubmissionOptions {
  onSuccess?: (result: { status: string; accuracy: number }) => void;
  onError?: (error: unknown) => void;
}

/**
 * Hook for submitting game attempts and invalidating mistakes on success
 * Used in retry mode to automatically remove fixed mistakes from the list
 */
export const useGameSubmission = (options?: UseGameSubmissionOptions) => {
  const queryClient = useQueryClient();
  const { user } = useAuth();
  const studentId = user?.userId ?? "";
  const { mutateAsync: submitAttempt } = useSubmitGameAttempt();

  const submitWithInvalidation = async (
    exerciseId: string,
    givenAnswer: string[],
  ) => {
    const result = await submitAttempt({
      exerciseId,
      givenAnswer,
    });

    if (result.status === "Success") {
      queryClient.invalidateQueries({
        queryKey: ["gamesMistakes", { studentId }],
      });
      options?.onSuccess?.(result);
    } else {
      options?.onError?.(new Error("Answer was incorrect"));
    }

    return result;
  };

  return {
    submitAttempt: submitWithInvalidation,
    studentId,
  };
};

import { useMutation, useQueryClient } from "@tanstack/react-query";

/** Runs a change, then refreshes every page's data; exposes the failure message. */
export function useSaveAction<T>(action: (input: T) => Promise<unknown>, onDone?: () => void) {
  const queryClient = useQueryClient();
  const mutation = useMutation({
    mutationFn: action,
    onSuccess: () => {
      onDone?.();
      return queryClient.invalidateQueries();
    },
  });
  return { run: mutation.mutate, busy: mutation.isPending, error: mutation.error?.message ?? "", reset: mutation.reset };
}

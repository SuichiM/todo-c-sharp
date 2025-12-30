import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createTodoItem } from "../api/todos";
import { todoKeys } from "./useTodos";

/**
 * Hook to create a new todo item
 * Invalidates all todo queries on success to refetch updated data
 */
export const useCreateTodo = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: createTodoItem,
    onSuccess: () => {
      // Invalidate all todo queries to refetch updated lists
      queryClient.invalidateQueries({ queryKey: todoKeys.all });
    },
  });
};

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createTodoItem, updateTodoItem } from "../api/todos";
import { todoKeys } from "./useTodos";
import type { UpdateTodoItemRequest } from "../types/todo";

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

/**
 * Hook to update an existing todo item
 * Invalidates all todo queries on success to refetch updated data
 */
export const useUpdateTodo = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateTodoItemRequest }) =>
      updateTodoItem(id, data),
    onSuccess: () => {
      // Invalidate all todo queries to refetch updated lists
      queryClient.invalidateQueries({ queryKey: todoKeys.all });
    },
  });
};

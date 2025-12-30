import { useQuery } from "@tanstack/react-query";
import {
  fetchPendingTodos,
  fetchCompletedTodos,
  fetchOverdueTodos,
  fetchCategories,
} from "../api/todos";

/**
 * Query keys for todo-related queries
 */
export const todoKeys = {
  all: ["todos"] as const,
  pending: () => [...todoKeys.all, "pending"] as const,
  completed: () => [...todoKeys.all, "completed"] as const,
  overdue: () => [...todoKeys.all, "overdue"] as const,
  categories: () => ["categories"] as const,
};

/**
 * Hook to fetch all categories
 */
export const useCategories = () => {
  return useQuery({
    queryKey: todoKeys.categories(),
    queryFn: fetchCategories,
    staleTime: 5 * 60 * 1000, // 5 minutes (categories change infrequently)
    refetchOnWindowFocus: false,
  });
};

/**
 * Hook to fetch pending (incomplete) todos
 */
export const usePendingTodos = () => {
  return useQuery({
    queryKey: todoKeys.pending(),
    queryFn: fetchPendingTodos,
    staleTime: 30000, // 30 seconds
    refetchOnWindowFocus: true,
  });
};

/**
 * Hook to fetch completed todos
 */
export const useCompletedTodos = () => {
  return useQuery({
    queryKey: todoKeys.completed(),
    queryFn: fetchCompletedTodos,
    staleTime: 30000,
    refetchOnWindowFocus: true,
  });
};

/**
 * Hook to fetch overdue todos
 */
export const useOverdueTodos = () => {
  return useQuery({
    queryKey: todoKeys.overdue(),
    queryFn: fetchOverdueTodos,
    staleTime: 30000,
    refetchOnWindowFocus: true,
  });
};

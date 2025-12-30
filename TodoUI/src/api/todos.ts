import apiClient from "./client";
import { type TodoItem } from "../types/todo";

/**
 * Fetch all pending (incomplete) todos
 */
export const fetchPendingTodos = async (): Promise<TodoItem[]> => {
  const response = await apiClient.get<TodoItem[]>("/api/todos/pending");
  return response.data;
};

/**
 * Fetch all completed todos
 */
export const fetchCompletedTodos = async (): Promise<TodoItem[]> => {
  const response = await apiClient.get<TodoItem[]>("/api/todos/completed");
  return response.data;
};

/**
 * Fetch all overdue todos
 */
export const fetchOverdueTodos = async (): Promise<TodoItem[]> => {
  const response = await apiClient.get<TodoItem[]>("/api/todos/overdue");
  return response.data;
};

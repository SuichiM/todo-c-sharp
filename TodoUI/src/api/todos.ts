import apiClient from "./client";
import { type TodoItem, type CreateTodoItemRequest } from "../types/todo";

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

/**
 * Create a new todo item
 */
export const createTodoItem = async (
  data: CreateTodoItemRequest
): Promise<TodoItem> => {
  const response = await apiClient.post<TodoItem>("/api/todos", data);
  return response.data;
};

import apiClient from "./client";
import {
  type TodoItem,
  type CreateTodoItemRequest,
  type UpdateTodoItemRequest,
  type Category,
} from "../types/todo";

/**
 * Fetch all categories
 */
export const fetchCategories = async (): Promise<Category[]> => {
  const response = await apiClient.get<Category[]>("/api/categories");
  return response.data;
};

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

/**
 * Update an existing todo item (partial update)
 */
export const updateTodoItem = async (
  id: number,
  data: UpdateTodoItemRequest
): Promise<TodoItem> => {
  const response = await apiClient.put<TodoItem>(`/api/todos/${id}`, data);
  return response.data;
};

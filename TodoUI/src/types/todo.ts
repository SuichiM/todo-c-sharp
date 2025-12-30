/**
 * Category entity from the API
 */
export interface Category {
  id: number;
  name: string;
}

/**
 * TodoItem entity from the API
 * Matches TodoItemDto response structure
 */
export interface TodoItem {
  id: number;
  title: string;
  isCompleted: boolean;
  createdAt: string; // ISO 8601 datetime string
  dueTime: string | null; // ISO 8601 datetime string or null
  category: Category;
}

/**
 * Request payload for creating a new todo item
 * Matches CreateTodoItemRequest from API
 */
export interface CreateTodoItemRequest {
  title: string; // Required, 3-200 characters
  categoryId: number; // Required, must be > 0 and exist
  dueTime?: string; // Optional, must be future date if provided (ISO 8601)
}

/**
 * Request payload for updating a todo item
 * All fields are optional for partial updates
 * Matches UpdateTodoItemRequest from API
 */
export interface UpdateTodoItemRequest {
  title?: string; // Optional, 3-200 characters if provided
  isCompleted?: boolean; // Optional
  categoryId?: number; // Optional, must be > 0 and exist if provided
  dueTime?: string | null; // Optional, must be future date if provided (ISO 8601)
}

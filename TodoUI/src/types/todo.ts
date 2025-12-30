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

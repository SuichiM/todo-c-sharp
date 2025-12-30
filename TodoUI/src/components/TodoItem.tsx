import { Card, CardContent, Typography, Chip, Box } from "@mui/material";
import { AccessTime, Warning } from "@mui/icons-material";
import dayjs from "dayjs";
import { type TodoItem as TodoItemType } from "../types/todo";

interface TodoItemProps {
  todo: TodoItemType;
}

/**
 * TodoItem component - displays a single todo in read-only mode
 * Shows title, category, dates, and overdue indicator
 */
export const TodoItem = ({ todo }: TodoItemProps) => {
  const isOverdue =
    todo.dueTime && dayjs(todo.dueTime).isBefore(dayjs()) && !todo.isCompleted;

  return (
    <Card
      sx={{
        mb: 2,
        borderLeft: isOverdue
          ? "4px solid #d32f2f"
          : todo.isCompleted
          ? "4px solid #2e7d32"
          : undefined,
        bgcolor: isOverdue ? "#ffebee" : undefined,
      }}
    >
      <CardContent>
        <Box
          display="flex"
          justifyContent="space-between"
          alignItems="flex-start"
          mb={1}
        >
          <Typography variant="h6" component="div">
            {todo.title}
          </Typography>
          <Chip
            label={todo.category.name}
            size="small"
            color="primary"
            variant="outlined"
          />
        </Box>

        <Box display="flex" gap={2} alignItems="center" flexWrap="wrap">
          <Box display="flex" alignItems="center" gap={0.5}>
            <AccessTime fontSize="small" color="action" />
            <Typography variant="body2" color="text.secondary">
              Created: {dayjs(todo.createdAt).format("MMM D, YYYY")}
            </Typography>
          </Box>

          {todo.dueTime && (
            <Box display="flex" alignItems="center" gap={0.5}>
              {isOverdue && <Warning fontSize="small" color="error" />}
              <Typography
                variant="body2"
                color={isOverdue ? "error" : "text.secondary"}
                fontWeight={isOverdue ? 600 : 400}
              >
                Due: {dayjs(todo.dueTime).format("MMM D, YYYY h:mm A")}
                {isOverdue && " (Overdue)"}
              </Typography>
            </Box>
          )}
        </Box>

        {todo.isCompleted && (
          <Box mt={1}>
            <Chip label="Completed" size="small" color="success" />
          </Box>
        )}
      </CardContent>
    </Card>
  );
};
